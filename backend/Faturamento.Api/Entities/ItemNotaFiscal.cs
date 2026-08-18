namespace Faturamento.Api.Entities
{
    public class ItemNotaFiscal
    {
        public int Id { get; set; }

        public int NotaFiscalId { get; set; }

        // Copia do produto da api Estoque
        public int ProdutoId { get; set; }

        public string CodigoProduto { get; set; } = string.Empty;

        public string DescricaoProduto { get; set; } = string.Empty;

        public int Quantidade { get; set; }
    }
}