using Orbyss.Blazor.JsonForms.UiSchema;

namespace Orbyss.Blazor.JsonForms.Interpretation;

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
    FormUiSchemaElement element,
    UiSchemaRuleInterpretation? rule,
    double? minimum = null,
    double? maximum = null)

    : UiSchemaControlInterpretationBase(labelInterpretation, readOnly, disabled, hidden, relativeSchemaJsonPath, absoluteSchemaJsonPath, controlJsonPropertyName, absoluteParentObjectSchemaPath, element, rule)
{
    public override UiSchemaElementInterpretationType ElementType => UiSchemaElementInterpretationType.Control;

    public ControlType ControlType { get; } = controlType;

    public double? Minimum { get; } = minimum;

    public double? Maximum { get; } = maximum;
}