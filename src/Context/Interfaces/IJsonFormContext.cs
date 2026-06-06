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

    void Instantiate(JsonFormContextInitOptions initOptions);

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

    string? GetCssClass(Guid controlContextId);

    IEnumerable<TranslatedEnumItem> GetTranslatedEnumItems(Guid controlContextId);

    FormPageContext GetPage(int index);

    void InstantiateList(Guid listContextId);

    void AddListItem(Guid listContextId);

    void RemoveListItem(Guid listContextId, Guid listItemContextId);

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
    /// Fires any <see cref="JsonFormContextInitOptions.OnControlValueChanged"/> subscribers.
    /// </summary>
    Task NotifyControlValueChanged(Guid controlContextId);

    /// <summary>
    /// Called by the form engine after a control receives a raw input event.
    /// Fires any <see cref="JsonFormContextInitOptions.OnControlInputChanged"/> subscribers.
    /// </summary>
    Task NotifyControlInputChanged(Guid controlContextId);
}