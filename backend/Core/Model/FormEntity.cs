using System.Text.Json.Serialization;

namespace NetMailGun.Core.Model;

public class FormEntity
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required FormField[] Fields { get; set; }
    public required string[] AllowedOrigins { get; set; }
    public FormBotValidation? BotValidator { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
    public FormSubscription? Subscription { get; set; }
}

public class FormSubscription
{
    public required string Provider { get; set; }
    public required FormSubscriptionFields FieldReferences { get; set; }
}
public class FormSubscriptionFields
{
    public required string Email { get; set; }
    public string? Name { get; set; }
    public string? Lastname { get; set; }
    public string? Phone { get; set; }
}

public class FormBotValidation
{
    public required string Provider { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = [];
}

[JsonDerivedType(typeof(TextField), "text")]
[JsonDerivedType(typeof(IntField), "int")]
[JsonDerivedType(typeof(DoubleField), "double")]
[JsonDerivedType(typeof(DateOnlyField), "date")]
[JsonDerivedType(typeof(TimeOnlyField), "time")]
[JsonDerivedType(typeof(DateTimeField), "dateTime")]
[JsonDerivedType(typeof(EmailAddressField), "email")]
[JsonDerivedType(typeof(BooleanField), "boolean")]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
public class FormField
{
    public required string Name { get; set; }
    public bool IsRequired { get; set; }
}

public class TextField : FormField
{
    public uint? MinLen { get; set; }
    public uint? MaxLen { get; set; }
    public string? Regex { get; set; }
    public string[]? AllowedValues { get; set; }
}

public abstract class RangeField<T> : FormField where T : struct, IComparable<T>
{
    public T? Min { get; set; }
    public T? Max { get; set; }
}

public class IntField : RangeField<int>;

public class DoubleField : RangeField<double>;

public class DateOnlyField : RangeField<DateOnly>;

public class TimeOnlyField : RangeField<TimeOnly>;

public class DateTimeField : RangeField<DateTime>;

public class EmailAddressField : FormField;

public class BooleanField : FormField;