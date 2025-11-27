using Microsoft.AspNetCore.Cors.Infrastructure;
using NetFormsManager.Core.Repositories;

namespace NetFormsManager.Infrastructure.Middleware;

public class DynamicCorsPolicyProvider(
    IFormsRepository formsRepository,
    DefaultCorsPolicyProvider defaultCorsPolicyProvider) : ICorsPolicyProvider
{
    public const string DynamicCorsPolicy = "dynamicCors";

    public Task<CorsPolicy?> GetPolicyAsync(HttpContext context, string? policyName)
    {
        if (policyName is not DynamicCorsPolicy ||
            !context.GetRouteData().Values.TryGetValue("formId", out var formIdObj) ||
            !Guid.TryParse(formIdObj?.ToString(), out var formId))
        {
            return defaultCorsPolicyProvider.GetPolicyAsync(context, policyName);
        }

        return HandleDynamicCorsPolicy(formId);
    }

    private async Task<CorsPolicy?> HandleDynamicCorsPolicy(Guid formId)
    {
        var form = await formsRepository
            .FindByIdAsync(formId)
            .ConfigureAwait(false);
        if (form is null || form.AllowedOrigins.Length == 0)
        {
            return null;
        }

        var builder = new CorsPolicyBuilder()
            .AllowAnyHeader()
            .AllowAnyMethod();

        if (form.AllowedOrigins is ["*"])
            builder.AllowAnyOrigin();
        else
            builder.WithOrigins(form.AllowedOrigins
                .SelectMany(host => new[] { $"http://{host}", $"https://{host}" })
                .ToArray()
            );
        return builder.Build();
    }
}