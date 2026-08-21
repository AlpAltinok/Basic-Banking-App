using BankaApp.Application.Common.Exceptions;
using BankaApp.Application.DTOs.Transfer;
using BankaApp.Application.Interfaces;
using BankaApp.Domain.Entities;
using BankaApp.Domain.Enums;

namespace BankaApp.Application.Services;

public class TransferService : ITransferService
{
    private readonly IUserRepository _userRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TransferService(
        IUserRepository userRepository,
        IWalletRepository walletRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TransferResponse> TransferAsync(
        Guid fromUserId,
        TransferRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new BusinessException("INVALID_AMOUNT", "Tutar 0'dan büyük olmalıdır.");
        }

        var toEmail = request.ToEmail.Trim().ToLowerInvariant();

        // Idempotency: aynı key ile tekrar istek gelirse ikinci kez para çekilmez.
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existing = await _transactionRepository.GetByIdempotencyKeyAsync(
                request.IdempotencyKey, cancellationToken);

            if (existing is not null)
            {
                var senderWallet = await _walletRepository.GetByUserIdAsync(fromUserId, cancellationToken)
                    ?? throw new NotFoundException("Gönderen cüzdan bulunamadı.");
                var sender = await _userRepository.GetByIdAsync(fromUserId, cancellationToken)
                    ?? throw new NotFoundException("Gönderen kullanıcı bulunamadı.");

                return new TransferResponse
                {
                    TransactionId = existing.Id,
                    FromEmail = sender.Email,
                    ToEmail = toEmail,
                    Amount = existing.Amount,
                    Currency = existing.Currency,
                    SenderBalanceAfter = senderWallet.Balance,
                    Description = existing.Description,
                    CreatedAtUtc = existing.CreatedAtUtc
                };
            }
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var fromUser = await _userRepository.GetByIdAsync(fromUserId, ct)
                ?? throw new NotFoundException("Gönderen kullanıcı bulunamadı.");

            var toUser = await _userRepository.GetByEmailAsync(toEmail, ct)
                ?? throw new NotFoundException("Alıcı kullanıcı bulunamadı.");

            if (toUser.Id == fromUserId)
            {
                throw new BusinessException("SELF_TRANSFER", "Kendinize transfer yapamazsınız.");
            }

            var fromWallet = await _walletRepository.GetByUserIdAsync(fromUserId, ct)
                ?? throw new NotFoundException("Gönderen cüzdan bulunamadı.");

            var toWallet = await _walletRepository.GetByUserIdAsync(toUser.Id, ct)
                ?? throw new NotFoundException("Alıcı cüzdan bulunamadı.");

            if (fromWallet.Currency != toWallet.Currency)
            {
                throw new BusinessException("CURRENCY_MISMATCH", "Para birimleri uyuşmuyor.");
            }

            if (fromWallet.Balance < request.Amount)
            {
                throw new BusinessException(
                    "INSUFFICIENT_BALANCE",
                    $"Yetersiz bakiye. Mevcut: {fromWallet.Balance} {fromWallet.Currency}");
            }

            // Atomic update inside a DB transaction + optimistic concurrency on Version.
            fromWallet.Balance -= request.Amount;
            fromWallet.Version++;
            fromWallet.UpdatedAtUtc = DateTime.UtcNow;

            toWallet.Balance += request.Amount;
            toWallet.Version++;
            toWallet.UpdatedAtUtc = DateTime.UtcNow;

            var transaction = new Transaction
            {
                FromWalletId = fromWallet.Id,
                ToWalletId = toWallet.Id,
                Amount = request.Amount,
                Currency = fromWallet.Currency,
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Completed,
                Description = request.Description ?? $"Transfer → {toEmail}",
                IdempotencyKey = string.IsNullOrWhiteSpace(request.IdempotencyKey)
                    ? null
                    : request.IdempotencyKey.Trim()
            };

            await _transactionRepository.AddAsync(transaction, ct);

            return new TransferResponse
            {
                TransactionId = transaction.Id,
                FromEmail = fromUser.Email,
                ToEmail = toUser.Email,
                Amount = request.Amount,
                Currency = fromWallet.Currency,
                SenderBalanceAfter = fromWallet.Balance,
                Description = transaction.Description,
                CreatedAtUtc = transaction.CreatedAtUtc
            };
        }, cancellationToken);
    }
}
