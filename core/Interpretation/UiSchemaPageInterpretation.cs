namespace Orbyss.Blazor.JsonForms.Interpretation;

public sealed class UiSchemaPageInterpretation(
    bool readOnly,
    bool disabled,
    bool hidden,
    UiSchemaElementInterpretationBase[] interpretedElements,
    UiSchemaLabelInterpretation? labelInterpretation,
    UiSchemaRuleInterpretation? rule)
{
    public UiSchemaPageInterpretation(
        bool readOnly,
        bool disabled,
        bool hidden,
        UiSchemaElementInterpretationBase interpretedElement,
        UiSchemaLabelInterpretation? labelInterpretation,
        UiSchemaRuleInterpretation? rule)

        : this(readOnly, disabled, hidden, [interpretedElement], labelInterpretation, rule)
    {
    }

    public UiSchemaElementInterpretationBase[] InterpretedElements { get; } = interpretedElements;

    public UiSchemaLabelInterpretation? LabelInterpretation { get; } = labelInterpretation;

    public UiSchemaRuleInterpretation? Rule { get; } = rule;

    public bool ReadOnly => readOnly;

    public bool Disabled => disabled;

    public bool Hidden => hidden;
}
