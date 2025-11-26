using NetMailGun.Api;
using NetMailGun.Core.Services;

namespace NetMailGun.Endpoints;

public static class ProvidersEndpoints
{
    public static void MapProvidersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var providersGroup = endpoints.MapGroup("/providers");

        providersGroup.MapGet(
            "/subscriptions",
            (ISubscriptionProviderFactory subscriptionsFactory) => Results.Ok(subscriptionsFactory.EnumerateProviders())
        );


        providersGroup.MapGet("/botDetectors", (IBotValidatorFactory factory) => factory.EnumerateProviders()
            .Select(providerName => factory.TryGetMetadata(providerName, out var metadata)
                ? new BotValidationProviderDto(providerName, metadata.Parameters)
                : null)
            .OfType<BotValidationProviderDto>());

        providersGroup.MapGet("/botDetectors/{name}", (IBotValidatorFactory factory, string name) =>
        {
            var provider = factory
                .EnumerateProviders()
                .Where(providerName => providerName.Equals(name, StringComparison.OrdinalIgnoreCase))
                .Select(providerName => factory.TryGetMetadata(providerName, out var metadata)
                    ? new BotValidationProviderDto(providerName, metadata.Parameters)
                    : null)
                .OfType<BotValidationProviderDto>()
                .FirstOrDefault();
            
            return provider is null ? ErrorResults.NotFound() : Results.Ok(provider);
        });
    }
}