using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Orbyss.Blazor.JsonForms.Core.Interpretation;

namespace Orbyss.Blazor.JsonForms.Core.Context.Models;

public abstract class FormControlContextBase<TInterpretation>(
    TInterpretation interpretation,
    string absoluteDataJsonPath,
    string? absoluteParentDataJsonPath)
    : FormElementContextBase<TInterpretation>(interpretation)
    where TInterpretation : UiSchemaControlInterpretationBase
{
    public string AbsoluteDataJsonPath { get; } = absoluteDataJsonPath;

    public IEnumerable<ErrorType> Errors { get; protected set; } = [];

    public string? AbsoluteParentObjectDataJsonPath { get; } = absoluteParentDataJsonPath;

    protected override sealed bool DisabledCore => Interpretation.Disabled;

    protected override bool HiddenCore => Interpretation.Hidden;

    public override sealed bool ReadOnly => Interpretation.ReadOnly;

    protected bool GetIsPropertyRequired(JToken formData, JToken schema, string? absoluteParentDataJsonPath, out JToken? parentData)
    {
        parentData = null;

        if (absoluteParentDataJsonPath is null)
        {
            return false;
        }

        parentData = formData.SelectToken(absoluteParentDataJsonPath, false);

        if (parentData is null)
        {
            return false;
        }

        var parentSchemaPath = Interpretation.AbsoluteParentSchemaJsonPath;

        if (parentSchemaPath is null)
        {
            return false;
        }

        var parentSchemaToken = schema.SelectToken(parentSchemaPath, false);

        if (parentSchemaToken is null)
        {
            return false;
        }

        var parentSchema = JSchema.Parse($"{parentSchemaToken}");

        return parentSchema.Required.Contains(Interpretation.JsonPropertyName);
    }
}
