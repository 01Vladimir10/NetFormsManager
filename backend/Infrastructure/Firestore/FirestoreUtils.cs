using Google.Api.Gax;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

namespace NetMailGun.Infrastructure.Firestore;

internal static class FirestoreUtils
{
    internal static IServiceCollection RegisterFirestoreDb(
        this IServiceCollection services,
        IConfiguration configuration,
        string key,
        Action<FirestoreDbBuilder>? onBuild = null)
    {
        FirestoreDbBuilder builder;
        switch (configuration["Source"]?.ToLowerInvariant().Trim())
        {
            case "credentials":
                var credentialParameters = configuration
                                               .GetSection("Credentials")
                                               .Get<FirestoreCredentialOptions>() ??
                                           throw new Exception("Missing Firestore credentials configuration");

                // Handle escaped newlines in private key (common in JSON config files)
                var privateKey = credentialParameters.PrivateKey.Replace("\\n", "\n");
                builder = new FirestoreDbBuilder
                {
                    UniverseDomain = credentialParameters.UniverseDomain,
                    Credential = new ServiceAccountCredential(
                        new ServiceAccountCredential.Initializer(
                                id: credentialParameters.ClientEmail,
                                tokenServerUrl: credentialParameters.TokenUri
                            )
                            {
                                ProjectId = credentialParameters.ProjectId,
                                KeyId = credentialParameters.PrivateKeyId,
                                UniverseDomain = credentialParameters.UniverseDomain,
                                UseJwtAccessWithScopes = credentialParameters.UniverseDomain != "googleapis.com",
                            }
                            .FromPrivateKey(privateKey)
                    ),
                    UseJwtAccessWithScopes = false,
                    BatchGetDocumentsRetrySettings = null,
                    ProjectId = credentialParameters.ProjectId,
                    EmulatorDetection = EmulatorDetection.EmulatorOrProduction
                };
                break;
            default:
                builder = new FirestoreDbBuilder();
                break;
        }

        onBuild?.Invoke(builder);
        services.AddKeyedSingleton(key, builder.Build());
        return services;
    }
}