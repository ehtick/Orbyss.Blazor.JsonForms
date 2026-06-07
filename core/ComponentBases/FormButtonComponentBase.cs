using Microsoft.AspNetCore.Components;
using Orbyss.Blazor.JsonForms.Core.Constants;
using System.Globalization;

namespace Orbyss.Blazor.JsonForms.Core.ComponentBases;

/// <summary>
/// Abstract base class for Submit, Next and Previous button components.
///
/// <para>
/// Inherit to get standard parameters pre-declared: <c>Text</c>, <c>Disabled</c>,
/// <c>OnClicked</c>, <c>Class</c>, <c>Style</c>, <c>Culture</c>.
/// </para>
/// </summary>
public abstract class FormButtonComponentBase : ComponentBase
{
    /// <summary>The button label resolved from the translation schema.</summary>
    [Parameter] public string? Text { get; set; }

    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Fired when the button is clicked. The engine wires this to the form's submit / navigation logic.
    /// </summary>
    [Parameter] public EventCallback OnClicked { get; set; }

    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public CultureInfo Culture { get; set; } = FormCulture.Instance;
}
