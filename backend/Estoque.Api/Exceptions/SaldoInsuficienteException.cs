namespace Estoque.Api.Exceptions
{
    public class SaldoInsuficienteException(
        int produtoId,
        int saldoDisponivel,
        int quantidadeSolicitada) : Exception(
            $"O produto com o ID {produtoId} possui saldo {saldoDisponivel}, " +
            $"mas foram solicitadas {quantidadeSolicitada} unidades.");
}