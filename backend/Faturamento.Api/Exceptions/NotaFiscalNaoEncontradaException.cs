using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Faturamento.Api.Exceptions
{
    public class NotaFiscalNaoEncontradaException(int id) : Exception(
        $"A nota fiscal com o ID {id} não foi encontrada."
        );

}