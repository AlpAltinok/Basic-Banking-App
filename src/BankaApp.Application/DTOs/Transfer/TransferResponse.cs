namespace BankaApp.Application.DTOs.Transfer;

public class TransferResponse
{
    public Guid TransactionId { get; set; }
    public string FromEmail { get; set; } = string.Empty;
    public string ToEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal SenderBalanceAfter { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
