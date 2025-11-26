using System.Text.Json;
using Google.Cloud.Firestore;
using NetMailGun.Core.Services;
using NetMailGun.Infrastructure.Firestore;

namespace NetMailGun.Infrastructure.Subscriptions.Firestore;

public class FirestoreSubscriptionProvider([FromKeyedServices(FirestoreServiceKeys.Subscriptions)] FirestoreDb db)
    : ISubscriptionProvider
{
    private const string CollectionName = "subscriptions";

    public async Task<string> SubscribeAsync(Guid formId, string emailAddress, string? name, string? lastName = null,
        string? phoneNumber = null)
    {
        var reference = db.Collection(CollectionName).Document(CreateKey(formId, emailAddress));

        var document = await reference.GetSnapshotAsync();

        if (document.Exists)
        {
            var entity = document.ConvertTo<SubscriberEntity>();
            return entity.UnsubscriptionToken;
        }

        var unSubscriptionToken = Guid.NewGuid().ToString("N");

        await reference.SetAsync(new SubscriberEntity
        {
            FormId = formId,
            UnsubscriptionToken = unSubscriptionToken,
            EmailAddress = emailAddress,
            Name = name,
            Lastname = lastName,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow
        });

        return unSubscriptionToken;
    }

    public async Task UnsubscribeAsync(string token)
    {
        var documents = await db.Collection(CollectionName)
            .WhereEqualTo(JsonNamingPolicy.CamelCase.ConvertName(nameof(SubscriberEntity.UnsubscriptionToken)), token)
            .GetSnapshotAsync();

        if (documents is [var document, ..])
        {
            await db.Collection(CollectionName)
                .Document(document.Id)
                .DeleteAsync();
        }
    }

    public async Task<PaginationResult<SubscriberEntity>> GetSubscribersAsync(Guid formId, int page, int pageSize)
    {
        var offset = (page - 1) * pageSize;
        var aggregateResult = await db
            .Collection(CollectionName)
            .WhereEqualTo("formId", formId)
            .Count()
            .GetSnapshotAsync();

        var totalItems = aggregateResult?.Count ?? 0;

        if (totalItems <= offset)
        {
            return PaginationResult<SubscriberEntity>.From(page, pageSize, totalItems);
        }

        var snapshot = await db
            .Collection(CollectionName)
            .WhereEqualTo(GetPropertyName(nameof(SubscriberEntity.FormId)), formId)
            .Offset(offset)
            .Limit(pageSize)
            .GetSnapshotAsync();

        return PaginationResult<SubscriberEntity>.From(
            page,
            pageSize,
            totalItems,
            items: snapshot.Documents
                .Select(x => x.ConvertTo<SubscriberEntity>())
                .ToArray()
        );
    }

    public async Task<List<SubscriberEntity>> GetAllSubscribersAsync(Guid formId)
    {
        var snapshot = await db
            .Collection(CollectionName)
            .Where(Filter.EqualTo(GetPropertyName(nameof(SubscriberEntity.FormId)), formId))
            .GetSnapshotAsync();

        return snapshot.Select(x => x.ConvertTo<SubscriberEntity>()).ToList();
    }

    private static string GetPropertyName(string name) => JsonNamingPolicy.CamelCase.ConvertName(name);
    private static string CreateKey(Guid formId, string emailAddress) => $"{formId}_{emailAddress}";
}