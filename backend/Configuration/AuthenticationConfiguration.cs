using NetMailGun.Infrastructure.Auth.TokenAuth;

namespace NetMailGun.Configuration;

public static class AuthenticationConfiguration
{
    public static void AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        configuration = configuration.GetSection("Auth");
        switch (configuration["Provider"]?.ToLowerInvariant())
        {
            case "token":
            default:
                services.AddTokenAuthentication(configuration.GetSection("TokenAuth"));
                break;
        }
    }
}