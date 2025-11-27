using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using NetFormsManager.Core.Services;

namespace NetFormsManager.Infrastructure.Email;

public class SmtpOptions
{
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string Host { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public int Port { get; set; }
    public bool UseSsl { get; set; }
    public int Timeout { get; set; } = 10000;
}

public class SmtpEmailService : IEmailService, IDisposable
{
    private SmtpClient _smtpClient;
    private readonly IDisposable? _subscription;
    private SmtpOptions _options;

    public SmtpEmailService(IOptionsMonitor<SmtpOptions> options)
    {
        _options = options.CurrentValue;
        _smtpClient = CreateSmtpClient(options.CurrentValue);
        _subscription = options.OnChange(value =>
        {
            _options = value;
            _smtpClient = CreateSmtpClient(value);
        });
    }

    private static SmtpClient CreateSmtpClient(SmtpOptions options) => new(options.Host, options.Port)
    {
        EnableSsl = options.UseSsl,
        DeliveryMethod = SmtpDeliveryMethod.Network,
        Credentials = !string.IsNullOrWhiteSpace(options.Username) && !string.IsNullOrWhiteSpace(options.Password)
            ? new NetworkCredential(options.Username, options.Password)
            : null,
        Timeout = options.Timeout
    };

    public Task SendAsync(IEnumerable<EmailMessage> messages) =>
        Task.WhenAll(messages.Select(message => _smtpClient.SendMailAsync(ConvertToMailMessage(message))));

    private MailMessage ConvertToMailMessage(EmailMessage emailMessage)
    {
        var message = new MailMessage
        {
            Subject = emailMessage.Subject,
            Body = emailMessage.Body,
        };
        foreach (var mailAddress in ToMailAddress(emailMessage.To))
            message.To.Add(mailAddress);
        foreach (var mailAddress in ToMailAddress(emailMessage.Bcc))
            message.Bcc.Add(mailAddress);
        foreach (var mailAddress in ToMailAddress(emailMessage.Cc))
            message.CC.Add(mailAddress);
        foreach (var mailAddress in ToMailAddress(emailMessage.ReplyTo))
            message.ReplyToList.Add(mailAddress);

        if (!string.IsNullOrWhiteSpace(emailMessage.FromEmail) &&
            MailAddress.TryCreate(emailMessage.FromEmail, emailMessage.FromName, out var from))
            message.From = from;
        else if (!string.IsNullOrWhiteSpace(_options.FromEmail) &&
                 MailAddress.TryCreate(_options.FromEmail, _options.FromName, out var defaultFrom))
            message.From = defaultFrom;

        return message;

        static IEnumerable<MailAddress> ToMailAddress(IEnumerable<string>? addresses) => addresses?
            .Select(address => MailAddress.TryCreate(address, out var mailAddress) ? mailAddress : null)
            .OfType<MailAddress>() ?? [];
    }

    public void Dispose()
    {
        _smtpClient.Dispose();
        _subscription?.Dispose();
    }
}