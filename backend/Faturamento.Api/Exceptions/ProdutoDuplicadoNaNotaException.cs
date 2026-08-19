using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Faturamento.Api.Exceptions
{
    public class ProdutoDuplicadoNaNotaException(int produtoId) : Exception(
            $"O produto com o ID {produtoId} foi informado mais de uma vez na nota fiscal."
            );

}