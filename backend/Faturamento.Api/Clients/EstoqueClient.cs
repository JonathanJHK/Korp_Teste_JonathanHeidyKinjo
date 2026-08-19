using System.Net;
using System.Text.Json;
using Estoque.Api.DTOs.Estoque;
using Estoque.Api.Exceptions;
using Faturamento.Api.DTOs.Estoque;
using Faturamento.Api.Exceptions;
using Faturamento.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Faturamento.Api.Clients
{
    public class EstoqueClient : IEstoqueClient
    {
        // Cliente HTTP usado para realizar as chamadas ao serviço de Estoque.
        private readonly HttpClient _httpClient;

        // Logger usado para registrar respostas inesperadas do serviço remoto.
        private readonly ILogger<EstoqueClient> _logger;

        public EstoqueClient(HttpClient httpClient, ILogger<EstoqueClient> logger)
        {
            // As dependências são fornecidas pelo sistema de injeção de dependência.
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ProdutoEstoqueResponseDTO> BuscarProdutoPorId(
            int produtoId,
            CancellationToken cancellationToken)
        {
            try
            {
                // Consulta o produto no serviço de Estoque e encaminha o token
                // para permitir o cancelamento da requisição.
                var response = await _httpClient.GetAsync(
                    $"api/produtos/{produtoId}",
                    cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    // Traduz o 404 da API de Estoque para uma exceção específica
                    // do domínio do Faturamento.
                    throw new ProdutoEstoqueNaoEncontradoException(produtoId);
                }

                if (!response.IsSuccessStatusCode)
                {
                    // Qualquer status diferente de sucesso ou 404 indica uma
                    // falha inesperada na comunicação com o Estoque.
                    _logger.LogWarning(
                        "O Estoque respondeu com o status {StatusCode} ao consultar o produto {ProdutoId}.",
                        response.StatusCode,
                        produtoId);

                    throw new ServicoEstoqueIndisponivelException(
                        "O serviço de Estoque não conseguiu processar a solicitação.");
                }

                // Converte o corpo JSON da resposta para o DTO esperado pelo
                // restante da aplicação.
                var produto =
                    await response.Content.ReadFromJsonAsync<ProdutoEstoqueResponseDTO>(
                        cancellationToken);

                if (produto is null)
                {
                    // Uma resposta bem-sucedida sem conteúdo válido é tratada
                    // como indisponibilidade ou falha de contrato da API.
                    throw new ServicoEstoqueIndisponivelException(
                        "O serviço de Estoque retornou uma resposta inválida.");
                }

                return produto;
            }
            // Essas exceções já representam falhas conhecidas e devem chegar
            // ao chamador sem serem substituídas por outra exceção.
            catch (ProdutoEstoqueNaoEncontradoException)
            {
                throw;
            }
            catch (ServicoEstoqueIndisponivelException)
            {
                throw;
            }
            // O filtro diferencia timeout de cancelamento solicitado pelo
            // chamador. Apenas o timeout é convertido em indisponibilidade.
            catch (OperationCanceledException exception)
                when (!cancellationToken.IsCancellationRequested)
            {
                throw new ServicoEstoqueIndisponivelException(
                    "O tempo limite de comunicação com o Estoque foi excedido.",
                    exception);
            }
            // Falhas de rede são convertidas para uma exceção de infraestrutura
            // compreensível para o serviço de Faturamento.
            catch (HttpRequestException exception)
            {
                throw new ServicoEstoqueIndisponivelException(
                    "Não foi possível estabelecer comunicação com o Estoque.",
                    exception);
            }
        }

        public async Task<BaixaEstoqueResponseDTO> BaixarEstoque(
            BaixaEstoqueRequestDTO dto,
            CancellationToken cancellationToken)
        {
            try
            {
                // Envia a solicitação de baixa para o endpoint do Estoque.
                var response = await _httpClient.PostAsJsonAsync(
                    "api/estoque/baixas",
                    dto,
                    cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    // Lê a mensagem detalhada da API para preservar o motivo
                    // específico pelo qual um produto não foi encontrado.
                    var detalhe = await ObterDetalheDoErro(
                        response,
                        cancellationToken);

                    throw new ProdutoEstoqueNaoEncontradoException(detalhe ?? "Um dos produtos não foi encontrado no Estoque.");
                }

                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    // Conflitos representam uma rejeição da operação, como
                    // saldo insuficiente ou outra regra de negócio violada.
                    var detalhe = await ObterDetalheDoErro(
                        response,
                        cancellationToken);

                    throw new OperacaoEstoqueRejeitadaException(detalhe ?? "A operação de baixa foi rejeitada pelo Estoque.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    // Registra e converte outros status de erro em uma exceção
                    // genérica de indisponibilidade do Estoque.
                    _logger.LogWarning(
                        "O Estoque respondeu com o status {StatusCode} durante a baixa.",
                        response.StatusCode);

                    throw new ServicoEstoqueIndisponivelException("O serviço de Estoque não conseguiu processar a baixa.");
                }

                // Desserializa a resposta de sucesso para o DTO da baixa.
                var resultado = await response.Content
                    .ReadFromJsonAsync<BaixaEstoqueResponseDTO>(
                        cancellationToken);

                if (resultado is null)
                {
                    // Impede que uma resposta vazia ou inválida avance para o
                    // processamento do faturamento.
                    throw new ServicoEstoqueIndisponivelException("O serviço de Estoque retornou uma resposta inválida.");
                }

                return resultado;
            }
            // Mantém as exceções de negócio sem alterar sua mensagem ou tipo.
            catch (ProdutoEstoqueNaoEncontradoException)
            {
                throw;
            }
            catch (OperacaoEstoqueRejeitadaException)
            {
                throw;
            }
            catch (ServicoEstoqueIndisponivelException)
            {
                throw;
            }
            // Converte apenas cancelamentos causados por timeout; um
            // cancelamento explícito do chamador continua sendo propagado.
            catch (OperationCanceledException exception)
                when (!cancellationToken.IsCancellationRequested)
            {
                throw new ServicoEstoqueIndisponivelException(
                    "O tempo limite da operação de baixa foi excedido.",
                    exception);
            }
            // Converte erros de conexão ou transporte em uma falha de serviço.
            catch (HttpRequestException exception)
            {
                throw new ServicoEstoqueIndisponivelException(
                    "Não foi possível estabelecer comunicação com o Estoque.",
                    exception);
            }
        }

        private static async Task<string?> ObterDetalheDoErro(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            try
            {
                // Tenta interpretar o corpo como ProblemDetails, formato padrão
                // usado pela API para transportar detalhes de erros HTTP.
                var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);

                return problemDetails?.Detail;
            }
            catch (JsonException)
            {
                // Retorna nulo quando o corpo não contém um JSON válido.
                return null;
            }
            catch (NotSupportedException)
            {
                // Retorna nulo quando o conteúdo possui um formato não suportado
                // pelo desserializador configurado.
                return null;
            }
        }
    }
}