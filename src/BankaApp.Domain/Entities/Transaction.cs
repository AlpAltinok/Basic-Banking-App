using BankaApp.Domain.Enums;

namespace BankaApp.Domain.Entities;

/// <summary>
/// Finansal hareket kaydı (ledger satırı).
/// Sadece bakiyeyi güncellemek yetmez; her işlemin izi kalmalı.
/// </summary>
public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? FromWalletId { get; set; }

    public Guid? ToWalletId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "TRY";

    public TransactionType Type { get; set; }

    public TransactionStatus Status { get; set; } = TransactionStatus.Completed;

    public string? Description { get; set; }

    /// <summary>
    /// Aynı isteğin iki kez işlenmesini engellemek için (ileride idempotency).
    /// </summary>
    public string? IdempotencyKey { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public Wallet? FromWallet { get; set; }

    public Wallet? ToWallet { get; set; }
}
