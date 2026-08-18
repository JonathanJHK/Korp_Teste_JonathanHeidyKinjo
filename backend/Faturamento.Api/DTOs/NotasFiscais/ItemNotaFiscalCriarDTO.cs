using System.ComponentModel.DataAnnotations;

namespace Faturamento.Api.DTOs.NotasFiscais
{
    public class ItemNotaFiscalCriarDTO
    {
        [Required(ErrorMessage = "O produto é obrigatório.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "O identificador do produto deve ser maior que zero.")]
        public int? ProdutoId { get; set; }

        [Required(ErrorMessage = "A quantidade é obrigatória.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "A quantidade deve ser maior que zero.")]
        public int? Quantidade { get; set; }
    }
}