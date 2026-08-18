namespace Faturamento.Api.DTOs.NotasFiscais
{
    public class ItemNotaFiscalResponseDTO
    {
        public int Id { get; set; }

        public int ProdutoId { get; set; }

        public string CodigoProduto { get; set; } = string.Empty;

        public string DescricaoProduto { get; set; } = string.Empty;

        public int Quantidade { get; set; }
    }
}