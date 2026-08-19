using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Estoque.Api.DTOs.Estoque
{
    public class ItemBaixaEstoqueRequestDTO
    {
        public int ProdutoId { get; set; }

        public int Quantidade { get; set; }
    }
}