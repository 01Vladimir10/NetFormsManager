using NetMailGun.Infrastructure.Subscriptions;
using NetMailGun.Infrastructure.Subscriptions.Firestore;

namespace NetMailGun.Configuration;

public static class SubscriptionsConfiguration
{
    public static void AddSubscriptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSubscriptionsProviderFactory(builder =>
        {
            foreach (var section in configuration.GetSection("Subscriptions").GetChildren())
            {
                switch (section.Key.ToLowerInvariant())
                {
                    case "firestore":
                        services.RegisterFirestoreSubscriptionProviderServices(section);
                        builder.RegisterProvider<FirestoreSubscriptionProvider>("firestore");
                        break;
                }
            }
        });
    }
}