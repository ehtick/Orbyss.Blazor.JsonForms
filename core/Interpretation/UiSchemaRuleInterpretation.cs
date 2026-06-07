using Newtonsoft.Json.Schema;
using Orbyss.Blazor.JsonForms.Core.UiSchema;

namespace Orbyss.Blazor.JsonForms.Core.Interpretation;

public sealed class UiSchemaRuleInterpretation(
    string absoluteJsonSchemaPath,
    JSchema schema,
    UiSchemaElementRuleEffect effect)
{
    public string AbsoluteJsonSchemaPath { get; } = absoluteJsonSchemaPath;

    public JSchema Schema { get; } = schema;

    public UiSchemaElementRuleEffect Effect { get; } = effect;
}
