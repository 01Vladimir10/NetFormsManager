using System.Net.Mail;
using FluentValidation;

namespace NetFormsManager.Api.Validators;

public class EmailTemplateRequestDtoValidator : AbstractValidator<EmailTemplateRequestDto>
{
    public EmailTemplateRequestDtoValidator()
    {
        RuleFor(x => x.SubjectTemplate)
            .NotEmpty()
            .WithMessage("SubjectTemplate is required");

        RuleFor(x => x.BodyTemplate)
            .NotEmpty()
            .WithMessage("BodyTemplate is required");

        RuleFor(x => x.To)
            .NotNull()
            .NotEmpty()
            .WithMessage("You must specify at least one recipient email address")
            .Must(emails => emails == null || emails.All(email => !string.IsNullOrWhiteSpace(email) && IsValidEmailOrTemplate(email)))
            .WithMessage("Invalid email addresses or template variables");

        RuleFor(x => x.Bcc)
            .Must(emails => emails == null || emails.Length == 0 || emails.All(email => !string.IsNullOrWhiteSpace(email) && IsValidEmailOrTemplate(email)))
            .WithMessage("Invalid email addresses or template variables")
            .When(x => x.Bcc is { Length: > 0 });

        RuleFor(x => x.Cc)
            .Must(emails => emails == null || emails.Length == 0 || emails.All(email => !string.IsNullOrWhiteSpace(email) && IsValidEmailOrTemplate(email)))
            .WithMessage("Invalid email addresses or template variables")
            .When(x => x.Cc is { Length: > 0 });

        RuleFor(x => x.ReplyTo)
            .Must(emails => emails == null || emails.Length == 0 || emails.All(email => !string.IsNullOrWhiteSpace(email) && IsValidEmailOrTemplate(email)))
            .WithMessage("Invalid email addresses or template variables")
            .When(x => x.ReplyTo   is { Length: > 0 });
    }

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidEmailOrTemplate(string? email) =>
        !string.IsNullOrWhiteSpace(email) && (IsMustacheTemplate(email) || IsValidEmail(email));

    private static bool IsMustacheTemplate(ReadOnlySpan<char> email)
    {
        if (!email.StartsWith("{{", StringComparison.Ordinal) ||
            !email.EndsWith("}}", StringComparison.Ordinal)) return false;
        var variableName = email[2..^2].Trim();
        return !variableName.IsEmpty && IsAlphanumeric(variableName);
    }

    private static bool IsAlphanumeric(ReadOnlySpan<char> span)
    {
        foreach (var c in span)
            if (!char.IsLetterOrDigit(c))
                return false;
        return true;
    }
}

