using Newtonsoft.Json.Linq;
using Orbyss.Blazor.JsonForms.Core.Context.Models;
using Orbyss.Blazor.JsonForms.Core.Models;

namespace Orbyss.Blazor.JsonForms.Core.Context.Interfaces;

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

    string? GetArrayAddLabel(Guid arrayContextId);

    IEnumerable<TranslatedEnumItem> GetTranslatedEnumItems(Guid controlContextId);

    FormPageContext GetPage(int index);

    void InstantiateList(Guid listContextId);

    void AddListItem(Guid listContextId);

    void RemoveListItem(Guid listContextId, Guid listItemContextId);

    void InstantiateArray(Guid arrayContextId);

    void AddArrayItem(Guid arrayContextId);

    /// <summary>Appends a new array item seeded with <paramref name="itemData"/> (e.g. a dialog-based add).</summary>
    void AddArrayItem(Guid arrayContextId, JToken itemData);

    void RemoveArrayItem(Guid arrayContextId, Guid arrayItemId);

    /// <summary>Replaces the data of an existing array item (e.g. a dialog-based edit).</summary>
    void UpdateArrayItem(Guid arrayContextId, Guid arrayItemId, JToken itemData);

    /// <summary>Returns the current JSON data of an array item, for pre-filling a dialog-based edit.</summary>
    JToken? GetArrayItemData(Guid arrayContextId, Guid arrayItemId);

    void MoveArrayItem(Guid arrayContextId, int fromIndex, int toIndex);

    Task NotifyArrayItemAdded(Guid arrayContextId, int addedIndex);

    Task NotifyArrayItemRemoved(Guid arrayContextId, int removedIndex);

    Task NotifyArrayItemMoved(Guid arrayContextId, int fromIndex, int toIndex);

    Task NotifyArrayItemUpdated(Guid arrayContextId, int updatedIndex);

    void ChangeLanguage(string language);

    void ChangeDisabled(bool disabled);

    void ChangeReadOnly(bool readOnly);

    FormControlContext? FindControl(Func<FormControlContext, bool> predicate);

    IEnumerable<FormControlContext> FindControls(Func<FormControlContext, bool> predicate);

    Task NotifyControlValueChanged(Guid controlContextId);

    Task NotifyControlInputChanged(Guid controlContextId);

    Task InvokeAction(string actionKey);
}
