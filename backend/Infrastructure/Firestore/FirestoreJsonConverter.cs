using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Cloud.Firestore;

namespace NetFormsManager.Infrastructure.Firestore;

public class FirestoreGuidConverter : IFirestoreConverter<Guid>
{
    public object ToFirestore(Guid value)
    {
        return value.ToString();
    }
    public Guid FromFirestore(object value)
    {
        return value is string text ? Guid.Parse(text) : Guid.Empty;
    }
}

public class FirestoreJsonConverter<T> : IFirestoreConverter<T>
{
    // ReSharper disable once StaticMemberInGenericType
    public object ToFirestore(T value)
    {
        return JsonSerializer.SerializeToElement(value, FirestoreJsonConverterDefaults.SerializerOptions)
            .EnumerateObject()
            .ToDictionary(x => x.Name, x => ConvertJsonElement(x.Value));

        static object ConvertJsonElement(JsonElement element) => element.ValueKind switch
        {
            JsonValueKind.String => element.GetString()!,
            JsonValueKind.Number => element.TryGetInt32(out var intValue) ? intValue :
                element.TryGetInt64(out var longValue) ? longValue :
                element.TryGetUInt32(out var uintValue) ? uintValue :
                element.TryGetDouble(out var doubleValue) ? doubleValue :
                element.TryGetDecimal(out var decimalValue) ? decimalValue :
                element.GetRawText(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null!,
            JsonValueKind.Array => element
                .EnumerateArray()
                .Select(ConvertJsonElement)
                .ToArray(),
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(
                p => p.Name,
                p => ConvertJsonElement(p.Value)
            ),
            _ => element.ToString()
        };
    }

    public T FromFirestore(object value)
        => value is Dictionary<string, object> map
            ? JsonSerializer
                  .SerializeToElement(map, FirestoreJsonConverterDefaults.SerializerOptions)
                  .Deserialize<T>(FirestoreJsonConverterDefaults.SerializerOptions) ??
              throw new Exception("Failed to parse object from dictionary")
            : throw new ArgumentException($"Expected a dictionary for {typeof(T).Name}", nameof(value));
}

public readonly struct FirestoreJsonConverterDefaults
{
    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}