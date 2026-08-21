using System.ComponentModel.DataAnnotations;

namespace BankaApp.Application.DTOs.Wallet;

public class MoneyOperationRequest
{
    [Range(0.01, 1_000_000)]
    public decimal Amount { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}
