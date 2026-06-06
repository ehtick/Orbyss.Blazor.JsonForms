using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Orbyss.Blazor.JsonForms.Context.Interfaces;
using Orbyss.Blazor.JsonForms.UiSchema;
using Orbyss.Blazor.JsonForms.Utils;
using Orbyss.Components.Json.Models;

namespace Orbyss.Blazor.JsonForms.Context.Models;

/// <summary>
/// Handler delegate invoked when a control's committed value changes or when raw input changes.
/// </summary>
/// <param name="control">The control whose value changed.</param>
/// <param name="form">The form context, used to read/write other control values.</param>
public delegate Task FormControlEventHandler(FormControlContext control, IJsonFormContext form);

public sealed class JsonFormContextInitOptions
{
    public JsonFormContextInitOptions(JSchema dataSchema, FormUiSchema uiSchema, TranslationSchema translationSchema)
    {
        DataSchema = dataSchema;
        UiSchema = uiSchema;
        TranslationSchema = translationSchema;
    }

    public JsonFormContextInitOptions(string dataSchemaJson, string uiSchemaJson, string translationSchemaJson)
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
    /// Fired when a control's committed value changes (e.g. on blur, selection, or toggle).
    /// Subscribe with += to add multiple handlers.
    /// The caller is responsible for -= if the handler captures a short-lived object
    /// and this <see cref="JsonFormContextInitOptions"/> instance outlives it.
    /// </summary>
    public event FormControlEventHandler? OnControlValueChanged;

    /// <summary>
    /// Fired on every raw input event (e.g. each keystroke in a text field).
    /// Subscribe with += to add multiple handlers.
    /// Debouncing, if needed, is the caller's responsibility.
    /// The caller is responsible for -= if the handler captures a short-lived object
    /// and this <see cref="JsonFormContextInitOptions"/> instance outlives it.
    /// </summary>
    public event FormControlEventHandler? OnControlInputChanged;

    internal Task InvokeControlValueChanged(FormControlContext control, IJsonFormContext form)
        => OnControlValueChanged?.Invoke(control, form) ?? Task.CompletedTask;

    internal Task InvokeControlInputChanged(FormControlContext control, IJsonFormContext form)
        => OnControlInputChanged?.Invoke(control, form) ?? Task.CompletedTask;
}