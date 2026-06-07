using Newtonsoft.Json.Schema;
using Orbyss.Blazor.JsonForms.Core.Interpretation;

namespace Orbyss.Blazor.JsonForms.Interpretation.Interfaces;

public interface IControlTypeInterpreter
{
    ControlType Interpret(JSchema jsonSchema, string absoluteControlJsonSchemaPath, string? absoluteControlParentSchemaJsonPath);
}