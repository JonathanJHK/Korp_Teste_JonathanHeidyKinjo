using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Estoque.Api.Exceptions
{
    public class OperacaoEstoqueRejeitadaException(string mensagem) : Exception(mensagem);

}