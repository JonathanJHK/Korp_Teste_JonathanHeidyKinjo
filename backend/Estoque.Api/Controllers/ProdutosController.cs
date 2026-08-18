using Estoque.Api.DTOs;
using Estoque.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Estoque.Api.Controllers
{
    [ApiController]
    [Route("api/produtos")]
    public class ProdutosController : ControllerBase
    {
        private readonly IProdutoService _produtoService;

        public ProdutosController(IProdutoService produtoService)
        {
            _produtoService = produtoService;
        }

        [HttpPost]
        [ProducesResponseType<ProdutoResponseDTO>(
            StatusCodes.Status201Created)]
        [ProducesResponseType<ValidationProblemDetails>(
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(
            StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ProdutoResponseDTO>> Cadastrar(
            [FromBody] ProdutoCriarDTO novoProduto,
            CancellationToken cancellationToken)
        {
            var produto = await _produtoService.Cadastrar(
                novoProduto,
                cancellationToken);

            // Retorna o produto criado com o status 201,
            return CreatedAtAction(
                nameof(BuscarPorId),
                new { id = produto.Id },
                produto);
        }

        [HttpGet]
        [ProducesResponseType<IReadOnlyList<ProdutoResponseDTO>>(
            StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ProdutoResponseDTO>>> Listar(
            CancellationToken cancellationToken)
        {
            var produtos = await _produtoService.Listar(cancellationToken);

            return Ok(produtos);
        }

        [HttpGet("{id:int:min(1)}")]
        [ProducesResponseType<ProdutoResponseDTO>(
            StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProdutoResponseDTO>> BuscarPorId(
        int id,
        CancellationToken cancellationToken)
        {
            var produto = await _produtoService.BuscarPorId(
                id,
                cancellationToken);

            return Ok(produto);
        }
    }
}