namespace BankaApp.Application.DTOs.Wallet;

public class WalletResponse
{
    public Guid WalletId { get; set; }
    public Guid UserId { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "TRY";
    public DateTime UpdatedAtUtc { get; set; }
}
