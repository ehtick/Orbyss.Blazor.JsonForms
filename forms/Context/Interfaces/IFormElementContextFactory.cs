using Orbyss.Blazor.JsonForms.Core.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Core.Context.Models;
using Orbyss.Blazor.JsonForms.Core.Interpretation;

namespace Orbyss.Blazor.JsonForms.Context.Interfaces;

public interface IFormElementContextFactory
{
    IFormElementContext Create(UiSchemaElementInterpretationBase interpretation, string? parentAbsoluteDataJsonPath);

    FormPageContext[] CreatePages(UiSchemaPageInterpretation[] pageInterpretations);
}