using System.Diagnostics;
using NetFormsManager.Api;

namespace NetFormsManager.Middleware;

public class ExceptionMiddleware(ILogger<ExceptionMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(ErrorDto.InternalServerError());
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(
                    exception,
                    "Request failed. Endpoint: {Method} {Path}. TraceId: {TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    Activity.Current?.TraceId
                );
            }
        }
    }
}