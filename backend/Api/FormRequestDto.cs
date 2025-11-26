using NetMailGun.Core.Model;

namespace NetMailGun.Api;

public class FormRequestDto
{
    public string Name { get; set; } = string.Empty;
    public FormField[] Fields { get; set; } = [];
    public string[] AllowedOrigins { get; set; } = [];
    public FormBotValidation? BotValidationProvider { get; set; }
    public FormSubscriptionProviderDto? SubscriptionsProvider { get; set; }
}

public class FormSubscriptionProviderDto
{
    public string? Provider { get; set; }
    public FormSubscriptionProviderFieldsDto? FieldReferences { get; set; }
}
public class FormSubscriptionProviderFieldsDto
{
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Lastname { get; set; }
    public string? Phone { get; set; }
}
