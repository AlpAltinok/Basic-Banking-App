using System.Net;
using System.Text.Json;
using BankaApp.Application.Common.Exceptions;

namespace BankaApp.Api.Middleware;

/// <summary>
/// Yakalanmayan exception'ları HTTP yanıtına çevirir.
/// API'nin çökmesini engeller, tutarlı hata formatı döner.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorCode, message) = exception switch
        {
            NotFoundException notFound => (HttpStatusCode.NotFound, notFound.ErrorCode, notFound.Message),
            BusinessException business => (HttpStatusCode.BadRequest, business.ErrorCode, business.Message),
            UnauthorizedAccessException unauthorized => (HttpStatusCode.Unauthorized, "UNAUTHORIZED", unauthorized.Message),
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "Beklenmeyen bir hata oluştu.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception");
        }
        else
        {
            _logger.LogWarning(exception, "Handled business exception: {ErrorCode}", errorCode);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = new
        {
            errorCode,
            message,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
