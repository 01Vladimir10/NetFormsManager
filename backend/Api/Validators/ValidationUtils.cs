using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;

namespace NetFormsManager.Api.Validators;

public static class ValidationUtils
{
    public static bool IsValidEmailAddress([NotNullWhen(true)] this string? email)
        => !string.IsNullOrWhiteSpace(email) && MailAddress.TryCreate(email, out _);
}