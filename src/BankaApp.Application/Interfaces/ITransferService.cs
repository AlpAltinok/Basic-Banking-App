using BankaApp.Application.DTOs.Transfer;

namespace BankaApp.Application.Interfaces;

public interface ITransferService
{
    Task<TransferResponse> TransferAsync(
        Guid fromUserId,
        TransferRequest request,
        CancellationToken cancellationToken = default);
}
