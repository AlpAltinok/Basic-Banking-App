using BankaApp.Domain.Entities;

namespace BankaApp.Application.Interfaces;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);

    Task<Transaction?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Transaction>> GetByWalletIdAsync(
        Guid walletId,
        int take = 20,
        CancellationToken cancellationToken = default);
}
