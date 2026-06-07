using Newtonsoft.Json.Linq;

namespace Orbyss.Blazor.JsonForms.Core.Interpretation;

public sealed class UiSchemaActionButtonInterpretation(
    UiSchemaLabelInterpretation labelInterpretation,
    bool disabled,
    bool hidden,
    string actionKey,
    IReadOnlyDictionary<string, JToken?> options,
    UiSchemaRuleInterpretation? rule)
    : UiSchemaElementInterpretationBase(labelInterpretation)
{
    private readonly IReadOnlyDictionary<string, JToken?> _options = options;

    public override UiSchemaElementInterpretationType ElementType => UiSchemaElementInterpretationType.ActionButton;

    /// <summary>
    /// Maps to the key registered via <c>JsonFormOptions.RegisterAction</c>.
    /// </summary>
    public string ActionKey { get; } = actionKey;

    public bool Disabled { get; } = disabled;

    public bool Hidden { get; } = hidden;

    public UiSchemaRuleInterpretation? Rule { get; } = rule;

    public JToken? GetOption(string key)
        => _options.TryGetValue(key, out var value) ? value : null;
}
