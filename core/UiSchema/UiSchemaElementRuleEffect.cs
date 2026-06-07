using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace Orbyss.Blazor.JsonForms.Core.UiSchema;

[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UiSchemaElementRuleEffect
{
    Show,
    Hide,
    Disable,
    Enable
}
