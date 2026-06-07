using Newtonsoft.Json.Linq;

namespace Orbyss.Blazor.JsonForms.Core.Interpretation;

/// <summary>
/// Interpretation produced for <c>ArrayLayout</c> UI schema elements.
/// Holds the resolved paths, min/max item constraints, the "add" button label key, and the
/// per-item layout interpretation (explicit or auto-generated from JSON Schema properties).
/// </summary>
public sealed class UiSchemaArrayLayoutInterpretation(
    UiSchemaLabelInterpretation? labelInterpretation,
    bool readOnly,
    bool disabled,
    bool hidden,
    string relativeSchemaJsonPath,
    string absoluteSchemaJsonPath,
    string relativeItemsSchemaJsonPath,
    string absoluteItemsSchemaJsonPath,
    string arrayJsonPropertyName,
    string? absoluteParentObjectSchemaPath,
    string? addLabel,
    UiSchemaElementInterpretationBase itemsInterpretation,
    IReadOnlyDictionary<string, JToken?> options,
    UiSchemaRuleInterpretation? rule)

    : UiSchemaControlInterpretationBase(
        labelInterpretation,
        readOnly,
        disabled,
        hidden,
        relativeSchemaJsonPath,
        absoluteSchemaJsonPath,
        arrayJsonPropertyName,
        absoluteParentObjectSchemaPath,
        options,
        rule)
{
    public override UiSchemaElementInterpretationType ElementType => UiSchemaElementInterpretationType.ArrayLayout;

    /// <summary>Absolute JSON Schema path to the array's item schema (e.g. <c>$.properties.addresses.items</c>).</summary>
    public string AbsoluteItemsSchemaJsonPath { get; } = absoluteItemsSchemaJsonPath;

    /// <summary>Relative JSON Schema path to the items schema from the array's parent scope.</summary>
    public string RelativeItemsSchemaJsonPath { get; } = relativeItemsSchemaJsonPath;

    /// <summary>
    /// Translation key for the "add item" button label.  <c>null</c> means use the default <c>+</c>.
    /// </summary>
    public string? AddLabel { get; } = addLabel;

    /// <summary>The interpreted layout shown for each array item row.</summary>
    public UiSchemaElementInterpretationBase ItemsInterpretation { get; } = itemsInterpretation;
}
