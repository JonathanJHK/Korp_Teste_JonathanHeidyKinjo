using Faturamento.Api.DTOs.NotasFiscais;

namespace Faturamento.Api.Interfaces
{
    public interface INotaFiscalService
    {
        Task<NotaFiscalResponseDTO> Cadastrar(
            NotaFiscalCriarDTO novaNotaFiscal,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<NotaFiscalResponseDTO>> Listar(
            CancellationToken cancellationToken);

        Task<NotaFiscalResponseDTO> BuscarPorId(
            int id,
            CancellationToken cancellationToken);

        Task<NotaFiscalResponseDTO> Imprimir(
            int id,
            CancellationToken cancellationToken);
    }
}