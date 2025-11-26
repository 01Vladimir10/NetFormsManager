using NetMailGun.Core.Services;
using NetMailGun.Infrastructure.Email;

namespace NetMailGun.Configuration;

public static class EmailConfiguration
{
    public static void AddEmailService(this IServiceCollection services, IConfiguration configuration)
    {
        
        switch (configuration["Email:Provider"]?.ToLower() ?? "default")
        {
            case "default":
            case "smtp":
                services.Configure<SmtpOptions>(configuration.GetSection("Email:Smtp"));
                services.AddScoped<IEmailService, SmtpEmailService>();
                break;
        }
    }
}