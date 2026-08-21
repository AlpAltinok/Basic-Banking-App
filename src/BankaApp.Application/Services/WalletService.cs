using BankaApp.Application.Common.Exceptions;
using BankaApp.Application.DTOs.Wallet;
using BankaApp.Application.Interfaces;
using BankaApp.Domain.Entities;
using BankaApp.Domain.Enums;

namespace BankaApp.Application.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WalletService(
        IWalletRepository walletRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WalletResponse> GetMyWalletAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var wallet = await GetRequiredWalletAsync(userId, cancellationToken);
        return MapWallet(wallet);
    }

    public async Task<WalletResponse> DepositAsync(
        Guid userId,
        MoneyOperationRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateAmount(request.Amount);

        var wallet = await GetRequiredWalletAsync(userId, cancellationToken);

        // Deposit: money enters the system → ToWallet increases.
        wallet.Balance += request.Amount;
        wallet.Version++;
        wallet.UpdatedAtUtc = DateTime.UtcNow;

        await _transactionRepository.AddAsync(new Transaction
        {
            ToWalletId = wallet.Id,
            Amount = request.Amount,
            Currency = wallet.Currency,
            Type = TransactionType.Deposit,
            Status = TransactionStatus.Completed,
            Description = request.Description ?? "Para yatırma"
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapWallet(wallet);
    }

    public async Task<WalletResponse> WithdrawAsync(
        Guid userId,
        MoneyOperationRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateAmount(request.Amount);

        var wallet = await GetRequiredWalletAsync(userId, cancellationToken);

        if (wallet.Balance < request.Amount)
        {
            throw new BusinessException(
                "INSUFFICIENT_BALANCE",
                $"Yetersiz bakiye. Mevcut: {wallet.Balance} {wallet.Currency}");
        }

        // Withdrawal: money leaves the system → FromWallet decreases.
        wallet.Balance -= request.Amount;
        wallet.Version++;
        wallet.UpdatedAtUtc = DateTime.UtcNow;

        await _transactionRepository.AddAsync(new Transaction
        {
            FromWalletId = wallet.Id,
            Amount = request.Amount,
            Currency = wallet.Currency,
            Type = TransactionType.Withdrawal,
            Status = TransactionStatus.Completed,
            Description = request.Description ?? "Para çekme"
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapWallet(wallet);
    }

    public async Task<IReadOnlyList<TransactionResponse>> GetMyTransactionsAsync(
        Guid userId,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        var wallet = await GetRequiredWalletAsync(userId, cancellationToken);
        var items = await _transactionRepository.GetByWalletIdAsync(wallet.Id, take, cancellationToken);

        return items.Select(t => new TransactionResponse
        {
            TransactionId = t.Id,
            Type = t.Type.ToString(),
            Status = t.Status.ToString(),
            Amount = t.Amount,
            Currency = t.Currency,
            Description = t.Description,
            CreatedAtUtc = t.CreatedAtUtc
        }).ToList();
    }

    private async Task<Wallet> GetRequiredWalletAsync(Guid userId, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);
        if (wallet is null)
        {
            throw new NotFoundException("Kullanıcıya ait cüzdan bulunamadı.");
        }

        return wallet;
    }

    private static void ValidateAmount(decimal amount)
    {
        // decimal kullanıyoruz çünkü 0.1 + 0.2 float'ta bozulur; paradaki kuruşlar kritik.
        if (amount <= 0)
        {
            throw new BusinessException("INVALID_AMOUNT", "Tutar 0'dan büyük olmalıdır.");
        }
    }

    private static WalletResponse MapWallet(Wallet wallet) => new()
    {
        WalletId = wallet.Id,
        UserId = wallet.UserId,
        Balance = wallet.Balance,
        Currency = wallet.Currency,
        UpdatedAtUtc = wallet.UpdatedAtUtc
    };
}
