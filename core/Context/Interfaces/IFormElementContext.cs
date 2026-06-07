using Newtonsoft.Json.Linq;
using Orbyss.Blazor.JsonForms.Core.Interpretation;

namespace Orbyss.Blazor.JsonForms.Core.Context.Interfaces;

public interface IFormElementContext
{
    Guid Id { get; }

    bool Validate(JToken formData, JToken schema);

    /// <summary>
    /// The interpretation model for this element.
    /// Cast to a concrete type (e.g. <see cref="UiSchemaControlInterpretation"/>) based on
    /// <see cref="UiSchemaElementInterpretationBase.ElementType"/>.
    /// </summary>
    UiSchemaElementInterpretationBase Interpretation { get; }

    bool FindDataPathBySchemaPath(string schemaPath, out string dataPath);

    bool Disabled { get; }

    bool ReadOnly { get; }

    bool Hidden { get; }

    void SetHidden(bool? value);

    void SetDisabled(bool? value);
}
