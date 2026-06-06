using Microsoft.AspNetCore.Components;
using Orbyss.Blazor.JsonForms.Constants;
using System.Globalization;

namespace Orbyss.Blazor.JsonForms.ComponentBases;

/// <summary>
/// Abstract base class for list item wrapper components.
///
/// <para>
/// Inherit to get standard parameters pre-declared:
/// <c>Disabled</c>, <c>ReadOnly</c>, <c>OnRemoveItemClicked</c>, <c>ChildContent</c>,
/// <c>Class</c>, <c>Style</c>, <c>Culture</c>.
/// </para>
/// </summary>
public abstract class FormListItemComponentBase : ComponentBase
{
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>Fired when the user clicks the remove button for this item.</summary>
    [Parameter] public EventCallback OnRemoveItemClicked { get; set; }

    /// <summary>The rendered control fields for this item, provided by the engine.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public CultureInfo Culture { get; set; } = FormCulture.Instance;
}
