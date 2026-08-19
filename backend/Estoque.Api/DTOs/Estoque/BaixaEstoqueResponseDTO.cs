namespace Estoque.Api.DTOs.Estoque
{
    public class BaixaEstoqueResponseDTO
    {
        public string ChaveIdempotencia { get; set; } = string.Empty;

        public bool Processada { get; set; }

        public bool JaProcessadaAnteriormente { get; set; }

        public DateTime DataDeProcessamento { get; set; }
    }
}