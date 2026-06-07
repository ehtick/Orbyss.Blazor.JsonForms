using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orbyss.Blazor.JsonForms.Core.Utils;
using System.Text.Json.Serialization;

namespace Orbyss.Blazor.JsonForms.Core.UiSchema;

public sealed record FormUiSchemaElement(
       [property: JsonProperty(PropertyName = "type"), JsonPropertyName("type")] UiSchemaElementType Type,
       [property: JsonProperty(PropertyName = "label"), JsonPropertyName("label")] string? Label,
       [property: JsonProperty(PropertyName = "i18n"), JsonPropertyName("i18n")] string? I18n,
       [property: JsonProperty(PropertyName = "elements"), JsonPropertyName("elements")] FormUiSchemaElement[] Elements,
       [property: JsonProperty(PropertyName = "scope"), JsonPropertyName("scope")] string? Scope,
       [property: JsonProperty(PropertyName = "rule"), JsonPropertyName("rule")] UiSchemaElementRule? Rule,
       [property: JsonProperty(PropertyName = "options"), JsonPropertyName("options")] object? Options)
{
    /// <summary>
    /// Optional UI schema element describing the layout of a single array item.
    /// Used by <c>ArrayLayout</c> elements. When absent, a <c>HorizontalLayout</c> is
    /// auto-generated from the JSON Schema <c>items.properties</c>.
    /// </summary>
    [Newtonsoft.Json.JsonProperty(PropertyName = "items")]
    [System.Text.Json.Serialization.JsonPropertyName("items")]
    public FormUiSchemaElement? Items { get; init; }

    public bool HasOption(string key)
    {
        return OptionsReader.HasOption(Options, key);
    }

    public JToken GetOption(string key)
    {
        return OptionsReader.GetOption(Options, key);
    }

    public bool HasChildElements => Elements?.Length > 0;
}
