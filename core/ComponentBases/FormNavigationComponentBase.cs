using Microsoft.AspNetCore.Components;
using Orbyss.Blazor.JsonForms.Core.Constants;
using System.Globalization;

namespace Orbyss.Blazor.JsonForms.Core.ComponentBases;

/// <summary>
/// Abstract base class for the navigation component that wraps multi-page forms
/// (previous / next / submit controls and page progress).
///
/// <para>
/// Inherit to get standard parameters pre-declared:
/// <c>OnSubmitClicked</c>, <c>HideSubmitButton</c>, <c>Class</c>, <c>Style</c>, <c>Culture</c>.
/// </para>
/// </summary>
public abstract class FormNavigationComponentBase : ComponentBase
{
    /// <summary>
    /// Callback wired by the engine to the form's submit handler.
    /// Null when the form has no submit action.
    /// </summary>
    [Parameter] public EventCallback? OnSubmitClicked { get; set; }

    /// <summary>When <c>true</c> the submit button should be hidden.</summary>
    [Parameter] public bool HideSubmitButton { get; set; }

    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public CultureInfo Culture { get; set; } = FormCulture.Instance;
}
