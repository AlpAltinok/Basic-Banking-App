namespace BankaApp.Application.Common.Exceptions;

/// <summary>
/// İş kuralı ihlali. API katmanı bunu 400/409 gibi HTTP koduna çevirir.
/// </summary>
public class BusinessException : Exception
{
    public string ErrorCode { get; }

    public BusinessException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}
