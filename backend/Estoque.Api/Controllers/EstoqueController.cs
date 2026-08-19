using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Estoque.Api.DTOs.Estoque;
using Estoque.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Estoque.Api.Controllers
{
    [ApiController]
    [Route("api/estoque")]
    public class EstoqueController : ControllerBase
    {
        private readonly IEstoqueService _estoqueService;

        public EstoqueController(
            IEstoqueService estoqueService)
        {
            _estoqueService = estoqueService;
        }

        [HttpPost("baixas")]
        [ProducesResponseType<BaixaEstoqueResponseDTO>(
       StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(
       StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(
       StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(
       StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BaixaEstoqueResponseDTO>> Baixar(
            [FromBody] BaixaEstoqueDTO novaBaixaEstoque,
            CancellationToken cancellationToken)
        {
            var resultado = await _estoqueService.Baixar(
                novaBaixaEstoque,
                cancellationToken);

            return Ok(resultado);
        }
    }
}