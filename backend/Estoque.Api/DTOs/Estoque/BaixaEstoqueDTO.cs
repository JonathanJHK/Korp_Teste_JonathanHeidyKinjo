using System.ComponentModel.DataAnnotations;

namespace Estoque.Api.DTOs.Estoque
{
    public class BaixaEstoqueDTO
    {
        [Required(ErrorMessage = "A chave de idempotência é obrigatória.")]
        [MaxLength(
            100,
            ErrorMessage = "A chave de idempotência deve possuir no máximo 100 caracteres.")]
        public string ChaveIdempotencia { get; set; } = string.Empty;

        [Required(ErrorMessage = "Os itens da baixa são obrigatórios.")]
        [MinLength(
            1,
            ErrorMessage = "A baixa deve possuir pelo menos um item.")]
        public List<ItemBaixaEstoqueDTO> Itens { get; set; } = [];
    }
}