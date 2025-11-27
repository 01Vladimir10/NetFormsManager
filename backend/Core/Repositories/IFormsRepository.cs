using NetFormsManager.Core.Model;

namespace NetFormsManager.Core.Repositories;

public interface IRepository<T>
{
    public Task<T> CreateAsync(T entity);
    public Task<T> UpdateAsync(T entity);
}

public interface IFormsRepository : IRepository<FormEntity>
{
    public Task<FormEntity?> FindByIdAsync(Guid id);
    public Task<bool> ExistsAsync(Guid id);
    public Task DeleteByIdAsync(Guid id);
    public Task<List<FormEntity>> GetAllAsync();
}

public interface IEmailTemplatesRepository : IRepository<EmailTemplateEntity>
{
    public Task<EmailTemplateEntity?> FindByIdAsync(Guid formId, Guid id);
    public Task<List<EmailTemplateEntity>> GetByFormIdAsync(Guid formId);
    public Task DeleteByIdAsync(Guid formId, Guid id);
}