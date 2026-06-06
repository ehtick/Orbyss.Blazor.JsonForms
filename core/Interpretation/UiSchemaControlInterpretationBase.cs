using Newtonsoft.Json.Linq;

namespace Orbyss.Blazor.JsonForms.Interpretation;

/// <summary>
/// Abstract base for all control-like element interpretations (controls, lists, arrays, action buttons).
/// Holds resolved schema paths, visibility/state flags, rule, and the element's options as a
/// pre-extracted dictionary so interpretation models are self-contained with no dependency on the
/// input UI schema element.
/// </summary>
public abstract class UiSchemaControlInterpretationBase(
    UiSchemaLabelInterpretation? labelInterpretation,
    bool readOnly,
    bool disabled,
    bool hidden,
    string relativeSchemaJsonPath,
    string absoluteSchemaJsonPath,
    string jsonPropertyName,
    string? absoluteParentSchemaJsonPath,
    IReadOnlyDictionary<string, JToken?> options,
    UiSchemaRuleInterpretation? rule)
    : UiSchemaElementInterpretationBase(labelInterpretation)
{
    private readonly IReadOnlyDictionary<string, JToken?> _options = options;

    public string AbsoluteSchemaJsonPath { get; } = absoluteSchemaJsonPath;

    public string JsonPropertyName { get; } = jsonPropertyName;

    public string? AbsoluteParentSchemaJsonPath { get; } = absoluteParentSchemaJsonPath;

    public bool ReadOnly { get; } = readOnly;

    public bool Disabled { get; } = disabled;

    public bool Hidden { get; } = hidden;

    public string RelativeSchemaJsonPath { get; } = relativeSchemaJsonPath;

    public UiSchemaRuleInterpretation? Rule { get; } = rule;

    /// <summary>
    /// Returns the option value for the given key, or <c>null</c> if the option is not set.
    /// Options are extracted from the UI schema element at interpretation time.
    /// </summary>
    public JToken? GetOption(string key)
        => _options.TryGetValue(key, out var value) ? value : null;
}
