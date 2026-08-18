using System.ComponentModel.DataAnnotations;

namespace Faturamento.Api.DTOs.NotasFiscais
{
    public class NotaFiscalCriarDTO
    {
        [Required(ErrorMessage = "Os itens da nota fiscal são obrigatórios.")]
        [MinLength(
            1,
            ErrorMessage = "A nota fiscal deve possuir pelo menos um item.")]
        public List<ItemNotaFiscalCriarDTO> Itens { get; set; } = [];
    }
}