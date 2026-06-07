using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Orbyss.Blazor.JsonForms.Core.ComponentFactory;



using Orbyss.Blazor.JsonForms.Core.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Core.Context.Models;
using Orbyss.Blazor.JsonForms.Core.Models;
using Orbyss.Blazor.JsonForms.Core.UiSchema;
using Orbyss.Blazor.JsonForms.Core.Utils;

namespace Orbyss.Blazor.JsonForms.Core;

/// <summary>
/// Handler delegate invoked when a control's committed value changes or when raw input changes.
/// </summary>
/// <param name="control">The control whose value changed.</param>
/// <param name="form">The form context, used to read/write other control values.</param>
public delegate Task FormControlEventHandler(FormControlContext control, IJsonFormContext form);

/// <summary>
/// Handler delegate invoked when an <c>ActionButton</c> element is clicked.
/// </summary>
/// <param name="form">The form context, used to read/write control values.</param>
public delegate Task FormActionHandler(IJsonFormContext form);

/// <summary>
/// Fired after a new item has been appended to an <c>ArrayLayout</c>.
/// </summary>
public delegate Task ArrayItemAddedHandler(FormArrayContext arrayContext, int addedIndex, IJsonFormContext form);

/// <summary>
/// Fired after an item has been removed from an <c>ArrayLayout</c>.
/// </summary>
public delegate Task ArrayItemRemovedHandler(FormArrayContext arrayContext, int removedIndex, IJsonFormContext form);

/// <summary>
/// Fired after an item has been moved within an <c>ArrayLayout</c> (drag-to-reorder).
/// </summary>
public delegate Task ArrayItemMovedHandler(FormArrayContext arrayContext, int fromIndex, int toIndex, IJsonFormContext form);

/// <summary>
/// Fired after an item's data has been replaced in an <c>ArrayLayout</c>
/// (e.g. after a dialog-based edit committed via <c>UpdateArrayItem</c>).
/// </summary>
public delegate Task ArrayItemUpdatedHandler(FormArrayContext arrayContext, int updatedIndex, IJsonFormContext form);

public sealed class JsonFormOptions
{
    public JsonFormOptions(JSchema dataSchema, FormUiSchema uiSchema, TranslationSchema translationSchema)
    {
        DataSchema = dataSchema;
        UiSchema = uiSchema;
        TranslationSchema = translationSchema;
    }

    public JsonFormOptions(string dataSchemaJson, string uiSchemaJson, string translationSchemaJson)
    {
        DataSchema = JSchema.Parse(dataSchemaJson);
        UiSchema = DefaultJsonConverter.Deserialize<FormUiSchema>(uiSchemaJson);
        TranslationSchema = DefaultJsonConverter.Deserialize<TranslationSchema>(translationSchemaJson);
    }

    public JSchema DataSchema { get; }
    public FormUiSchema UiSchema { get; }
    public TranslationSchema TranslationSchema { get; }
    public JToken? Data { get; init; }
    public string? Language { get; init; }
    public bool Disabled { get; init; }
    public bool ReadOnly { get; init; }

    /// <summary>
    /// Per-form factory configuration. Runs on this form's own (transient) sub-factory instances,
    /// on top of the application defaults set in <c>AddJsonForms</c>, so a single form can override a
    /// component type, register an alias, or add parameters without affecting other forms.
    /// Ignored when <see cref="ComponentFactory"/> is supplied (that is full manual control).
    /// </summary>    
    public Action<FormComponentFactoryOptions>? ConfigureFactories { get; set; }

    /// <summary>
    /// Fired when a control's committed value changes (e.g. on blur, selection, or toggle).
    /// Subscribe with += to add multiple handlers.
    /// </summary>
    public event FormControlEventHandler? OnControlValueChanged;

    /// <summary>
    /// Fired on every raw input event (e.g. each keystroke in a text field).
    /// Subscribe with +=.
    /// </summary>
    public event FormControlEventHandler? OnControlInputChanged;

    public Task InvokeControlValueChanged(FormControlContext control, IJsonFormContext form)
        => OnControlValueChanged?.Invoke(control, form) ?? Task.CompletedTask;

    public Task InvokeControlInputChanged(FormControlContext control, IJsonFormContext form)
        => OnControlInputChanged?.Invoke(control, form) ?? Task.CompletedTask;

    // ── Array events ──────────────────────────────────────────────────────────

    /// <summary>Fired after a new item has been appended to an <c>ArrayLayout</c>.</summary>
    public event ArrayItemAddedHandler? OnArrayItemAdded;

    /// <summary>Fired after an item has been removed from an <c>ArrayLayout</c>.</summary>
    public event ArrayItemRemovedHandler? OnArrayItemRemoved;

    /// <summary>Fired after an item has been moved within an <c>ArrayLayout</c>.</summary>
    public event ArrayItemMovedHandler? OnArrayItemMoved;

    /// <summary>Fired after an item's data has been replaced within an <c>ArrayLayout</c>.</summary>
    public event ArrayItemUpdatedHandler? OnArrayItemUpdated;

    public Task InvokeArrayItemAdded(FormArrayContext array, int addedIndex, IJsonFormContext form)
        => OnArrayItemAdded?.Invoke(array, addedIndex, form) ?? Task.CompletedTask;

    public Task InvokeArrayItemRemoved(FormArrayContext array, int removedIndex, IJsonFormContext form)
        => OnArrayItemRemoved?.Invoke(array, removedIndex, form) ?? Task.CompletedTask;

    public Task InvokeArrayItemMoved(FormArrayContext array, int fromIndex, int toIndex, IJsonFormContext form)
        => OnArrayItemMoved?.Invoke(array, fromIndex, toIndex, form) ?? Task.CompletedTask;

    public Task InvokeArrayItemUpdated(FormArrayContext array, int updatedIndex, IJsonFormContext form)
        => OnArrayItemUpdated?.Invoke(array, updatedIndex, form) ?? Task.CompletedTask;

    private readonly Dictionary<string, FormActionHandler> _actions = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers an action handler invoked when an <c>ActionButton</c> with the given key is clicked.
    /// Calling this with the same key a second time replaces the previous handler.
    /// </summary>
    public void RegisterAction(string key, FormActionHandler handler)
        => _actions[key] = handler;

    public Task InvokeAction(string key, IJsonFormContext form)
        => _actions.TryGetValue(key, out var handler) ? handler(form) : Task.CompletedTask;
}
