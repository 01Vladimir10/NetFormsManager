using NetMailGun.Infrastructure.Database.Firestore;
using NetMailGun.Infrastructure.Database.Mem;

namespace NetMailGun.Configuration;

public static class DatabaseConfiguration
{
    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        configuration = configuration.GetSection("Database");
        switch (configuration["Provider"]?.ToLowerInvariant().Trim())
        {
            case "firestore":
                services.AddFirestoreDbProvider(configuration.GetSection("Firestore"));
                break;
            default:
                services.AddMemoryDbProvider();
                break;
        }
    }
}