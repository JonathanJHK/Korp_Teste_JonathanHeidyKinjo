using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Estoque.Api.Exceptions
{
    public class NotaFiscalJaFechadaException(int id) : Exception(
            $"A nota fiscal com o ID {id} já está fechada e não pode ser impressa novamente.");
}