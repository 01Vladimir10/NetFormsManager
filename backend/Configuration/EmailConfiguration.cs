using NetFormsManager.Core.Services;
using NetFormsManager.Infrastructure.Email;

namespace NetFormsManager.Configuration;

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