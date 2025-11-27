using NetFormsManager.Infrastructure.Subscriptions;
using NetFormsManager.Infrastructure.Subscriptions.Firestore;

namespace NetFormsManager.Configuration;

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