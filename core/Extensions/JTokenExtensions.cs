using Newtonsoft.Json.Linq;

namespace Orbyss.Blazor.JsonForms.Core.Extensions;

public static class JTokenExtensions
{
    public static object ToDotnetObject(this JToken jToken)
    {        
        return jToken switch
        {
            JValue jsonvalue => jsonvalue.ToDotnetValue(),
            JObject jsonObject => jsonObject.ToDotnetObject(),
            JArray jsonArray => jsonArray.ToDotnetList(),

            _ => throw new NotSupportedException($"Unsupported JSON token type: {jToken.Type}")
        };
    }

    public static List<object> ToDotnetList(this JArray jsonArray)
    {
        var result = new List<object>();

        if (jsonArray.Count == 0)
        {
            return result;
        }

        foreach (var item in jsonArray)
        {
            result.Add(
                item.ToDotnetObject()
            );
        }

        return result;
    }

    public static Dictionary<string, object> ToDotnetObject(this JObject jsonObject)
    {
        var result = new Dictionary<string, object>();

        if(jsonObject.Count == 0)
        {
            return result;
        }

        foreach(var property in jsonObject.Properties())
        {
            result[property.Name] = property.Value.ToDotnetObject();
        }

        return result;
    }


    public static object ToDotnetValue(this JValue jsonValue)
    {
        return jsonValue.Type switch
        {
            JTokenType.String => jsonValue.Value<string>()!,
            JTokenType.Integer => jsonValue.Value<long>(),
            JTokenType.TimeSpan => jsonValue.Value<TimeSpan>(),
            JTokenType.Guid => jsonValue.Value<Guid>(),
            JTokenType.Float => jsonValue.Value<double>(),
            JTokenType.Boolean => jsonValue.Value<bool>(),
            JTokenType.Date => jsonValue.Value<DateTimeOffset>(),
            JTokenType.Null => null!,
            _ => throw new NotSupportedException($"Unsupported JSON value type: {jsonValue.Type}")
        };
    }
}
