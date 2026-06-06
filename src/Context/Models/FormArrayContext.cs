using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Orbyss.Blazor.JsonForms.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Interpretation;

namespace Orbyss.Blazor.JsonForms.Context.Models;

/// <summary>
/// Context for an <c>ArrayLayout</c> element.  Manages the ordered list of
/// <see cref="FormArrayItemContext"/> entries and validates the array against
/// JSON Schema <c>minItems</c> / <c>maxItems</c> constraints.
/// </summary>
public sealed class FormArrayContext(
    UiSchemaArrayLayoutInterpretation interpretation,
    string absoluteDataJsonPath,
    string? absoluteParentDataJsonPath)
    : FormControlContextBase<UiSchemaArrayLayoutInterpretation>(interpretation, absoluteDataJsonPath, absoluteParentDataJsonPath)
{
    private readonly List<FormArrayItemContext> items = [];

    // ── Public surface ────────────────────────────────────────────────────────

    /// <summary>A snapshot of the current item list.</summary>
    public FormArrayItemContext[] Items => [.. items];

    // ── Internal mutation API (called by JsonFormDataContext) ─────────────────

    internal void AddItem(FormArrayItemContext item) => items.Add(item);

    /// <summary>Removes all item contexts (used before a full rebuild after remove / move).</summary>
    internal void ClearItems() => items.Clear();

    /// <summary>
    /// Removes the item with the given id and returns its former index,
    /// so the caller knows which JArray index to delete.
    /// </summary>
    internal int RemoveItemById(Guid itemId)
    {
        for (var i = 0; i < items.Count; i++)
        {
            if (items[i].Id == itemId)
            {
                items.RemoveAt(i);
                return i;
            }
        }
        throw new InvalidOperationException($"Array does not contain an item with id '{itemId}'.");
    }

    // ── IFormElementContext ───────────────────────────────────────────────────

    public override bool FindDataPathBySchemaPath(string schemaPath, out string dataPath)
    {
        dataPath = string.Empty;
        if (Interpretation.AbsoluteSchemaJsonPath == schemaPath)
        {
            dataPath = AbsoluteDataJsonPath;
            return true;
        }
        return false;
    }

    public override bool Validate(JToken formData, JToken schema)
    {
        var errors = new List<ErrorType>();

        var arraySchemaToken = schema.SelectToken(Interpretation.AbsoluteSchemaJsonPath, false);
        if (arraySchemaToken is not null)
        {
            var arraySchema = JSchema.Parse($"{arraySchemaToken}");
            var arrayData   = formData.SelectToken(AbsoluteDataJsonPath, false);

            if (arrayData is JArray array)
            {
                if (arraySchema.MinimumItems.HasValue && array.Count < arraySchema.MinimumItems.Value)
                    errors.Add(ErrorType.MinimumItems);
                if (arraySchema.MaximumItems.HasValue && array.Count > arraySchema.MaximumItems.Value)
                    errors.Add(ErrorType.MaximumItems);
            }
        }

        Errors = [.. errors];

        // Validate every control inside every item
        var areItemsValid = ValidateElements(
            formData,
            schema,
            items.Select(i => i.ElementContext));

        return areItemsValid && !errors.Any();
    }
}
