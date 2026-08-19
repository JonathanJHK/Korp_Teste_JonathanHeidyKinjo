namespace Estoque.Api.Exceptions
{
    public class ProdutoDuplicadoNaBaixaException(int produtoId) : Exception(
            $"O produto com o ID {produtoId} foi informado mais de uma vez na baixa.");
}