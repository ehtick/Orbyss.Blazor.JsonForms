using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Orbyss.Blazor.JsonForms.Context.Models;

namespace Orbyss.Blazor.JsonForms.Context.Interfaces;

public interface IJsonFormDataContext
{
    JSchema GetJsonSchema();

    void Instantiate(JToken formData, JSchema dataSchema);

    bool Validate(IEnumerable<IFormElementContext> contexts);

    JToken? GetValue(FormControlContext formControlContext);

    void UpdateValue(FormControlContext formControlContext, JToken? value);

    JToken GetFormData();

    void AddListItem(FormListContext listContext);

    void RemoveListItem(FormListContext listContext, IFormElementContext listItemContext);

    void InstantiateList(FormListContext listContext);

    // ── Array (inline repeater) ───────────────────────────────────────────────

    /// <summary>
    /// Populates <paramref name="arrayContext"/> from existing JSON array data.
    /// Should be called once on first render, mirroring <see cref="InstantiateList"/>.
    /// </summary>
    void InstantiateArray(FormArrayContext arrayContext);

    /// <summary>Appends a new empty item to the array in both JSON data and the context list.</summary>
    void AddArrayItem(FormArrayContext arrayContext);

    /// <summary>
    /// Removes the item identified by <paramref name="itemId"/> from both JSON data and the context list,
    /// then rebuilds all remaining item contexts so their data paths stay in sync with the new indices.
    /// </summary>
    void RemoveArrayItem(FormArrayContext arrayContext, Guid itemId);

    /// <summary>
    /// Moves the item at <paramref name="fromIndex"/> to <paramref name="toIndex"/> in both JSON data and
    /// the context list, then rebuilds all item contexts.
    /// </summary>
    void MoveArrayItem(FormArrayContext arrayContext, int fromIndex, int toIndex);
}