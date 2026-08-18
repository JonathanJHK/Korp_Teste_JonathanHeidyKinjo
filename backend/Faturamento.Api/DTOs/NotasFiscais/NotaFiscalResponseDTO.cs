namespace Faturamento.Api.DTOs.NotasFiscais
{
    public class NotaFiscalResponseDTO
    {
        public int Id { get; set; }

        public long Numero { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime DataDeCriacao { get; set; }

        public DateTime? DataDeFechamento { get; set; }

        public List<ItemNotaFiscalResponseDTO> Itens { get; set; } = [];
    }
}