using Google.Cloud.Firestore;
using NetFormsManager.Core.Model;
using NetFormsManager.Core.Repositories;
using NetFormsManager.Infrastructure.Firestore;

namespace NetFormsManager.Infrastructure.Database.Firestore.Repositories;

internal class EmailTemplatesFirestoreRepository([FromKeyedServices(FirestoreServiceKeys.Db)] FirestoreDb firestoreDb) : IEmailTemplatesRepository
{
    private CollectionReference GetTemplatesCollection(Guid formId)
    {
        var formDocument = firestoreDb.Collection(FirestoreCollectionNames.Forms).Document(formId.ToString());
        return formDocument.Collection(FirestoreCollectionNames.Templates);
    }

    public async Task<EmailTemplateEntity> CreateAsync(EmailTemplateEntity entity)
    {
        var collection = GetTemplatesCollection(entity.FormId);
        var document = collection.Document(entity.Id.ToString());
        await document.SetAsync(entity);
        return entity;
    }

    public async Task<EmailTemplateEntity> UpdateAsync(EmailTemplateEntity entity)
    {
        var collection = GetTemplatesCollection(entity.FormId);
        var document = collection.Document(entity.Id.ToString());
        await document.SetAsync(entity);
        return entity;
    }

    public async Task<EmailTemplateEntity?> FindByIdAsync(Guid formId, Guid id)
    {
        var collection = GetTemplatesCollection(formId);
        var document = collection.Document(id.ToString());
        var snapshot = await document.GetSnapshotAsync();
        
        if (!snapshot.Exists)
        {
            return null;
        }

        return snapshot.ConvertTo<EmailTemplateEntity>();
    }

    public async Task<List<EmailTemplateEntity>> GetByFormIdAsync(Guid formId)
    {
        var collection = GetTemplatesCollection(formId);
        var snapshot = await collection.GetSnapshotAsync();
        
        return snapshot.Documents
            .Select(doc => doc.ConvertTo<EmailTemplateEntity>())
            .ToList();
    }

    public async Task DeleteByIdAsync(Guid formId, Guid id)
    {
        var collection = GetTemplatesCollection(formId);
        var document = collection.Document(id.ToString());
        await document.DeleteAsync();
    }
}

