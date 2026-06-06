using Microsoft.AspNetCore.Components;
using Orbyss.Blazor.JsonForms.Constants;
using System.Globalization;

namespace Orbyss.Blazor.JsonForms.ComponentBases;

/// <summary>
/// Abstract base class for list container components (a repeating group with an "add" button).
///
/// <para>
/// Inherit to get standard parameters pre-declared: <c>Title</c>, <c>Disabled</c>,
/// <c>ReadOnly</c>, <c>Error</c>, <c>OnAddItemClicked</c>, <c>ChildContent</c>,
/// <c>Class</c>, <c>Style</c>, <c>Culture</c>.
/// </para>
/// </summary>
public abstract class FormListComponentBase : ComponentBase
{
    /// <summary>Resolved list title from the translation schema.</summary>
    [Parameter] public string? Title { get; set; }

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>Validation error message set by the engine after form validation.</summary>
    [Parameter] public string? Error { get; set; }

    /// <summary>Fired when the user clicks the "add item" button.</summary>
    [Parameter] public EventCallback OnAddItemClicked { get; set; }

    /// <summary>The rendered list items provided by the engine.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public CultureInfo Culture { get; set; } = FormCulture.Instance;
}
