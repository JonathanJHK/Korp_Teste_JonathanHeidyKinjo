using Estoque.Api.DTOs;

namespace Estoque.Api.Interfaces
{
    public interface IProdutoService
    {
        // Cadastrar um novo produto, CancellationToken é usado para cancelar a operação se necessário (por exemplo, se o cliente desconectar antes da conclusão).
        Task<ProdutoResponseDTO> Cadastrar(
            ProdutoCriarDTO dto,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<ProdutoResponseDTO>> Listar(
            CancellationToken cancellationToken);

        Task<ProdutoResponseDTO> BuscarPorId(
            int id,
            CancellationToken cancellationToken);
    }
}