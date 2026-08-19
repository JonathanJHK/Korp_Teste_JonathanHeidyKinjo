using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Faturamento.Api.DTOs.Estoque;

namespace Faturamento.Api.Interfaces
{
    public interface IEstoqueClient
    {
        Task<ProdutoEstoqueResponseDTO> BuscarProdutoPorId(int produtoId, CancellationToken cancellationToken);
    }
}