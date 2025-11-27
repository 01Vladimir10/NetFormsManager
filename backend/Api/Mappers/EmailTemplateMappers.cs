using NetFormsManager.Core.Model;

namespace NetFormsManager.Api.Mappers;

public static class EmailTemplateMappers
{
    public static EmailTemplateDto ToDto(this EmailTemplateEntity templateEntity) => new()
    {
        EmailFormId = templateEntity.FormId,
        Id = templateEntity.Id,
        IsEnabled = templateEntity.IsEnabled,
        FromName = templateEntity.FromName,
        SubjectTemplate = templateEntity.SubjectTemplate,
        BodyTemplate = templateEntity.Body,
        ReplyTo = templateEntity.ReplyTo,
        To = templateEntity.To,
        Bcc = templateEntity.Bcc,
        Cc = templateEntity.Cc
    };

    public static EmailTemplateEntity FromDto(
        EmailTemplateRequestDto dto,
        Guid formId,
        Guid id
    ) => new()
    {
        FormId = formId,
        Id = id, 
        IsEnabled = dto.IsEnabled,
        FromName = dto.FromName,
        SubjectTemplate = dto.SubjectTemplate,
        Body = dto.BodyTemplate,
        ReplyTo = dto.ReplyTo,
        To = dto.To,
        Bcc = dto.Bcc,
        Cc = dto.Cc
    };
}

