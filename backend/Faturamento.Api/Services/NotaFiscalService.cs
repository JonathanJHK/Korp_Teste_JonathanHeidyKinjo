using Faturamento.Api.Data;
using Faturamento.Api.DTOs.NotasFiscais;
using Faturamento.Api.Entities;
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
            _appDbContext = appDbContext;
            _estoqueClient = estoqueClient;
        }

        public async Task<NotaFiscalResponseDTO> Cadastrar(
            NotaFiscalCriarDTO novaNotaFiscal,
            CancellationToken cancellationToken)
        {
            // Validar se o produto foi informado mais de uma vez
            ValidarProdutosDuplicados(novaNotaFiscal);

            // Itera sobre a coleção de itens da novaNotaFiscal
            var tarefas = novaNotaFiscal.Itens.Select(async item =>
            {
                // Obtém o produtoId e quantidade do item
                var produtoId = item.ProdutoId!.Value;
                var quantidade = item.Quantidade!.Value;

                // Busca o produto no estoque usando o produtoId e cancellationToken
                var produto =
                    await _estoqueClient.BuscarProdutoPorId(
                        produtoId,
                        cancellationToken);

                // Retorna um novo objeto ItemNotaFiscal com as informações do produto e quantidade
                return new ItemNotaFiscal
                {
                    ProdutoId = produto.Id,
                    CodigoProduto = produto.Codigo,
                    DescricaoProduto = produto.Descricao,
                    Quantidade = quantidade
                };
            });

            // Executar as tarefas em paralelo
            // E aguarda a conclusão de todas as tarefas
            var itens = await Task.WhenAll(tarefas);

            // Cria uma nova nota fiscal com os itens
            var notaFiscal = new NotaFiscal
            {
                Itens = itens.ToList()
            };

            // Adiciona a nova nota fiscal ao contexto
            _appDbContext.NotasFiscais.Add(notaFiscal);

            await _appDbContext.SaveChangesAsync(cancellationToken);

            return MapearParaResponse(notaFiscal);
        }

        public async Task<IReadOnlyList<NotaFiscalResponseDTO>> Listar(
            CancellationToken cancellationToken)
        {
            return await _appDbContext.NotasFiscais
                .AsNoTracking()
                .OrderByDescending(nota => nota.Numero)
                .Select(nota => new NotaFiscalResponseDTO
                {
                    Id = nota.Id,
                    Numero = nota.Numero,
                    Status = nota.Status.ToString(),
                    DataDeCriacao = nota.DataDeCriacao,
                    DataDeFechamento = nota.DataDeFechamento,
                    Itens = nota.Itens
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
                .ToListAsync(cancellationToken);
        }

        public async Task<NotaFiscalResponseDTO> BuscarPorId(
            int id,
            CancellationToken cancellationToken)
        {
            var notaFiscal = await _appDbContext.NotasFiscais
                .AsNoTracking()
                .Include(nota => nota.Itens)
                .FirstOrDefaultAsync(
                    nota => nota.Id == id,
                    cancellationToken);

            if (notaFiscal is null)
            {
                throw new NotaFiscalNaoEncontradaException(id);
            }

            return MapearParaResponse(notaFiscal);
        }

        // Validar se o produto foi informado mais de uma vez
        private static void ValidarProdutosDuplicados(
            NotaFiscalCriarDTO notaFiscal)
        {
            // Verificar se o produto foi informado mais de uma vez
            var produtoDuplicado = notaFiscal.Itens
                .GroupBy(item => item.ProdutoId!.Value)
                .FirstOrDefault(grupo => grupo.Count() > 1);

            if (produtoDuplicado is not null)
            {
                // Se o produto for informado mais de uma vez, lançar uma exceção
                throw new ProdutoDuplicadoNaNotaException(
                    produtoDuplicado.Key);
            }
        }

        // Mapear para NotaFiscalResponseDTO
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