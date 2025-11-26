using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using NetMailGun.Core.Model;

namespace NetMailGun.Api.Validators;

public static class FormPayloadParser
{
    public static bool TryParse(
        FormField[] fields,
        Dictionary<string, string?> map,
        [NotNullWhen(true)] out Dictionary<string, object?>? payload,
        [NotNullWhen(false)] out Dictionary<string, List<string>>? errors)
    {
        var errBuilder = new ErrorsBuilder();
        payload = new Dictionary<string, object?>();
        foreach (var formField in fields)
        {
            var value = map.GetValueOrDefault(formField.Name);
            var parser = CreateParser(formField);
            if (parser is null)
            {
                errBuilder.AddError(formField.Name, "Unknown field type");
                continue;
            }

            if (!parser.TryParse(value, out var parsedValue, out var error))
            {
                errBuilder.AddError(formField.Name, error);
                continue;
            }

            payload[formField.Name] = parsedValue;
        }

        errors = errBuilder.Build();

        return errors.Count == 0;
    }

    private class ErrorsBuilder
    {
        private readonly Dictionary<string, List<string>> _errors = new();

        public void AddError(string property, string error)
        {
            if (_errors.TryGetValue(property, out var errors)) errors.Add(error);
            else _errors[property] = [error];
        }

        public Dictionary<string, List<string>> Build() => _errors;
    }

    private static IFieldParser? CreateParser(FormField field) =>
        field switch
        {
            BooleanField booleanField => new FieldParser<BooleanField, bool>(booleanField, _ => null),
            DateOnlyField dateOnlyField => new RangeFieldParser<DateOnlyField, DateOnly>(dateOnlyField),
            DateTimeField dateTimeField => new RangeFieldParser<DateTimeField, DateTime>(dateTimeField),
            DoubleField doubleField => new RangeFieldParser<DoubleField, double>(doubleField),
            IntField intField => new RangeFieldParser<IntField, int>(intField),
            TimeOnlyField timeOnlyField => new RangeFieldParser<TimeOnlyField, TimeOnly>(timeOnlyField),
            TextField textField => new FieldParser<TextField, string>(textField, (value) =>
            {
                if (textField.MaxLen is not null && value.Length > textField.MaxLen)
                {
                    return $"Value is too long, max len is: {textField.MaxLen}";
                }

                if (textField.MinLen is not null && value.Length < textField.MinLen)
                {
                    return "Value is too short, min len is: " + textField.MinLen;
                }

                if (textField.AllowedValues is { Length: > 0 } && !textField.AllowedValues.Contains(value))
                {
                    return "Invalid value, allowed values are: " + string.Join(", ", textField.AllowedValues);
                }

                if (string.IsNullOrWhiteSpace(textField.Regex)) return null;
                var regex = new Regex(textField.Regex);
                return !regex.IsMatch(value) ? "Invalid value provided" : null;
            }),
            EmailAddressField emailAddressField => new FieldParser<EmailAddressField, string>(
                emailAddressField,
                email =>
                {
                    if (string.IsNullOrWhiteSpace(email) && !field.IsRequired)
                        return null;
                    return email.IsValidEmailAddress() ? null : $"Invalid email address: {email}";
                }
            ),
            _ => null
        };
}

public interface IFieldParser
{
    public bool TryParse(string? value, out object? parsedValue, [NotNullWhen(false)] out string? error);
}

public class FieldParser<TField, TValue>(TField field, Func<TValue, string?> validate)
    : IFieldParser where TValue : IParsable<TValue> where TField : FormField
{
    public bool TryParse(string? value, out object? parsedValue,
        [NotNullWhen(false)] out string? error)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            parsedValue = null;
            error = field.IsRequired ? "This field is required" : null;
            return error is null;
        }

        if (!TValue.TryParse(value, null, out var casted))
        {
            parsedValue = null;
            error = $"Invalid value provided. Value cannot be parsed: '{value}'";
            return false;
        }

        error = validate(casted);
        parsedValue = casted;
        return error == null;
    }
}

public class RangeFieldParser<TField, TValue>(TField field) : FieldParser<TField, TValue>(field, value =>
    {
        if (field.Min is not null && value.CompareTo(field.Min.Value) < 0)
        {
            return $"Invalid value, min value is: {field.Min}";
        }

        if (field.Max is not null && value.CompareTo(field.Max.Value) > 0)
        {
            return $"Invalid value, max value is: {field.Max}";
        }

        return null;
    }
) where TValue : struct, IParsable<TValue>, IComparable<TValue> where TField : RangeField<TValue>;