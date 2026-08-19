using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Estoque.Api.DTOs.Estoque;

namespace Estoque.Api.Interfaces
{
    public interface IEstoqueService
    {
        Task<BaixaEstoqueResponseDTO> Baixar(
            BaixaEstoqueDTO novaBaixaEstoque,
            CancellationToken cancellationToken);
    }
}