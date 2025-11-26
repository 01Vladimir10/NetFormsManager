namespace NetMailGun.Core.Model;

public class EmailTemplateEntity
{
    public required Guid FormId { get; set; }
    public required Guid Id { get; set; }
    public required string[] To { get; set; }
    public required string SubjectTemplate { get; set; }
    public required string Body { get; set; }
    public bool IsEnabled { get; set; }
    public string? FromName { get; set; }
    public string[]? ReplyTo { get; set; }
    public string[]? Bcc { get; set; }
    public string[]? Cc { get; set; }
}