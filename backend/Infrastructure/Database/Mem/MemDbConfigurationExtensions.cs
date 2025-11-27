using NetFormsManager.Core.Repositories;
using NetFormsManager.Core.Services;
using NetFormsManager.Infrastructure.Templates;

namespace NetFormsManager.Infrastructure.Database.Mem;

public static class MemDbConfigurationExtensions
{
    public static void AddMemoryDbProvider(this IServiceCollection services)
    {
        services.AddSingleton<IFormsRepository, FormsMemRepository>();
        services.AddSingleton<IEmailTemplatesRepository, EmailTemplatesMemRepository>();
        services.AddSingleton<ITemplateRendererService, MustacheTemplateService>();
    }
    
}