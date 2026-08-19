using Estoque.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Faturamento.Api.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IProblemDetailsService _problemDetailsService;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IProblemDetailsService problemDetailsService)
        {
            _logger = logger;
            _problemDetailsService = problemDetailsService;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, titulo, detalhe) = exception switch
            {
                NotaFiscalNaoEncontradaException => (
                    StatusCodes.Status404NotFound,
                    "Nota fiscal não encontrada",
                    exception.Message),

                ProdutoEstoqueNaoEncontradoException => (
                    StatusCodes.Status404NotFound,
                    "Produto não encontrado",
                    exception.Message),

                ProdutoDuplicadoNaNotaException => (
                    StatusCodes.Status409Conflict,
                    "Produto duplicado na nota fiscal",
                    exception.Message),

                ServicoEstoqueIndisponivelException => (
                    StatusCodes.Status503ServiceUnavailable,
                    "Serviço de Estoque indisponível",
                    exception.Message),

                NotaFiscalJaFechadaException => (
                    StatusCodes.Status409Conflict,
                    "Nota fiscal já fechada",
                    exception.Message),

                OperacaoEstoqueRejeitadaException => (
                    StatusCodes.Status409Conflict,
                    "Operação de estoque rejeitada",
                    exception.Message),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Erro interno do servidor",
                    "Ocorreu um erro inesperado ao processar a solicitação.")
            };

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "Erro durante o processamento de {Metodo} {Caminho}",
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

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = titulo,
                Detail = detalhe,
                Instance = httpContext.Request.Path
            };

            // Adicionando o traceId ao problema
            problemDetails.Extensions["traceId"] =
                httpContext.TraceIdentifier;

            var foiEscrito =
                await _problemDetailsService.TryWriteAsync(
                    new ProblemDetailsContext
                    {
                        HttpContext = httpContext,
                        Exception = exception,
                        ProblemDetails = problemDetails
                    });

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