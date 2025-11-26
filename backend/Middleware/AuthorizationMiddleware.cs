using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using NetMailGun.Api;

namespace NetMailGun.Middleware;

public class AuthMiddleware(Core.Services.IAuthenticationService authenticationService) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await authenticationService.AuthenticateAsync(context);

        var allowAnonymous = context.GetEndpoint()?
            .Metadata
            .OfType<AllowAnonymousAttribute>()
            .Any() ?? false;

        if (allowAnonymous || context.User is { Identity.IsAuthenticated: true })
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = 401;
        context.Response.ContentType = MediaTypeNames.Application.Json;
        await context.Response.WriteAsJsonAsync(ErrorDto.Unauthorized());
    }
}