using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Orbyss.Blazor.JsonForms.Core.UiSchema;

public record UiSchemaElementRuleCondition(
    [property: JsonProperty(PropertyName = "scope"), JsonPropertyName("scope")] string Scope,
    [property: JsonProperty(PropertyName = "schema"), JsonPropertyName("schema")] object Schema
);
