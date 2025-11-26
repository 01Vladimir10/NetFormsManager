namespace NetMailGun.Core.Services;

public interface IEmailService
{
    public Task SendAsync(IEnumerable<EmailMessage> message);
}

public class EmailMessage
{
    public required ICollection<string> To { get; set; }
    public ICollection<string>? Bcc { get; set; }
    public ICollection<string>? Cc { get; set; }
    public ICollection<string>? ReplyTo { get; set; }
    public string? FromName { get; set; }
    public string? FromEmail { get; set; }
    public required string Body { get; set; }
    public required string Subject { get; set; }
}