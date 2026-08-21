using Estoque.Api.DTOs.Estoque;
using Estoque.Api.Exceptions;
using Faturamento.Api.Data;
using Faturamento.Api.DTOs.NotasFiscais;
using Faturamento.Api.Entities;
using Faturamento.Api.Enums;
using Faturamento.Api.Exceptions;
using Faturamento.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Faturamento.Api.Services
{
    public class NotaFiscalService : INotaFiscalService
    {

        private readonly AppDbContext _appDbContext;
        private readonly IEstoqueClient _estoqueClient;

        public NotaFiscalService(
            AppDbContext appDbContext,
            IEstoqueClient estoqueClient)
        {
            // Recebe as dependências por injeção de dependência.
            _appDbContext = appDbContext;
            _estoqueClient = estoqueClient;
        }

        public async Task<NotaFiscalResponseDTO> Cadastrar(
            NotaFiscalCriarDTO novaNotaFiscal,
            CancellationToken cancellationToken)
        {
            // Garante que cada produto apareça apenas uma vez na nota fiscal.
            ValidarProdutosDuplicados(novaNotaFiscal);

            // Cria uma tarefa para consultar cada produto no Estoque.
            var tarefas = novaNotaFiscal.Itens.Select(async item =>
            {
                // Obtém os valores obrigatórios do item após a validação do DTO.
                var produtoId = item.ProdutoId!.Value;
                var quantidade = item.Quantidade!.Value;

                // Consulta o Estoque para confirmar a existência do produto e
                // obter seus dados atuais, como código e descrição.
                var produto =
                    await _estoqueClient.BuscarProdutoPorId(
                        produtoId,
                        cancellationToken);

                // Copia os dados do Estoque para formar o item que será gravado
                // como parte da nota fiscal.
                return new ItemNotaFiscal
                {
                    ProdutoId = produto.Id,
                    CodigoProduto = produto.Codigo,
                    DescricaoProduto = produto.Descricao,
                    Quantidade = quantidade
                };
            });

            // Aguarda todas as consultas em paralelo. Se alguma falhar, a
            // criação da nota é interrompida e a exceção é propagada.
            var itens = await Task.WhenAll(tarefas);

            // Monta a entidade da nota fiscal com os itens validados.
            var notaFiscal = new NotaFiscal
            {
                Itens = itens.ToList()
            };

            // Adiciona a nova entidade no banco de dados.
            _appDbContext.NotasFiscais.Add(notaFiscal);

            // Persiste a nota e seus itens usando o token de cancelamento.
            await _appDbContext.SaveChangesAsync(cancellationToken);

            // Converte a entidade persistida para o DTO retornado pela API.
            return MapearParaResponse(notaFiscal);
        }

        public async Task<IReadOnlyList<NotaFiscalResponseDTO>> Listar(
            CancellationToken cancellationToken)
        {
            // Consulta as notas sem rastreamento, pois os dados serão apenas
            // lidos e não serão alterados nesta operação.
            return await _appDbContext.NotasFiscais
                .AsNoTracking()
                // Exibe primeiro as notas abertas e, dentro de cada status,
                // mantém a ordem crescente pelo identificador.
                .OrderBy(nota => nota.Status == StatusNotaFiscal.Aberta ? 0 : 1)
                .ThenBy(nota => nota.Id)
                // Projeta diretamente para o DTO, evitando carregar entidades
                // completas desnecessariamente.
                .Select(nota => new NotaFiscalResponseDTO
                {
                    Id = nota.Id,
                    Numero = nota.Numero,
                    Status = nota.Status.ToString(),
                    DataDeCriacao = nota.DataDeCriacao,
                    DataDeFechamento = nota.DataDeFechamento,
                    Itens = nota.Itens
                        // Mantém os itens em uma ordem estável dentro da nota.
                        .OrderBy(item => item.Id)
                        .Select(item => new ItemNotaFiscalResponseDTO
                        {
                            Id = item.Id,
                            ProdutoId = item.ProdutoId,
                            CodigoProduto = item.CodigoProduto,
                            DescricaoProduto = item.DescricaoProduto,
                            Quantidade = item.Quantidade
                        }).ToList()
                })
                // Executa a consulta no banco e retorna todos os resultados.
                .ToListAsync(cancellationToken);
        }

        public async Task<NotaFiscalResponseDTO> BuscarPorId(
            int id,
            CancellationToken cancellationToken)
        {
            // Busca a nota e carrega seus itens para montar a resposta completa.
            var notaFiscal = await _appDbContext.NotasFiscais
                .AsNoTracking()
                .Include(nota => nota.Itens)
                .FirstOrDefaultAsync(
                    nota => nota.Id == id,
                    cancellationToken);

            if (notaFiscal is null)
            {
                // Informa ao chamador que o identificador não corresponde a uma
                // nota fiscal existente.
                throw new NotaFiscalNaoEncontradaException(id);
            }

            // Retorna a entidade convertida para o formato público da API.
            return MapearParaResponse(notaFiscal);
        }

        public async Task<NotaFiscalResponseDTO> Imprimir(
            int id,
            CancellationToken cancellationToken)
        {
            // Carrega a nota fiscal e os itens necessários para realizar a baixa.
            var notaFiscal = await _appDbContext.NotasFiscais
                .Include(nota => nota.Itens)
                .FirstOrDefaultAsync(
                    nota => nota.Id == id,
                    cancellationToken);

            if (notaFiscal is null)
            {
                // Não é possível imprimir uma nota que não foi encontrada.
                throw new NotaFiscalNaoEncontradaException(id);
            }

            if (notaFiscal.Status != StatusNotaFiscal.Aberta)
            {
                // A baixa só pode ser realizada uma vez, enquanto a nota estiver
                // aberta. Notas fechadas não podem ser impressas novamente.
                throw new NotaFiscalJaFechadaException(id);
            }

            // Constrói a solicitação enviada ao Estoque usando uma chave estável.
            // Essa chave permite que a baixa seja idempotente em caso de repetição.
            var baixaEstoque = new BaixaEstoqueRequestDTO
            {
                ChaveIdempotencia = $"nota-fiscal:{notaFiscal.Id}",

                Itens = notaFiscal.Itens
                    // Transforma os itens da nota no formato esperado pelo Estoque.
                    .Select(item => new ItemBaixaEstoqueRequestDTO
                    {
                        ProdutoId = item.ProdutoId,
                        Quantidade = item.Quantidade
                    })
                    .ToList()
            };

            // Baixa todos os produtos no serviço de Estoque antes de fechar a nota.
            await _estoqueClient.BaixarEstoque(
                baixaEstoque,
                cancellationToken);

            // Atualiza o estado da nota somente depois que a baixa foi aceita.
            notaFiscal.Status = StatusNotaFiscal.Fechada;
            notaFiscal.DataDeFechamento = DateTime.UtcNow;

            // Persiste o novo status e a data de fechamento.
            await _appDbContext.SaveChangesAsync(cancellationToken);

            // Retorna a nota já fechada no formato de resposta da API.
            return MapearParaResponse(notaFiscal);
        }

        // Valida se o mesmo produto foi informado mais de uma vez na nota.
        private static void ValidarProdutosDuplicados(
            NotaFiscalCriarDTO notaFiscal)
        {
            // Agrupa os itens pelo produto e localiza o primeiro grupo repetido.
            var produtoDuplicado = notaFiscal.Itens
                .GroupBy(item => item.ProdutoId!.Value)
                .FirstOrDefault(grupo => grupo.Count() > 1);

            if (produtoDuplicado is not null)
            {
                // Interrompe o cadastro para evitar uma nota ambígua.
                throw new ProdutoDuplicadoNaNotaException(
                    produtoDuplicado.Key);
            }
        }

        // Converte uma entidade NotaFiscal para o DTO exposto pela API.
        private static NotaFiscalResponseDTO MapearParaResponse(
            NotaFiscal notaFiscal)
        {
            return new NotaFiscalResponseDTO
            {
                Id = notaFiscal.Id,
                Numero = notaFiscal.Numero,
                Status = notaFiscal.Status.ToString(),
                DataDeCriacao = notaFiscal.DataDeCriacao,
                DataDeFechamento = notaFiscal.DataDeFechamento,
                Itens = notaFiscal.Itens
                    // Ordena os itens para que a resposta tenha uma ordem estável.
                    .OrderBy(item => item.Id)
                    .Select(item => new ItemNotaFiscalResponseDTO
                    {
                        Id = item.Id,
                        ProdutoId = item.ProdutoId,
                        CodigoProduto = item.CodigoProduto,
                        DescricaoProduto = item.DescricaoProduto,
                        Quantidade = item.Quantidade
                    }).ToList()
            };
        }
    }
}