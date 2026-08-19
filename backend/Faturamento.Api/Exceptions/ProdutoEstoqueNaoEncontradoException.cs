namespace Faturamento.Api.Exceptions;

public class ProdutoEstoqueNaoEncontradoException : Exception
{
    public ProdutoEstoqueNaoEncontradoException(int produtoId)
        : base(
            $"O produto com o ID {produtoId} não foi encontrado no Estoque.")
    {
    }

    public ProdutoEstoqueNaoEncontradoException(string mensagem) : base(mensagem)
    {
    }
}