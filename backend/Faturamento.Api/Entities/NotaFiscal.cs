using Faturamento.Api.Enums;

namespace Faturamento.Api.Entities
{
    public class NotaFiscal
    {
        public int Id { get; set; }

        public long Numero { get; set; }

        public StatusNotaFiscal Status { get; set; } = StatusNotaFiscal.Aberta;

        public DateTime DataDeCriacao { get; set; } = DateTime.UtcNow;

        public DateTime? DataDeFechamento { get; set; }

        public ICollection<ItemNotaFiscal> Itens { get; set; } = new List<ItemNotaFiscal>();
    }
}