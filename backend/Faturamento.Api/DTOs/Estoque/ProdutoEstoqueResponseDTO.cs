namespace Faturamento.Api.DTOs.Estoque
{
    public class ProdutoEstoqueResponseDTO
    {
        public int Id { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public int Saldo { get; set; }
    }
}