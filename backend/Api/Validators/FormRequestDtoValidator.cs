using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;
using NetFormsManager.Core.Model;
using NetFormsManager.Core.Services;

namespace NetFormsManager.Api.Validators;

public class FormRequestDtoValidator : AbstractValidator<FormRequestDto>
{
    public FormRequestDtoValidator(IBotValidatorFactory botValidatorFactory,
        ISubscriptionProviderFactory subscriptionProviderFactory)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required");

        RuleFor(x => x.Fields)
            .NotNull()
            .NotEmpty()
            .WithMessage("You must specify at least one field")
            .Must(fields => fields != null && !fields.Any(f => ReferenceEquals(f, null)))
            .WithMessage("Fields cannot contain empty entries");

        RuleForEach(x => x.Fields)
            .ChildRules(field =>
            {
                field.RuleFor(f => f.Name)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage("Field name is required")
                    .Must(x => x.Length > 0 && x.All(char.IsLetterOrDigit) && char.IsLetterOrDigit(x[0]))
                    .WithMessage("Field names must be alphanumeric");

                // Validate field-specific rules based on type
                field.RuleFor(f => f)
                    .SetInheritanceValidator(builder => builder
                        .Add(new FormFieldValidators.DateOnly())
                        .Add(new FormFieldValidators.TimeOnly())
                        .Add(new FormFieldValidators.Text())
                        .Add(new FormFieldValidators.DateTime())
                        .Add(new FormFieldValidators.Int())
                        .Add(new FormFieldValidators.Double()));
            });

        RuleFor(x => x.Fields)
            .Must(fields =>
            {
                if (fields == null) return true;
                var duplicates = fields
                    .GroupBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToArray();
                return duplicates.Length == 0;
            })
            .WithMessage(x =>
            {
                var duplicates = x.Fields
                    .GroupBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToArray();

                return duplicates.Length > 0
                    ? $"Duplicate field names detected: {string.Join(", ", duplicates)}"
                    : "Duplicate field names detected";
            });

        When(x => x.SubscriptionsProvider is not null, () =>
        {
            RuleFor(x => x.SubscriptionsProvider!.Provider)
                .Must(providerName =>
                    providerName is null ||
                    subscriptionProviderFactory.EnumerateProviders()
                        .Contains(providerName, StringComparer.OrdinalIgnoreCase)
                )
                .WithMessage(
                    $"Invalid value, register providers are: {string.Join(", ", subscriptionProviderFactory.EnumerateProviders())}"
                );

            RuleFor(x => x.SubscriptionsProvider!.FieldReferences)
                .NotNull();
        });

        When(x => x.SubscriptionsProvider?.FieldReferences is not null, () =>
        {
            RuleFor(x => x.SubscriptionsProvider!.FieldReferences!.Email)
                .NotNull()
                .NotEmpty()
                .Must((request, fieldName) =>
                    !string.IsNullOrWhiteSpace(fieldName)
                    && request.Fields.Any(x =>
                        x.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase)
                        && x is EmailAddressField
                    )
                )
                .WithMessage("This must be the name of one of the fields of type 'email' in this form");

            RuleFor(x => x.SubscriptionsProvider!.FieldReferences!.Name)
                .Must((request, fieldName) =>
                    string.IsNullOrWhiteSpace(fieldName) || request.Fields.Any(x =>
                        x.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase)
                        && x is TextField or EmailAddressField
                    )
                )
                .WithMessage("This must be the name of one of the fields of type 'email' or 'text' in this form");

