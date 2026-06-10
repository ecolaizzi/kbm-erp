using System.Net;
using System.Text.Json;
using KBM.Application.Auth;

namespace KBM.Api.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            var payload = new ApiError("INTERNAL_ERROR", "Errore interno del server.");
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
