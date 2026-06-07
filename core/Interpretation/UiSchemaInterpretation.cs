namespace Orbyss.Blazor.JsonForms.Core.Interpretation;

/// <summary>
/// The fully-interpreted form structure produced by <c>IFormUiSchemaInterpreter</c>.
/// Contains the page interpretations that the engine uses to create form element contexts.
/// </summary>
public sealed class UiSchemaInterpretation(UiSchemaPageInterpretation[] pages)
{
    public UiSchemaInterpretation(UiSchemaPageInterpretation page)
        : this([page])
    { }

    public UiSchemaPageInterpretation[] Pages { get; } = pages;
}
