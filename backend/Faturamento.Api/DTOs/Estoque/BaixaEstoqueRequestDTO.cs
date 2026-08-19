using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Estoque.Api.DTOs.Estoque
{
    public class BaixaEstoqueRequestDTO
    {
        public string ChaveIdempotencia { get; set; } = string.Empty;

        public List<ItemBaixaEstoqueRequestDTO> Itens { get; set; } = [];
    }
}