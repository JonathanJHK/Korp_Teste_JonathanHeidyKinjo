using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Faturamento.Api.Exceptions
{
    public class ServicoEstoqueIndisponivelException(string mensagem, Exception? innerException = null) : Exception(
        mensagem, innerException
        );
}