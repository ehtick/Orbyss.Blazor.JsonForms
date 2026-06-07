using Newtonsoft.Json.Schema;
using Orbyss.Blazor.JsonForms.Core.Interpretation;
using Orbyss.Blazor.JsonForms.Core.UiSchema;

namespace Orbyss.Blazor.JsonForms.Interpretation.Interfaces;

public interface IFormUiSchemaInterpreter
{
    UiSchemaInterpretation Interpret(FormUiSchema uiSchema, JSchema jsonSchema);
}