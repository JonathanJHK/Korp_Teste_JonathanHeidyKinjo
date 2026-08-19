using Faturamento.Api.DTOs.NotasFiscais;
using Faturamento.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Faturamento.Api.Controllers
{
    [ApiController]
    [Route("api/notas-fiscais")]
    public class NotasFiscaisController : ControllerBase
    {
        private readonly INotaFiscalService _notaFiscalService;

        public NotasFiscaisController(
            INotaFiscalService notaFiscalService)
        {
            _notaFiscalService = notaFiscalService;
        }



        [HttpPost]
        [ProducesResponseType<NotaFiscalResponseDTO>(
            StatusCodes.Status201Created)]
        [ProducesResponseType<ValidationProblemDetails>(
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(
            StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(
            StatusCodes.Status409Conflict)]
        [ProducesResponseType<ProblemDetails>(
            StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<NotaFiscalResponseDTO>> Cadastrar(
            [FromBody] NotaFiscalCriarDTO novaNotaFiscal,
            CancellationToken cancellationToken)
        {
            var notaFiscal =
                await _notaFiscalService.Cadastrar(
                    novaNotaFiscal,
                    cancellationToken);

            return CreatedAtAction(
                nameof(BuscarPorId),
                new { id = notaFiscal.Id },
                notaFiscal);
        }

        [HttpGet]
        [ProducesResponseType<
            IReadOnlyList<NotaFiscalResponseDTO>>(
            StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<NotaFiscalResponseDTO>>> Listar(
            CancellationToken cancellationToken)
        {
            var notasFiscais =
                await _notaFiscalService.Listar(
                    cancellationToken);

            return Ok(notasFiscais);
        }

        [HttpGet("{id:int:min(1)}")]
        [ProducesResponseType<NotaFiscalResponseDTO>(
            StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(
            StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NotaFiscalResponseDTO>> BuscarPorId(
            int id,
            CancellationToken cancellationToken)
        {
            var notaFiscal =
                await _notaFiscalService.BuscarPorId(
                    id,
                    cancellationToken);

            return Ok(notaFiscal);
        }

    }
}