namespace Orbyss.Blazor.JsonForms.Interpretation;

/// <summary>
/// Abstract base for all UI schema element interpretations.
/// Plugins work with concrete subtypes (e.g. <see cref="UiSchemaControlInterpretation"/>) — cast
/// based on <see cref="ElementType"/> or by checking the concrete context type.
/// </summary>
public abstract class UiSchemaElementInterpretationBase(UiSchemaLabelInterpretation? label)
{
    public UiSchemaLabelInterpretation? Label { get; } = label;

    public abstract UiSchemaElementInterpretationType ElementType { get; }
}
