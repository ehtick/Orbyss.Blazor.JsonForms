using Newtonsoft.Json.Linq;
using Orbyss.Blazor.JsonForms.Interpretation;
using Orbyss.Components.Json.Models;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Orbyss.Blazor.JsonForms.Constants;

/// <summary>
/// Maps <see cref="ControlType"/> enum values to their corresponding CLR types,
/// and converts raw JToken form data to the correct CLR type for each control.
/// </summary>
public static class ControlTypeLookup
{
    // Field names MUST match ControlType enum member names exactly (case-insensitive parse).
    public static readonly Type Enum              = typeof(string);
    public static readonly Type EnumList          = typeof(IEnumerable<string>);
    public static readonly Type DateTime          = typeof(DateTime?);
    public static readonly Type DateTimeUtcTicks  = typeof(DateTimeUtcTicks?);
    public static readonly Type DateOnly          = typeof(DateOnly?);
    public static readonly Type DateOnlyUtcTicks  = typeof(DateUtcTicks?);
    public static readonly Type Number            = typeof(double?);
    public static readonly Type Integer           = typeof(int?);
    public static readonly Type String            = typeof(string);
    public static readonly Type Boolean           = typeof(bool);

    private static readonly ReadOnlyDictionary<ControlType, Type> _typePerControlType =
        typeof(ControlTypeLookup)
            .GetFields(BindingFlags.Static | BindingFlags.Public)
            .ToDictionary(
                f => System.Enum.Parse<ControlType>(f.Name, ignoreCase: true),
                f => (Type)f.GetValue(null)!)
            .AsReadOnly();

    /// <summary>Returns the CLR type for the given <see cref="ControlType"/>.</summary>
    /// <exception cref="NotSupportedException">When the control type has no registered CLR type.</exception>
    public static Type GetForControlType(ControlType controlType)
    {
        if (!_typePerControlType.TryGetValue(controlType, out var result))
            throw new NotSupportedException($"Cannot look up CLR type for control type '{controlType}'.");
        return result;
    }

    /// <summary>
    /// Converts a raw JSON token from the form data store to the CLR value expected by
    /// the component registered for <paramref name="controlType"/>.
    /// Returns <c>null</c> (or <c>false</c> for Boolean) when <paramref name="token"/> is null or JSON null.
    /// </summary>
    public static object? ConvertFromJToken(JToken? token, ControlType controlType)
    {
        if (token is null || token.Type == JTokenType.Null)
            return controlType == ControlType.Boolean ? false : (object?)null;

        return controlType switch
        {
            ControlType.String            => token.ToString(),
            ControlType.Number            => token.ToObject<double?>(),
            ControlType.Integer           => token.ToObject<int?>(),
            ControlType.Boolean           => token.ToObject<bool>(),
            ControlType.Enum              => token.ToString(),
            ControlType.EnumList          => token.ToObject<IEnumerable<string>>(),
            ControlType.DateTime          => token.ToObject<DateTime?>(),
            ControlType.DateTimeUtcTicks  => ConvertDateTimeUtcTicks(token),
            ControlType.DateOnly          => ConvertDateOnly(token),
            ControlType.DateOnlyUtcTicks  => ConvertDateOnlyUtcTicks(token),
            _ => throw new NotSupportedException($"No value conversion defined for control type '{controlType}'.")
        };
    }

    // ── Helpers for custom date/time types ───────────────────────────────────

    private static DateTimeUtcTicks? ConvertDateTimeUtcTicks(JToken token)
    {
        var raw = $"{token}";
        return string.IsNullOrWhiteSpace(raw) ? null : new DateTimeUtcTicks((long)token);
    }

    private static DateOnly? ConvertDateOnly(JToken token)
    {
        var raw = $"{token}";
        return string.IsNullOrWhiteSpace(raw)
            ? null
            : System.DateOnly.ParseExact(raw, "yyyy-MM-dd");
    }

    private static DateUtcTicks? ConvertDateOnlyUtcTicks(JToken token)
    {
        var raw = $"{token}";
        return string.IsNullOrWhiteSpace(raw) ? null : new DateUtcTicks((long)token);
    }
}
