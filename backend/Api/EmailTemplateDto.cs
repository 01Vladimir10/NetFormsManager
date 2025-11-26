namespace NetMailGun.Api;

public class EmailTemplateDto
{
    public required Guid EmailFormId { get; init; }
    public required Guid Id { get; init; }
    public required bool IsEnabled { get; init; }
    public string? FromName { get; init; }
    public required string SubjectTemplate { get; init; }
    public required string BodyTemplate { get; init; }
    public string[]? ReplyTo { get; init; }
    public required string[] To { get; init; }
    public string[]? Bcc { get; init; }
    public string[]? Cc { get; init; }

    public EmailTemplateDto()
    {
    }
}

