using System.Diagnostics.CodeAnalysis;
using NetMailGun.Core.Services;

namespace NetMailGun.Infrastructure.Subscriptions;

internal class SubscriptionsProviderFactory(IServiceProvider serviceProvider, ICollection<string> providers)
    : ISubscriptionProviderFactory
{
    public bool TryCreate(string name, [NotNullWhen(true)] out ISubscriptionProvider? instance)
    {
        instance = serviceProvider.GetKeyedService<ISubscriptionProvider>(name);
        return instance is not null;
    }

    public IEnumerable<string> EnumerateProviders() => providers;
}

public static class SubscriptionsProviderFactoryExtensions
{
    public class Builder(IServiceCollection services, ICollection<string> providers)
    {
        public void RegisterProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
            string key) where T : class, ISubscriptionProvider
        {
            services.AddKeyedScoped<ISubscriptionProvider, T>(key);
            providers.Add(key);
        }
    }

    public static void AddSubscriptionsProviderFactory(
        this IServiceCollection services,
        Action<Builder> builder
    )
    {
        var providerNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        builder(new Builder(services, providerNames));
        services.AddScoped<ISubscriptionProviderFactory>(sp => new SubscriptionsProviderFactory(sp, providerNames));
    }
}