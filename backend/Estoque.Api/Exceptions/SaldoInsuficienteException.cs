namespace Estoque.Api.Exceptions
{
    public class SaldoInsuficienteException(
        string produtoCodigo,
        int saldoDisponivel,
        int quantidadeSolicitada) : Exception(
            $"O produto com o código {produtoCodigo} possui saldo {saldoDisponivel}, " +
            $"mas foram solicitadas {quantidadeSolicitada} unidades.");
}