namespace Estoque.Api.Exceptions
{
    public class ProdutoNaoEncontradoException(int id)
        : Exception($"Produto com o ID {id} não foi encontrado.");
}