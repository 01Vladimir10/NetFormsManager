using Google.Cloud.Firestore;
using NetFormsManager.Core.Model;
using NetFormsManager.Core.Repositories;
using NetFormsManager.Infrastructure.Database.Firestore.Repositories;
using NetFormsManager.Infrastructure.Firestore;

namespace NetFormsManager.Infrastructure.Database.Firestore;

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