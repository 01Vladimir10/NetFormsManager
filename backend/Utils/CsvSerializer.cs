using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace NetMailGun.Utils;

public static class CsvSerializer
{
    public static async Task SerializeAsync<T>(Stream target, IEnumerable<T> data, char separator = ',')
    {
        var properties = typeof(T)
            .GetProperties()
            .Where(x => x.CanRead)
            .Select(property => (name: property.Name, getter: CreateGetter<T>(property)))
            .ToArray();
        await using var writer = new StreamWriter(target, Encoding.UTF8);

        var sb = new StringBuilder(1024 * 4);
        for (var i = 0; i < properties.Length; i++)
        {
            if (i > 0) sb.Append(separator);
            CsvEncode(sb, properties[i].name, separator);
        }

        sb.AppendLine();
        await writer.WriteAsync(sb.ToString());

        foreach (var item in data)
        {
            sb.Clear();
            for (var i = 0; i < properties.Length; i++)
            {
                var getter = properties[i].getter;
                if (i > 0) sb.Append(separator);
                CsvEncode(sb, getter(item), separator);
            }
            sb.AppendLine();
            await writer.WriteAsync(sb.ToString());
        }

        await writer.FlushAsync();
    }

    private static void CsvEncode(StringBuilder builder, ReadOnlySpan<char> data, char separator = ',')
    {
        var needsQuotes = data.ContainsAny([separator, '"', '\r', '\n']);

        if (!needsQuotes)
        {
            builder.Append(data);
            return;
        }

        const char quote = '"';
        builder.Append(quote);

        var start = 0;
        for (var i = 0; i < data.Length; i++)
        {
            if (data[i] != quote) continue;

            if (i > start)
                builder.Append(data.Slice(start, i - start));

            // Write doubled quote
            builder.Append(quote);
            builder.Append(quote);
            start = i + 1;
        }


        if (start < data.Length)
        {
            builder.Append(data[start..]);
        }

        builder.Append(quote);
    }

    private static Func<T, string> CreateGetter<T>(PropertyInfo property)
    {
        // Ensure the property can be accessed from T (supports base/interface declaration)
        if (property.DeclaringType is null || !property.DeclaringType.IsAssignableFrom(typeof(T)))
        {
            throw new ArgumentException(
                $"Property '{property.Name}' is not declared on '{typeof(T)}' or its base/interface types.",
                nameof(property)
            );
        }

        var instance = Expression.Parameter(typeof(T), "instance");
        Expression typedInstance = instance;

        if (property.DeclaringType != typeof(T))
        {
            typedInstance = Expression.Convert(typedInstance, property.DeclaringType);
        }

        var access = Expression.Property(typedInstance, property);
        Expression body;

        if (property.PropertyType == typeof(string))
        {
            body = Expression.Coalesce(access, Expression.Constant(string.Empty));
        }
        else if (Nullable.GetUnderlyingType(property.PropertyType) is { } underlying)
        {
            // Nullable<T>: p.HasValue ? p.Value.ToString() : string.Empty
            var hasValue = Expression.Property(access, nameof(Nullable<>.HasValue));
            var value = Expression.Property(access, nameof(Nullable<>.Value));
            var toStringCall = Expression.Call(value, underlying.GetMethod(nameof(ToString), Type.EmptyTypes)!);
            body = Expression.Condition(hasValue, toStringCall, Expression.Constant(string.Empty));
        }
        else if (!property.PropertyType.IsValueType)
        {
            // Reference type: p != null ? p.ToString() : string.Empty
            var notNull = Expression.NotEqual(access, Expression.Constant(null, property.PropertyType));
            var toStringCall =
                Expression.Call(access, property.PropertyType.GetMethod(nameof(ToString), Type.EmptyTypes)!);
            body = Expression.Condition(notNull, toStringCall, Expression.Constant(string.Empty));
        }
        else
        {
            // Non-nullable value type: p.ToString()
            body = Expression.Call(access, property.PropertyType.GetMethod(nameof(ToString), Type.EmptyTypes)!);
        }

        return Expression.Lambda<Func<T, string>>(body, instance).Compile();
    }
}