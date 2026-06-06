using Newtonsoft.Json.Linq;
using Orbyss.Blazor.JsonForms.Interpretation;

namespace Orbyss.Blazor.JsonForms.Context.Models;

public sealed class FormActionButtonContext : FormElementContextBase<UiSchemaActionButtonInterpretation>
{
    private readonly UiSchemaActionButtonInterpretation _interpretation;

    public FormActionButtonContext(UiSchemaActionButtonInterpretation interpretation)
        : base(interpretation)
    {
        _interpretation = interpretation;
    }

    public override bool ReadOnly => false;

    protected override bool DisabledCore => _interpretation.Disabled;

    protected override bool HiddenCore => _interpretation.Hidden;

    public override bool FindDataPathBySchemaPath(string schemaPath, out string dataPath)
    {
        dataPath = string.Empty;
        return false;
    }

    public override bool Validate(JToken formData, JToken schema)
        => true; // Action buttons carry no data; they are always "valid"
}
