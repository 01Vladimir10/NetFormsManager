using Google.Cloud.Firestore;
using NetMailGun.Core.Model;
using NetMailGun.Core.Repositories;
using NetMailGun.Infrastructure.Database.Firestore.Repositories;
using NetMailGun.Infrastructure.Firestore;

namespace NetMailGun.Infrastructure.Database.Firestore;

internal static class FirestoreConfigurationExtensions
{
    public static void AddFirestoreDbProvider(this IServiceCollection services, IConfiguration configuration) =>
        services.RegisterFirestoreDb(configuration,
                FirestoreServiceKeys.Db,
                builder => builder.ConverterRegistry = new ConverterRegistry
                {
                    new FirestoreJsonConverter<FormEntity>(),
                    new FirestoreJsonConverter<EmailTemplateEntity>(),
                }
            )
            .AddSingleton<IFormsRepository, FormsFirestoreRepository>()
            .AddSingleton<IEmailTemplatesRepository, EmailTemplatesFirestoreRepository>();

}