namespace BankaApp.Domain.Entities;

/// <summary>
/// Kullanıcının dijital cüzdanı. Para tutarını ve para birimini temsil eder.
/// Bankacılıkta hesap (account) mantığının sadeleştirilmiş hali.
/// </summary>
public class Wallet
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    /// <summary>
    /// decimal: parasal değerler için float/double kullanma (yuvarlama hataları).
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// ISO 4217 para birimi kodu. Başlangıçta TRY.
    /// </summary>
    public string Currency { get; set; } = "TRY";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public User? User { get; set; }

    public ICollection<Transaction> OutgoingTransactions { get; set; } = new List<Transaction>();

    public ICollection<Transaction> IncomingTransactions { get; set; } = new List<Transaction>();
}
