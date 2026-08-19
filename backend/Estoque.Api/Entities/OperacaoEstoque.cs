namespace Estoque.Api.Entities
{
    public class OperacaoEstoque
    {
        public string ChaveIdempotencia { get; set; } = string.Empty;

        public DateTime DataDeProcessamento { get; set; } = DateTime.UtcNow;
    }
}