            RuleFor(x => x.SubscriptionsProvider!.FieldReferences!.Lastname)
                .Must((request, fieldName) =>
                    string.IsNullOrWhiteSpace(fieldName)
                    || request.Fields.Any(x =>
                        x.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase)
                        && x is TextField
                    )
                )
                .WithMessage("This must be the name of one of the fields of type 'text' in this form");

            RuleFor(x => x.SubscriptionsProvider!.FieldReferences!.Phone)
                .Must((request, fieldName) =>
                    string.IsNullOrWhiteSpace(fieldName)
                    || request.Fields.Any(x =>
                        x.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase)
                        && x is TextField
                    )
                )
                .WithMessage("This must be the name of one of the fields of type 'text' in this form");
        });

        RuleFor(x => x.AllowedOrigins)
            .NotNull()
            .NotEmpty()
            .WithMessage("You must specify at least one origin")
            .Must(origins => origins switch
            {
                null or ["*"] => true,
                _ => origins.All(domain => Uri.CheckHostName(domain) != UriHostNameType.Unknown)
            })
            .WithMessage("Invalid domains provided. If you want to allow any origin you must use a wildcard [\"*\"]; " +
                         "otherwise, you must specify valid domain names.");

        RuleFor(x => x.BotValidationProvider)
            .SetValidator(new BotValidationPropertyValidator(botValidatorFactory))
            .WithMessage("Invalid property value");
    }

    private class BotValidationPropertyValidator(IBotValidatorFactory factory)
        : PropertyValidator<FormRequestDto, FormBotValidation?>
    {
        public override bool IsValid(ValidationContext<FormRequestDto> context, FormBotValidation? value)
        {
            if (value is null) return true;
            var providers = factory.EnumerateProviders();
            if (!providers.Contains(value.Provider, StringComparer.OrdinalIgnoreCase))
            {
                context.AddFailure($"{nameof(FormRequestDto.BotValidationProvider)}.{nameof(value.Provider)}",
                    $"Invalid provider, supported providers are: {string.Join(',', factory.EnumerateProviders())}");
                return false;
            }

            if (!factory.TryGetMetadata(value.Provider, out var metadata)) return true;

            var errors = metadata.ParametersValidator(value.Parameters);

            foreach (var (parameterName, error) in errors.SelectMany(x => x.Value.Select(y => (x.Key, y))))
            {
                context.AddFailure(
                    $"{nameof(FormRequestDto.BotValidationProvider)}.{nameof(value.Parameters)}.{parameterName}",
                    error);
            }

            return errors.Count == 0;
        }

        public override string Name => nameof(BotValidationPropertyValidator);
    }

    private static class FormFieldValidators
    {
        public class DateTime : RangeFieldValidator<System.DateTime, DateTimeField>;

        public class DateOnly : RangeFieldValidator<System.DateOnly, DateOnlyField>;

        public class TimeOnly : RangeFieldValidator<System.TimeOnly, TimeOnlyField>;

        public class Int : RangeFieldValidator<int, IntField>;

        public class Double : RangeFieldValidator<double, DoubleField>;

        public class Text : AbstractValidator<TextField>
        {
            public Text()
            {
                RuleFor(x => x.MaxLen)
                    .Must(x => x is null or > 0)
                    .WithMessage("MaxLen must be greater than or equal to zero");
                RuleFor(x => x.MinLen)
                    .Must(x => x is null or > 0)
                    .WithMessage("MinLen must be greater than or equal to zero");

                RuleFor(x => new { x.MinLen, x.MaxLen })
                    .Must(x => x.MinLen < x.MaxLen)
                    .When(x => x is { MinLen: not null, MaxLen: not null })
                    .WithMessage("MinLen must be greater than or equal to MaxLen");

                RuleFor(x => x.Regex)
                    .Must(regex =>
                    {
                        if (string.IsNullOrWhiteSpace(regex)) return true;
                        try
                        {
                            _ = new Regex(regex);
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    })
                    .WithMessage("Invalid regex pattern");
            }
        }

        internal abstract class RangeFieldValidator<TValue, TField> : AbstractValidator<TField>
            where TValue : struct, IComparable<TValue>
            where TField : RangeField<TValue>
        {
            protected RangeFieldValidator()
            {
                RuleFor(x => new { x.Min, x.Max })
                    .Must(x => x.Min?.CompareTo(x.Max!.Value) <= 0)
                    .When(x => x is { Min: not null, Max: not null })
                    .WithMessage("MinLen must be greater than or equal to MaxLen");
            }
        }
    }
}