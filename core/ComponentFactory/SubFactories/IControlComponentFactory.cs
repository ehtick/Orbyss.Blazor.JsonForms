using Orbyss.Blazor.JsonForms.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Context.Models;

namespace Orbyss.Blazor.JsonForms.ComponentFactory;

/// <summary>
/// Creates component instances for data-bound control fields (text, number, boolean, enum, date…).
///
/// <para>
/// Implement this interface — instead of the full <see cref="IFormComponentFactory"/> — when you
/// only need to customise how input controls are resolved, without touching buttons, navigation,
/// lists, or array layouts.
/// </para>
/// </summary>
public interface IControlComponentFactory
{
    /// <summary>
    /// Creates a component instance for a data-bound control.
    /// The engine auto-wires Value, Label, Disabled, ReadOnly, CssClass, HelperText and slot-specific
    /// extras (Items for enums, PrefixText/SuffixText for numerics).
    /// The Razor component appends typed ValueChanged / OnValueChanged EventCallbacks after this call.
    /// </summary>
    IComponentInstance CreateControl(IJsonFormContext formContext, FormControlContext control);
}
