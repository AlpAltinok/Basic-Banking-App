namespace BankaApp.Domain.Entities;

/// <summary>
/// Sisteme kayıtlı kullanıcı. Auth modülünün temel entity'si.
/// Finansal bakiye burada tutulmaz; bakiye Wallet'a aittir (tek sorumluluk).
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Düz metin şifre ASLA saklanmaz. BCrypt hash tutulur.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navigation: bir kullanıcının bir cüzdanı olur (1-1)
    public Wallet? Wallet { get; set; }
}
