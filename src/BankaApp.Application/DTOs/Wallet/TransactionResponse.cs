namespace BankaApp.Application.DTOs.Wallet;

public class TransactionResponse
{
    public Guid TransactionId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
