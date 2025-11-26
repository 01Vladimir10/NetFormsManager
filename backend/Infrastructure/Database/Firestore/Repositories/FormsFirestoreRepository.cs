using Google.Cloud.Firestore;
using NetMailGun.Core.Model;
using NetMailGun.Core.Repositories;
using NetMailGun.Infrastructure.Firestore;

namespace NetMailGun.Infrastructure.Database.Firestore.Repositories;

internal class FormsFirestoreRepository([FromKeyedServices(FirestoreServiceKeys.Db)] FirestoreDb firestoreDb) : IFormsRepository
{
    public async Task<FormEntity> CreateAsync(FormEntity entity)
    {
        var collection = firestoreDb.Collection(FirestoreCollectionNames.Forms);
        var document = collection.Document(entity.Id.ToString());
        await document.SetAsync(entity);
        return entity;
    }

    public async Task<FormEntity> UpdateAsync(FormEntity entity)
    {
        var document = firestoreDb.Collection(FirestoreCollectionNames.Forms).Document(entity.Id.ToString());
        entity.LastUpdatedAt = DateTime.UtcNow;
        await document.SetAsync(entity);
        return entity;
    }

    public async Task<FormEntity?> FindByIdAsync(Guid id)
    {
        var document = firestoreDb.Collection(FirestoreCollectionNames.Forms).Document(id.ToString());
        var snapshot = await document.GetSnapshotAsync();
        return !snapshot.Exists ? null : snapshot.ConvertTo<FormEntity>();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var document = firestoreDb.Collection(FirestoreCollectionNames.Forms).Document(id.ToString());
        var snapshot = await document.GetSnapshotAsync();
        
        return snapshot.Exists;
    }

    public Task DeleteByIdAsync(Guid id)
    {
        return Task.FromResult(firestoreDb.Collection(FirestoreCollectionNames.Forms).Document(id.ToString()).DeleteAsync());
    }

    public async Task<List<FormEntity>> GetAllAsync()
    {
        var collection = firestoreDb.Collection(FirestoreCollectionNames.Forms);
        var snapshot = await collection.GetSnapshotAsync();

        return snapshot.Documents
            .Select(doc => doc.ConvertTo<FormEntity>())
            .ToList();
    }
}