using NetFormsManager.Core.Services;

namespace NetFormsManager.Infrastructure.Email;

public class EmptyEmailService(ILogger<EmptyEmailService> logger) : IEmailService
{
    public Task SendAsync(IEnumerable<EmailMessage> messages)
    {
        if (!logger.IsEnabled(LogLevel.Information)) return Task.CompletedTask;

        foreach (var message in messages)
        {
            logger.LogInformation(
                "Subject: {Subject}\nTo:{@To}\nCc:{@Cc}\nBcc:{Bcc}\nReply to: {@ReplyTo}\n{Body}",
                message.Subject, message.To, message.Cc, message.Bcc, message.ReplyTo, message.Body
            );
        }

        return Task.CompletedTask;
    }
}