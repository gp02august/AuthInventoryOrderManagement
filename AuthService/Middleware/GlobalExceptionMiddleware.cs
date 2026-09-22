using System.Net;
using System.Text.Json;

namespace AuthService.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(
                ex,
                "Unhandled exception occurred while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            context.Response.StatusCode = GetStatusCode(ex);

            context.Response.ContentType = "application/json";

            var response = new
            {
                message = ex.Message
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ArgumentException =>
                (int)HttpStatusCode.BadRequest,

            InvalidOperationException =>
                (int)HttpStatusCode.BadRequest,

            KeyNotFoundException =>
                (int)HttpStatusCode.NotFound,

            UnauthorizedAccessException =>
                (int)HttpStatusCode.Unauthorized,

            _ =>
                (int)HttpStatusCode.InternalServerError
        };
    }
}