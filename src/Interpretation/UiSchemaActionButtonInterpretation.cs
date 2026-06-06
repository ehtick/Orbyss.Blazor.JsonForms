using Newtonsoft.Json.Linq;
using Orbyss.Blazor.JsonForms.UiSchema;

namespace Orbyss.Blazor.JsonForms.Interpretation;

public sealed class UiSchemaActionButtonInterpretation(
    UiSchemaLabelInterpretation labelInterpretation,
    bool disabled,
    bool hidden,
    string actionKey,
    FormUiSchemaElement element,
    UiSchemaRuleInterpretation? rule)
    : UiSchemaElementInterpretationBase(labelInterpretation)
{
    public override UiSchemaElementInterpretationType ElementType => UiSchemaElementInterpretationType.ActionButton;

    /// <summary>
    /// Maps to the key registered via <c>JsonFormContextInitOptions.RegisterAction</c>.
    /// </summary>
    public string ActionKey { get; } = actionKey;

    public bool Disabled { get; } = disabled;

    public bool Hidden { get; } = hidden;

    public UiSchemaRuleInterpretation? Rule { get; } = rule;

    public JToken? GetOption(string key)
        => element.HasOption(key) ? element.GetOption(key) : null;
}
