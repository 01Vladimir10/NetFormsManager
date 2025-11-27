namespace NetFormsManager.Configuration;

public static partial class CorsConfiguration
{
    public class CorsOptions
    {
        public bool Enabled { get; set; }
        public bool AllowAnyOrigin { get; set; }
        public bool AllowCredentials { get; set; }
        public string[]? AllowedOrigins { get; set; }
        public string[]? AllowedMethods { get; set; }
        public string[]? AllowedHeaders { get; set; }
        public TimeSpan? PreflightMaxAge { get; set; }
    }

    public static void ConfigureCors(this WebApplication app, IConfiguration configuration)
    {
        if (!configuration.GetSection("Cors").Exists())
        {
            return;
        }

        var options = configuration.GetSection("Cors").Get<CorsOptions>();
        
        if (options is not { Enabled: true })
        {
            return;
        }
        

        app.UseCors(builder =>
        {
            
            LogConfiguringCorsSettingsOptions(app.Logger, options);
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

    [LoggerMessage(LogLevel.Information, "Configuring CORS. Settings: {options}")]
    static partial void LogConfiguringCorsSettingsOptions(this ILogger logger, CorsOptions options);
}