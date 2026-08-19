namespace Faturamento.Api.Exceptions
{
    public class ProdutoEstoqueNaoEncontradoException(int produtoId) : Exception(
        $"O produto com o ID {produtoId} não foi encontrado no Estoque."
        );
}