using System.Diagnostics.CodeAnalysis;
using NetMailGun.Core.Services;

namespace NetMailGun.Infrastructure.TokenValidators;

public class BotValidationFactory(
    IServiceProvider serviceProvider,
    Dictionary<string, BotValidationProviderMetadata> metadata) : IBotValidatorFactory
{
    public bool TryCreate(string provider, [NotNullWhen(true)] out IBotValidatorProvider? instance)
    {
        instance = serviceProvider.GetKeyedService<IBotValidatorProvider>(provider.ToLowerInvariant());
        return instance is not null;
    }

    public IEnumerable<string> EnumerateProviders() => metadata.Keys;

    public bool TryGetMetadata(string provider,
        [NotNullWhen(true)] out BotValidationProviderMetadata? providerMetadata) =>
        metadata.TryGetValue(provider, out providerMetadata);
}

public static class TokenValidationFactoryExtensions
{
    public class RegistryBuilder(
        IServiceCollection serviceCollection,
        Dictionary<string, BotValidationProviderMetadata> validators)
    {
        public void RegisterValidator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>
            (string name) where T : class, IBotValidatorProvider
        {
            serviceCollection.AddKeyedScoped<IBotValidatorProvider, T>(name.ToLowerInvariant());
            validators[name] = T.GetMetadata();
        }
    }

    public static void AddTokenValidationFactory(this IServiceCollection services, Action<RegistryBuilder> builder)
    {
        var validators = new Dictionary<string, BotValidationProviderMetadata>(StringComparer.OrdinalIgnoreCase);
        builder(new RegistryBuilder(services, validators));
        services.AddScoped<IBotValidatorFactory>(provider => new BotValidationFactory(provider, validators));
    }
}