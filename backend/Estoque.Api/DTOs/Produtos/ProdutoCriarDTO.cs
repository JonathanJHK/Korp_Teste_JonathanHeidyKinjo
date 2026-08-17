using System.ComponentModel.DataAnnotations;

namespace Estoque.Api.DTOs
{
    public class ProdutoCriarDTO
    {
        [Required(ErrorMessage = "O código do produto é obrigatório.")]
        [MaxLength(50, ErrorMessage = "O código deve possuir no máximo 50 caracteres.")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição do produto é obrigatória.")]
        [MaxLength(200, ErrorMessage = "A descrição deve possuir no máximo 200 caracteres.")]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "O saldo do produto é obrigatório.")]
        [Range(0, int.MaxValue, ErrorMessage = "O saldo não pode ser negativo.")]
        public int? Saldo { get; set; }
    }
}