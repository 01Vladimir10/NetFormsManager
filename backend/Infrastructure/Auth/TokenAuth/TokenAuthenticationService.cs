using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using NetFormsManager.Core.Services;

namespace NetFormsManager.Infrastructure.Auth.TokenAuth;

public class TokenAuthenticationServiceOptions
{
    public string[] Tokens { get; set; } = [];
    public string? Schema { get; set; } = "Bearer";
    public string Header { get; set; } = "Authorization";
    public bool CaseInsensitiveComparison { get; set; }
}

public class TokenAuthenticationOpenApiConfigurationProvider(IOptions<TokenAuthenticationServiceOptions> options)
    : IAuthenticationOpenApiConfigurationProvider
{
    public IDictionary<string, IOpenApiSecurityScheme> GetSecuritySchemes() =>
        new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Token"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Scheme = options.Value.Schema,
                Name = options.Value.Header
            }
        };
}

public class TokenAuthenticationService(IOptionsMonitor<TokenAuthenticationServiceOptions> optionsMonitor)
    : IAuthenticationService
{
    public Task AuthenticateAsync(HttpContext context)
    {
        var options = optionsMonitor.CurrentValue;
        var token = context.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(options.Schema))
        {
            token = token.Replace(options.Schema, "").Trim();
        }

        var comparison = options.CaseInsensitiveComparison ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        if (options.Tokens.Contains(token, comparison))
        {
            context.User = new ClaimsPrincipal(
                new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Hash(token))], options.Schema)
            );
        }

        return Task.CompletedTask;
    }

    private static string Hash(string data) => Convert.ToBase64String(SHA1.HashData(Encoding.UTF8.GetBytes(data)));
}

public static class TokenAuthServiceExtensions
{
    public static void AddTokenAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TokenAuthenticationServiceOptions>(configuration);
        services.AddScoped<IAuthenticationService, TokenAuthenticationService>();
        services.AddTransient<IAuthenticationOpenApiConfigurationProvider, TokenAuthenticationOpenApiConfigurationProvider>();
    }
}