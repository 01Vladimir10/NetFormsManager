using Google.Cloud.Firestore;
using NetMailGun.Core.Services;
using NetMailGun.Infrastructure.Firestore;

namespace NetMailGun.Infrastructure.Subscriptions.Firestore;

public static class FirestoreSubscriptionConfigurationExtensions
{
    public static void RegisterFirestoreSubscriptionProviderServices(
        this IServiceCollection services,
        IConfiguration configuration
    ) => services.RegisterFirestoreDb(
        configuration: configuration,
        key: FirestoreServiceKeys.Subscriptions,
        onBuild: builder => builder.ConverterRegistry = new ConverterRegistry
        {
            new FirestoreJsonConverter<SubscriberEntity>(),
            new FirestoreGuidConverter()
        }
    );
}