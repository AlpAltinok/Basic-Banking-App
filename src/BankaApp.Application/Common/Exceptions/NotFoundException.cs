namespace BankaApp.Application.Common.Exceptions;

public class NotFoundException : BusinessException
{
    public NotFoundException(string message)
        : base("NOT_FOUND", message)
    {
    }
}
