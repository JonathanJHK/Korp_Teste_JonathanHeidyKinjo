using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Estoque.Api.Data;
using Estoque.Api.DTOs.Estoque;
using Estoque.Api.Entities;
using Estoque.Api.Exceptions;
using Estoque.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Estoque.Api.Services
{
    public class EstoqueService : IEstoqueService
    {
        private readonly AppDbContext _appDbContext;

        public EstoqueService(AppDbContext appDBContext)
        {
            _appDbContext = appDBContext;
        }

        public async Task<BaixaEstoqueResponseDTO> Baixar(
            BaixaEstoqueDTO novaBaixaEstoque,
            CancellationToken cancellationToken)
        {
            // Remove espaços extras para que a mesma chave seja reconhecida
            // de forma consistente, independentemente da formatação recebida.
            var chaveIdempotencia = novaBaixaEstoque.ChaveIdempotencia.Trim();

            // Impede que o mesmo produto apareça mais de uma vez na baixa.
            // Isso evita ambiguidades e mantém uma única operação por produto.
            ValidarProdutosDuplicados(novaBaixaEstoque);

            try
            {
                // Todas as alterações desta operação serão confirmadas ou
                // desfeitas juntas, garantindo atomicidade.
                await using var transaction = await _appDbContext.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    // Verifica se a chave já foi processada anteriormente.
                    // AsNoTracking evita que essa entidade seja acompanhada
                    // pelo Entity Framework, pois ela será apenas consultada.
                    var operacaoExistente =
                        await _appDbContext.OperacoesEstoque
                            .AsNoTracking()
                            .FirstOrDefaultAsync(
                                operacao =>
                                    operacao.ChaveIdempotencia ==
                                    chaveIdempotencia,
                                cancellationToken);

                    if (operacaoExistente is not null)
                    {
                        // Nenhuma alteração é necessária quando a operação já
                        // foi processada; apenas encerra a transação atual.
                        await transaction.CommitAsync(cancellationToken);

                        // Retorna uma resposta idempotente sem baixar o estoque
                        // novamente.
                        return CriarRespostaJaProcessada(operacaoExistente);
                    }

                    // Registra a operação antes de alterar o estoque.
                    var operacao = new OperacaoEstoque
                    {
                        ChaveIdempotencia = chaveIdempotencia
                    };

                    _appDbContext.OperacoesEstoque.Add(operacao);

                    // Persiste o registro da operação dentro da mesma transação
                    // que será usada para atualizar os produtos.
                    await _appDbContext.SaveChangesAsync(cancellationToken);

                    // Processa os produtos sempre na mesma ordem. Isso reduz a
                    // possibilidade de deadlocks em requisições concorrentes.
                    var itensOrdenados = novaBaixaEstoque.Itens
                        .OrderBy(item => item.ProdutoId!.Value)
                        .ToList();

                    foreach (var item in itensOrdenados)
                    {
                        // O operador ! indica que esses valores deveriam ter
                        // sido validados antes desta etapa;
                        var produtoId = item.ProdutoId!.Value;
                        var quantidade = item.Quantidade!.Value;

                        // Atualiza diretamente no banco somente se o produto
                        // existir e tiver saldo suficiente. A condição e a
                        // subtração são executadas atomicamente pelo banco.
                        var linhasAlteradas = await _appDbContext.Produtos
                                .Where(produto =>
                                    produto.Id == produtoId &&
                                    produto.Saldo >= quantidade)
                                .ExecuteUpdateAsync(
                                    atualizacao => atualizacao
                                        .SetProperty(
                                            produto => produto.Saldo,
                                            produto => produto.Saldo - quantidade),
                                    cancellationToken);

                        if (linhasAlteradas == 0)
                        {
                            // Nenhuma linha foi atualizada. A consulta abaixo
                            // diferencia produto inexistente de saldo insuficiente.
                            await ValidarFalhaNaBaixaAsync(
                                produtoId,
                                quantidade,
                                cancellationToken);
                        }
                    }

                    // Confirma as alterações realizadas na transação.
                    await transaction.CommitAsync(cancellationToken);

                    // Todos os itens foram baixados com sucesso; confirma que
                    // a operação foi processada nesta requisição.
                    return new BaixaEstoqueResponseDTO
                    {
                        ChaveIdempotencia = operacao.ChaveIdempotencia,
                        Processada = true,
                        JaProcessadaAnteriormente = false,
                        DataDeProcessamento = operacao.DataDeProcessamento
                    };
                }
                catch
                {
                    // Qualquer erro em qualquer item desfaz também as baixas
                    // realizadas anteriormente e o registro da operação.
                    await transaction.RollbackAsync(cancellationToken);

                    throw;
                }
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation
                })
            {
                // Duas requisições simultâneas podem consultar a chave antes
                // de qualquer uma inserir o registro. A restrição UNIQUE faz
                // uma delas falhar, e esta requisição reaproveita o resultado
                // da operação que venceu a disputa.
                _appDbContext.ChangeTracker.Clear();

                // Limpa as entidades rastreadas antes de consultar novamente,
                // evitando conflitos com a tentativa de inserção que falhou.
                var operacaoExistente =
                    await _appDbContext.OperacoesEstoque
                        .AsNoTracking()
                        .FirstAsync(
                            operacao => operacao.ChaveIdempotencia == chaveIdempotencia,
                            cancellationToken);

                return CriarRespostaJaProcessada(operacaoExistente);
            }
        }

        private async Task ValidarFalhaNaBaixaAsync(
            int produtoId,
            int quantidade,
            CancellationToken cancellationToken)
        {
            var produto = await _appDbContext.Produtos
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    produto => produto.Id == produtoId,
                    cancellationToken);

            if (produto is null)
            {
                throw new ProdutoNaoEncontradoException(produtoId);
            }

            throw new SaldoInsuficienteException(
                produto.Codigo,
                produto.Saldo,
                quantidade
                );
        }

        private static void ValidarProdutosDuplicados(BaixaEstoqueDTO dto)
        {
            var produtoDuplicado = dto.Itens
                .GroupBy(item => item.ProdutoId!.Value)
                .FirstOrDefault(grupo => grupo.Count() > 1);

            if (produtoDuplicado is not null)
            {
                throw new ProdutoDuplicadoNaBaixaException(produtoDuplicado.Key);
            }
        }

        private static BaixaEstoqueResponseDTO CriarRespostaJaProcessada(OperacaoEstoque operacao)
        {
            return new BaixaEstoqueResponseDTO
            {
                ChaveIdempotencia = operacao.ChaveIdempotencia,
                Processada = true,
                JaProcessadaAnteriormente = true,
                DataDeProcessamento = operacao.DataDeProcessamento
            };
        }
    }
}