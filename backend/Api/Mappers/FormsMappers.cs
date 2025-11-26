using NetMailGun.Core.Model;

namespace NetMailGun.Api.Mappers;

public static class FormsMappers
{
    extension(FormSubscription subscription)
    {
        private FormSubscriptionProviderDto ToDto() => new()
        {
            Provider = subscription.Provider,
            FieldReferences = new FormSubscriptionProviderFieldsDto
            {
                Email = subscription.FieldReferences.Email,
                Name = subscription.FieldReferences.Name,
                Lastname = subscription.FieldReferences.Lastname,
                Phone = subscription.FieldReferences.Phone
            }
        };

        private static FormSubscription FromDto(FormSubscriptionProviderDto dto) => new()
        {
            Provider = dto.Provider ?? "",
            FieldReferences = new FormSubscriptionFields
            {
                Email = dto.FieldReferences?.Email ?? "",
                Name = dto.FieldReferences?.Name,
                Lastname = dto.FieldReferences?.Lastname,
                Phone = dto.FieldReferences?.Phone
            }
        };
    }

    extension(FormEntity form)
    {
        public FormDto ToDto() => new()
        {
            Id = form.Id,
            Name = form.Name,
            Fields = form.Fields,
            AllowedOrigins = form.AllowedOrigins,
            BotValidationProvider = form.BotValidator,
            CreatedAt = form.CreatedAt,
            SubscriptionsProvider = form.Subscription?.ToDto()
        };

        public static FormEntity FromDto(
            FormRequestDto dto,
            Guid id,
            DateTime createdAt = default,
            DateTime updatedAt = default
        ) => new()
        {
            Id = id,
            Name = dto.Name,
            Fields = dto.Fields,
            AllowedOrigins = dto.AllowedOrigins,
            BotValidator = dto.BotValidationProvider,
            CreatedAt = createdAt,
            LastUpdatedAt = updatedAt,
            Subscription = dto.SubscriptionsProvider is null
                ? null
                : FormSubscription.FromDto(dto.SubscriptionsProvider)
        };
    }
}