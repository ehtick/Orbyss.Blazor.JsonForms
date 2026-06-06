using Microsoft.AspNetCore.Components;
using Orbyss.Blazor.JsonForms.Constants;
using System.Globalization;

namespace Orbyss.Blazor.JsonForms.ComponentBases;

/// <summary>
/// Abstract base class for action button components (buttons that fire a named action
/// registered on <c>JsonFormContextOptions.RegisterAction</c>).
///
/// <para>
/// Inherit to get standard parameters pre-declared: <c>Label</c>, <c>Disabled</c>,
/// <c>OnClick</c>, <c>Class</c>, <c>Style</c>, <c>Culture</c>.
/// </para>
/// </summary>
public abstract class FormActionButtonComponentBase : ComponentBase
{
    /// <summary>Resolved button label from the translation schema.</summary>
    [Parameter] public string? Label { get; set; }

    [Parameter] public bool Disabled { get; set; }

    /// <summary>Fired when the button is clicked. The engine dispatches the registered action handler.</summary>
    [Parameter] public EventCallback OnClick { get; set; }

    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public CultureInfo Culture { get; set; } = FormCulture.Instance;
}
