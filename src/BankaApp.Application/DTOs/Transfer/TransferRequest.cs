using System.ComponentModel.DataAnnotations;

namespace BankaApp.Application.DTOs.Transfer;

public class TransferRequest
{
    /// <summary>
    /// Alıcının e-posta adresi (kayıtlı kullanıcı).
    /// </summary>
    [Required, EmailAddress]
    public string ToEmail { get; set; } = string.Empty;

    [Range(0.01, 1_000_000)]
    public decimal Amount { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Aynı isteğin iki kez işlenmesini engellemek için istemci üretir (opsiyonel).
    /// </summary>
    [MaxLength(100)]
    public string? IdempotencyKey { get; set; }
}
