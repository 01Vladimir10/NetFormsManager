using Google.Cloud.Firestore;
using NetFormsManager.Core.Services;
using NetFormsManager.Infrastructure.Firestore;

namespace NetFormsManager.Infrastructure.Subscriptions.Firestore;

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