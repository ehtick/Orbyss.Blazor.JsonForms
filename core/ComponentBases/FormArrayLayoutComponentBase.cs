using Microsoft.AspNetCore.Components;
using Orbyss.Blazor.JsonForms.Core.Constants;
using Orbyss.Blazor.JsonForms.Core.Context.Models;
using System.Globalization;

namespace Orbyss.Blazor.JsonForms.Core.ComponentBases;

/// <summary>
/// Abstract base class for inline array repeater components (add / remove / reorder items
/// without a separate list detail page).
///
/// <para>
/// Inherit to get standard parameters pre-declared: <c>ArrayContext</c>, <c>AddLabel</c>,
/// <c>Class</c>, <c>Style</c>, <c>Culture</c>.
/// </para>
/// </summary>
public abstract class FormArrayLayoutComponentBase : ComponentBase
{
    /// <summary>
    /// The live array context. Exposes the item list and interpretation. Updated by the engine
    /// on every render cycle — always read the latest value rather than caching it.
    /// </summary>
    [Parameter] public FormArrayContext? ArrayContext { get; set; }

    /// <summary>
    /// Resolved "add item" label. <c>null</c> means the schema did not provide a label;
    /// your component should fall back to <c>+</c> or a default string.
    /// </summary>
    [Parameter] public string? AddLabel { get; set; }

    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public CultureInfo Culture { get; set; } = FormCulture.Instance;
}
