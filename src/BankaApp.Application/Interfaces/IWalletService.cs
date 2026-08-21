using BankaApp.Application.DTOs.Wallet;

namespace BankaApp.Application.Interfaces;

public interface IWalletService
{
    Task<WalletResponse> GetMyWalletAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<WalletResponse> DepositAsync(
        Guid userId,
        MoneyOperationRequest request,
        CancellationToken cancellationToken = default);

    Task<WalletResponse> WithdrawAsync(
        Guid userId,
        MoneyOperationRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TransactionResponse>> GetMyTransactionsAsync(
        Guid userId,
        int take = 20,
        CancellationToken cancellationToken = default);
}
