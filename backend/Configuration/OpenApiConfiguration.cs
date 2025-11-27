using Microsoft.OpenApi;
using NetFormsManager.Core.Services;

namespace NetFormsManager.Configuration;

public static class OpenApiConfiguration
{
    public static void AddManagedOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
    
            options.AddDocumentTransformer((doc, context, _) =>
            {
                var configuration = context.ApplicationServices.GetRequiredService<IConfiguration>();
                var servers = configuration.GetSection("OpenApi:Servers").Get<List<OpenApiServer>>();
                if (servers is { Count: > 0 })
                {
                    doc.Servers = servers;
                }
                var provider = context.ApplicationServices.GetService<IAuthenticationOpenApiConfigurationProvider>();
                if (provider is null) return Task.CompletedTask;
                doc.Components ??= new OpenApiComponents();
                doc.Components.SecuritySchemes = provider.GetSecuritySchemes();
                return Task.CompletedTask;
            });
        });
    }
    
}