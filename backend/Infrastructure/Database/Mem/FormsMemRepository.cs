using NetFormsManager.Core.Model;
using NetFormsManager.Core.Repositories;

namespace NetFormsManager.Infrastructure.Database.Mem;

public class FormsMemRepository : IFormsRepository
{
    private readonly Dictionary<Guid, FormEntity> _emailForms = [];

    public Task<FormEntity> CreateAsync(FormEntity entity)
    {
        _emailForms[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<FormEntity> UpdateAsync(FormEntity entity)
    {
        _emailForms[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<FormEntity?> FindByIdAsync(Guid id)
    {
        return Task.FromResult(_emailForms.GetValueOrDefault(id));
    }

    public Task<bool> ExistsAsync(Guid id)
    {
        return Task.FromResult(_emailForms.ContainsKey(id));
    }

    public Task DeleteByIdAsync(Guid id)
    {
        _emailForms.Remove(id);
        return Task.CompletedTask;
    }

    public Task<List<FormEntity>> GetAllAsync()
    {
        return Task.FromResult(_emailForms.Values.ToList());
    }
}