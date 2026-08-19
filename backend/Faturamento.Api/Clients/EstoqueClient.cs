using System.Net;
using Faturamento.Api.DTOs.Estoque;
using Faturamento.Api.Exceptions;
using Faturamento.Api.Interfaces;

namespace Faturamento.Api.Clients
{
    public class EstoqueClient : IEstoqueClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EstoqueClient> _logger;

        public EstoqueClient(HttpClient httpClient, ILogger<EstoqueClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ProdutoEstoqueResponseDTO> BuscarProdutoPorId(
            int produtoId,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"api/produtos/{produtoId}",
                    cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new ProdutoEstoqueNaoEncontradoException(produtoId);
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "O Estoque respondeu com o status {StatusCode} ao consultar o produto {ProdutoId}.",
                        response.StatusCode,
                        produtoId);

                    throw new ServicoEstoqueIndisponivelException(
                        "O serviço de Estoque não conseguiu processar a solicitação.");
                }

                // O conteúdo da resposta contém um ProdutoEstoqueResponseDTO, que contém os dados do produto
                var produto =
                    await response.Content.ReadFromJsonAsync<ProdutoEstoqueResponseDTO>(
                        cancellationToken);

                if (produto is null)
                {
                    throw new ServicoEstoqueIndisponivelException(
                        "O serviço de Estoque retornou uma resposta inválida.");
                }

                return produto;
            }
            catch (ProdutoEstoqueNaoEncontradoException)
            {
                throw;
            }
            catch (ServicoEstoqueIndisponivelException)
            {
                throw;
            }
            catch (OperationCanceledException exception)
                when (!cancellationToken.IsCancellationRequested)
            {
                throw new ServicoEstoqueIndisponivelException(
                    "O tempo limite de comunicação com o Estoque foi excedido.",
                    exception);
            }
            catch (HttpRequestException exception)
            {
                throw new ServicoEstoqueIndisponivelException(
                    "Não foi possível estabelecer comunicação com o Estoque.",
                    exception);
            }
        }
    }
}