using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using Orbyss.Blazor.JsonForms.Core.ComponentFactory;
using Orbyss.Blazor.JsonForms.Core.Models;
using System.Globalization;
using Orbyss.Blazor.JsonForms.Core.Constants;

namespace Orbyss.Blazor.JsonForms.Core.ComponentBases;

/// <summary>
/// Abstract base class for all data-bound form input components.
///
/// <para>
/// Inherit from this class to get standard Blazor parameters pre-declared and wired up:
/// <c>Value</c>, <c>ValueChanged</c>, <c>OnValueChanged</c>, <c>OnInputChanged</c>,
/// <c>Disabled</c>, <c>ReadOnly</c>, <c>Label</c>, <c>Class</c>, <c>Style</c>,
/// <c>Culture</c>, <c>HelperText</c>, <c>HelperIconText</c>, <c>ErrorHelperText</c>.
/// </para>
///
/// <para>
/// The default <see cref="ConvertFromJToken"/> implementation calls
/// <c>JToken.ToObject&lt;TValue&gt;</c>. Override this for types that need custom
/// deserialization (e.g. <c>DateTimeUtcTicks</c>, <c>DateOnly</c>).
/// </para>
///
/// <para>
/// For enum dropdowns or multi-select lists also declare <c>Items</c> and <c>MultiSelect</c>
/// parameters directly on your component — the engine sets them automatically when the
/// control type is <c>Enum</c> or <c>EnumList</c>.
/// </para>
/// </summary>
public abstract class FormInputComponentBase<TValue> : ComponentBase, IFormComponent
{
    /// <summary>The current bound value from the form data store.</summary>
    [Parameter] public TValue? Value { get; set; }

    /// <summary>
    /// Two-way binding callback. Implement this on your component to support
    /// <c>@bind-Value</c> with the engine's cascade.
    /// </summary>
    [Parameter] public EventCallback<TValue?> ValueChanged { get; set; }

    /// <summary>
    /// Fired when the user commits a value change (blur, selection, toggle).
    /// The engine hooks this to <c>IJsonFormContext.UpdateValue</c> and validation.
    /// </summary>
    [Parameter] public EventCallback<TValue?> OnValueChanged { get; set; }

    /// <summary>
    /// Fired on every raw input event (e.g. each keystroke).
    /// The engine hooks this to <c>IJsonFormContext.NotifyControlInputChanged</c>.
    /// Components that do not declare this parameter have it stripped automatically.
    /// </summary>
    [Parameter] public EventCallback<TValue?> OnInputChanged { get; set; }

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public CultureInfo Culture { get; set; } = FormCulture.Instance;
    [Parameter] public string? HelperText { get; set; }
    [Parameter] public string? HelperIconText { get; set; }

    /// <summary>
    /// Validation error text set by the engine after form validation runs.
    /// Cleared automatically when the user commits a new value.
    /// </summary>
    [Parameter] public string? ErrorHelperText { get; set; }

    /// <summary>
    /// Enum item list populated by the engine for <c>Enum</c> / <c>EnumList</c> controls.
    /// Declare this parameter on your component if you render a dropdown or multi-select.
    /// </summary>
    [Parameter] public IEnumerable<TranslatedEnumItem> Items { get; set; } = [];

    /// <summary>
    /// <c>true</c> when the control is configured as <c>EnumList</c> (multi-select).
    /// Declare this parameter on your component to switch between single- and multi-select mode.
    /// </summary>
    [Parameter] public bool MultiSelect { get; set; }

    /// <inheritdoc />
    /// <remarks>
    /// The default implementation uses <c>JToken.ToObject&lt;TValue&gt;</c>.
    /// Override for types that require custom deserialization.
    /// </remarks>
    public virtual object? ConvertFromJToken(JToken? token)
        => token is null || token.Type == JTokenType.Null
            ? default(TValue)
            : token.ToObject<TValue?>();
}
