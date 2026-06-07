using Newtonsoft.Json.Linq;

namespace Orbyss.Blazor.JsonForms.Core.Interpretation;

public sealed class UiSchemaControlInterpretation(
    ControlType controlType,
    UiSchemaLabelInterpretation labelInterpretation,
    bool readOnly,
    bool disabled,
    bool hidden,
    string relativeSchemaJsonPath,
    string absoluteSchemaJsonPath,
    string controlJsonPropertyName,
    string? absoluteParentObjectSchemaPath,
    IReadOnlyDictionary<string, JToken?> options,
    UiSchemaRuleInterpretation? rule,
    double? minimum = null,
    double? maximum = null)

    : UiSchemaControlInterpretationBase(
        labelInterpretation,
        readOnly,
        disabled,
        hidden,
        relativeSchemaJsonPath,
        absoluteSchemaJsonPath,
        controlJsonPropertyName,
        absoluteParentObjectSchemaPath,
        options,
        rule)
{
    public override UiSchemaElementInterpretationType ElementType => UiSchemaElementInterpretationType.Control;

    public ControlType ControlType { get; } = controlType;

    public double? Minimum { get; } = minimum;

    public double? Maximum { get; } = maximum;
}
