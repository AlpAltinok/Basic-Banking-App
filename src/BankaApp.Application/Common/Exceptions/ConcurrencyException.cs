namespace BankaApp.Application.Common.Exceptions;

/// <summary>
/// Another request updated the same wallet first (lost-update protection).
/// Maps to HTTP 409 Conflict.
/// </summary>
public class ConcurrencyException : BusinessException
{
    public ConcurrencyException(string message = "The wallet was modified by another request. Please retry.")
        : base("CONCURRENCY_CONFLICT", message)
    {
    }
}
