using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Estoque.Api.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IProblemDetailsService _problemDetailsService;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetailsService)
        {
            // Inicializa o logger para registrar informações sobre exceções
            _logger = logger;
            // Inicializa o serviço de detalhes do problema
            _problemDetailsService = problemDetailsService;
        }

        // Implementação do método TryHandleAsync para lidar com exceções globalmente
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Mapeia diferentes tipos de exceções para códigos de status HTTP, títulos e detalhes apropriados
            var (statusCode, titulo, detalhe) = exception switch
            {
                ProdutoNaoEncontradoException => (
                    StatusCodes.Status404NotFound,
                    "Produto não encontrado",
                    exception.Message),

                CodigoProdutoDuplicadoException => (
                    StatusCodes.Status409Conflict,
                    "Código de produto duplicado",
                    exception.Message),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Erro interno do servidor",
                    "Ocorreu um erro inesperado ao processar a solicitação.")
            };

            // Registra o erro no logger de acordo com o código de status
            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "Erro inesperado durante o processamento de {Metodo} {Caminho}",
                    httpContext.Request.Method,
                    httpContext.Request.Path);
            }
            else
            {
                _logger.LogWarning(
                    exception,
                    "Falha de negócio durante o processamento de {Metodo} {Caminho}",
                    httpContext.Request.Method,
                    httpContext.Request.Path);
            }

            httpContext.Response.StatusCode = statusCode;

            // Cria um objeto ProblemDetails para fornecer informações detalhadas sobre o erro
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = titulo,
                Detail = detalhe,
                Instance = httpContext.Request.Path
            };

            // Adiciona informações de rastreamento ao objeto ProblemDetails
            problemDetails.Extensions["traceId"] =
                httpContext.TraceIdentifier;

            // Tenta escrever o detalhe do problema no corpo da resposta
            var foiEscrito = await _problemDetailsService.TryWriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = httpContext,
                    Exception = exception,
                    ProblemDetails = problemDetails
                });

            // Se não foi possível escrever o detalhe do problema, escreve manualmente no corpo da resposta
            if (!foiEscrito)
            {
                await httpContext.Response.WriteAsJsonAsync(
                    problemDetails,
                    cancellationToken);
            }

            return true;
        }
    }
}