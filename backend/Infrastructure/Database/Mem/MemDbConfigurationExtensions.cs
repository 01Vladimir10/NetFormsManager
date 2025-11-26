using NetMailGun.Core.Repositories;
using NetMailGun.Core.Services;
using NetMailGun.Infrastructure.Templates;

namespace NetMailGun.Infrastructure.Database.Mem;

public static class MemDbConfigurationExtensions
{
    public static void AddMemoryDbProvider(this IServiceCollection services)
    {
        services.AddSingleton<IFormsRepository, FormsMemRepository>();
        services.AddSingleton<IEmailTemplatesRepository, EmailTemplatesMemRepository>();
        services.AddSingleton<ITemplateRendererService, MustacheTemplateService>();
    }
    
}