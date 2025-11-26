namespace NetMailGun.Api;

public class EmailTemplateRequestDto
{
    public bool IsEnabled { get; set; }
    public string? FromName { get; set; }
    public string SubjectTemplate { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public string[]? ReplyTo { get; set; }
    public string[] To { get; set; } = [];
    public string[]? Bcc { get; set; }
    public string[]? Cc { get; set; }
}