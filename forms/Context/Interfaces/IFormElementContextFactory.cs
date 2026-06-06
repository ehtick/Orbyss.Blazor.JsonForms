using Orbyss.Blazor.JsonForms.Context.Models;
using Orbyss.Blazor.JsonForms.Interpretation;

namespace Orbyss.Blazor.JsonForms.Context.Interfaces;

public interface IFormElementContextFactory
{
    IFormElementContext Create(UiSchemaElementInterpretationBase interpretation, string? parentAbsoluteDataJsonPath);

    FormPageContext[] CreatePages(UiSchemaPageInterpretation[] pageInterpretations);
}