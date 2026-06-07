using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Orbyss.Blazor.JsonForms.Core.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Core.Context.Models;

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
    /// Appends a new item seeded with <paramref name="itemData"/> (deep-cloned) to the array
    /// in both JSON data and the context list. Use this to land the result of a dialog-based add.
    /// </summary>
    void AddArrayItem(FormArrayContext arrayContext, JToken itemData);

    /// <summary>
    /// Replaces the data of the item identified by <paramref name="itemId"/> with
    /// <paramref name="itemData"/> (deep-cloned) and rebuilds the item contexts so their data paths
    /// stay in sync. Use this to land the result of a dialog-based edit.
    /// </summary>
    void UpdateArrayItem(FormArrayContext arrayContext, Guid itemId, JToken itemData);

    /// <summary>
    /// Returns the current JSON data of the item identified by <paramref name="itemId"/>,
    /// or <c>null</c> when the item does not exist. Use this to pre-fill a dialog-based edit.
    /// </summary>
    JToken? GetArrayItemData(FormArrayContext arrayContext, Guid itemId);

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