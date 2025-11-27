using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Cors.Infrastructure;
using NetFormsManager.Infrastructure.Middleware;

namespace NetFormsManager.Configuration;

public static class CorsConfiguration
{
    public class ConfigurableCorsOptions
    {
        public bool Enabled { get; set; }
        public bool AllowAnyOrigin { get; set; }
        public bool AllowCredentials { get; set; }
        public string[]? AllowedOrigins { get; set; }
        public string[]? AllowedMethods { get; set; }
        public string[]? AllowedHeaders { get; set; }
        public TimeSpan? PreflightMaxAge { get; set; }
    }

    public static void AddManagedCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CorsOptions>(corsOptions =>
        {
            var options = configuration.GetSection("Cors").Get<ConfigurableCorsOptions>();
            if (options is { Enabled: true })
            {
                corsOptions.AddDefaultPolicy(builder =>
                {
                    if (options.AllowAnyOrigin)
                    {
                        builder.AllowAnyOrigin();
                    }
                    else
                    {
                        if (options.AllowCredentials)
                            builder.AllowCredentials();
                        if (options.AllowedOrigins != null)
                            builder.WithOrigins(options.AllowedOrigins);
                    }

                    if (options.AllowedMethods != null)
                        builder.WithMethods(options.AllowedMethods);
                    if (options.AllowedHeaders != null)
                        builder.WithHeaders(options.AllowedHeaders);
                    if (options.PreflightMaxAge is not null)
                        builder.SetPreflightMaxAge(options.PreflightMaxAge.Value);
                });
            }
        });
        services.AddTransient<ICorsService, CorsService>();
        services.AddTransient<ICorsPolicyProvider, DynamicCorsPolicyProvider>();
        services.AddTransient<DefaultCorsPolicyProvider>();
    }

    public static TBuilder WithDynamicCors<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        => builder.WithMetadata(new EnableCorsAttribute(DynamicCorsPolicyProvider.DynamicCorsPolicy));
}