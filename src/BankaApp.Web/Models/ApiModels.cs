namespace BankaApp.Web.Models;

public class AuthResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}

public class WalletResponse
{
    public Guid WalletId { get; set; }
    public Guid UserId { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "TRY";
    public DateTime UpdatedAtUtc { get; set; }
}

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

public class ApiError
{
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
