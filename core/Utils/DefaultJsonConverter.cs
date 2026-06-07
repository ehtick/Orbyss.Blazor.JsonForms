using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Orbyss.Blazor.JsonForms.Core.Utils;

public static class DefaultJsonConverter
{
    public static TValue Deserialize<TValue>(string json)
    {
        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new StringEnumConverter());
        return JsonConvert.DeserializeObject<TValue>(json, settings)!;
    }

    public static object Deserialize(string json, Type targetType)
    {
        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new StringEnumConverter());
        return JsonConvert.DeserializeObject(json, targetType, settings)!;
    }
}
