using NetMailGun.Core.Model;
using NetMailGun.Core.Repositories;

namespace NetMailGun.Infrastructure.Database.Mem;

public class EmailTemplatesMemRepository : IEmailTemplatesRepository
{
    private readonly Dictionary<(Guid FormId, Guid TemplateId), EmailTemplateEntity> _emailTemplates = [];

    public Task<EmailTemplateEntity> CreateAsync(EmailTemplateEntity entity)
    {
        _emailTemplates[(entity.FormId, entity.Id)] = entity;
        return Task.FromResult(entity);
    }

    public Task<EmailTemplateEntity> UpdateAsync(EmailTemplateEntity entity)
    {
        _emailTemplates[(entity.FormId, entity.Id)] = entity;
        return Task.FromResult(entity);
    }

    public Task<EmailTemplateEntity?> FindByIdAsync(Guid formId, Guid id)
    {
        return Task.FromResult(_emailTemplates.GetValueOrDefault((formId, id)));
    }

    public Task<List<EmailTemplateEntity>> GetByFormIdAsync(Guid formId)
    {
        return Task.FromResult(_emailTemplates.Values
            .Where(t => t.FormId == formId)
            .ToList());
    }

    public Task DeleteByIdAsync(Guid formId, Guid id)
    {
        _emailTemplates.Remove((formId, id));
        return Task.CompletedTask;
    }
}

