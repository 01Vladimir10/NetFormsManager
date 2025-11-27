using Microsoft.OpenApi;

namespace NetFormsManager.Core.Services;

public interface IAuthenticationService
{
    public Task AuthenticateAsync(HttpContext context);
}

public interface IAuthenticationOpenApiConfigurationProvider
{
    public IDictionary<string, IOpenApiSecurityScheme> GetSecuritySchemes();
}