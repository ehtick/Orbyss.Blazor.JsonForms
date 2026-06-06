using Newtonsoft.Json.Linq;
using Orbyss.Blazor.JsonForms.Context.Models;
using Orbyss.Components.Json.Models;

namespace Orbyss.Blazor.JsonForms.Context.Interfaces;

public interface IJsonFormContext
{
    IEnumerable<FormPageContext> GetPages();

    string? ActiveLanguage { get; }

    IJsonFormNotification FormNotification { get; }

    int PageCount { get; }

    bool Disabled { get; }

    bool ReadOnly { get; }

    void Instantiate(JsonFormContextOptions initOptions);

    JToken? GetFormOption(string key);

    bool Validate(Guid? pageId = null);

    JToken? GetValue(Guid controlContextId);

    void UpdateValue(Guid controlContextId, JToken? value);

    JToken GetFormData();

    void UpdateFormData(Action<JToken> updateDelegate);

    string? GetDataContextError(Guid controlContextId);

    string? GetLabel(Guid contextId);

    string? GetHelperIconText(Guid controlContextId);

    string? GetHelperText(Guid controlContextId);

    string? GetPrefixText(Guid controlContextId);

    string? GetSuffixText(Guid controlContextId);

    string? GetCssClass(Guid elementContextId);

    /// <summary>
    /// Resolves the "add item" label for an <c>ArrayLayout</c> element through the translation schema.
    /// Returns <c>null</c> when no <c>addLabel</c> option is set; the component should default to <c>+</c>.
    /// </summary>
    string? GetArrayAddLabel(Guid arrayContextId);

    IEnumerable<TranslatedEnumItem> GetTranslatedEnumItems(Guid controlContextId);

    FormPageContext GetPage(int index);

    void InstantiateList(Guid listContextId);

    void AddListItem(Guid listContextId);

    void RemoveListItem(Guid listContextId, Guid listItemContextId);

    // ── Array (inline repeater) ───────────────────────────────────────────────

    /// <summary>
    /// Populates the array context from existing JSON data.  Call once on first render.
    /// </summary>
    void InstantiateArray(Guid arrayContextId);

    /// <summary>Appends a new empty item to the array and triggers a UI refresh.</summary>
    void AddArrayItem(Guid arrayContextId);

    /// <summary>Removes the item identified by <paramref name="arrayItemId"/> and triggers a UI refresh.</summary>
    void RemoveArrayItem(Guid arrayContextId, Guid arrayItemId);

    /// <summary>
    /// Moves the item at <paramref name="fromIndex"/> to <paramref name="toIndex"/> (drag-to-reorder)
    /// and triggers a UI refresh.
    /// </summary>
    void MoveArrayItem(Guid arrayContextId, int fromIndex, int toIndex);

    // ── Array notifications ───────────────────────────────────────────────────

    /// <summary>Fires the <c>OnArrayItemAdded</c> event registered on <c>JsonFormContextOptions</c>.</summary>
    Task NotifyArrayItemAdded(Guid arrayContextId, int addedIndex);

    /// <summary>Fires the <c>OnArrayItemRemoved</c> event registered on <c>JsonFormContextOptions</c>.</summary>
    Task NotifyArrayItemRemoved(Guid arrayContextId, int removedIndex);

    /// <summary>Fires the <c>OnArrayItemMoved</c> event registered on <c>JsonFormContextOptions</c>.</summary>
    Task NotifyArrayItemMoved(Guid arrayContextId, int fromIndex, int toIndex);

    void ChangeLanguage(string language);

    void ChangeDisabled(bool disabled);

    void ChangeReadOnly(bool readOnly);

    /// <summary>
    /// Returns the first control context that matches the predicate, or null if none match.
    /// Useful for locating a target context before calling <see cref="UpdateValue"/>.
    /// </summary>
    FormControlContext? FindControl(Func<FormControlContext, bool> predicate);

    /// <summary>
    /// Returns all control contexts that match the predicate.
    /// </summary>
    IEnumerable<FormControlContext> FindControls(Func<FormControlContext, bool> predicate);

    /// <summary>
    /// Called by the form engine after a control's committed value changes.
    /// Fires any <see cref="JsonFormContextOptions.OnControlValueChanged"/> subscribers.
    /// </summary>
    Task NotifyControlValueChanged(Guid controlContextId);

    /// <summary>
    /// Called by the form engine after a control receives a raw input event.
    /// Fires any <see cref="JsonFormContextOptions.OnControlInputChanged"/> subscribers.
    /// </summary>
    Task NotifyControlInputChanged(Guid controlContextId);

    /// <summary>
    /// Invokes the action handler registered for the given key via
    /// <see cref="JsonFormContextOptions.RegisterAction"/>.
    /// Called by the engine when an ActionButton is clicked.
    /// </summary>
    Task InvokeAction(string actionKey);
}