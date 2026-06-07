using Orbyss.Blazor.JsonForms.Core.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Core.Context.Models;

namespace Orbyss.Blazor.JsonForms.Core.ComponentFactory.SubFactories;

/// <summary>
/// Creates component instances for action buttons (buttons that fire a named action
/// registered via <c>JsonFormOptions.RegisterAction</c>).
///
/// <para>
/// Implement this interface — instead of the full <see cref="IFormComponentFactory"/> — when you
/// only need to customise action button rendering.
/// </para>
/// </summary>
public interface IActionButtonComponentFactory
{
    /// <summary>
    /// Creates a component instance for an action button.
    /// The engine auto-wires Label, Disabled and CssClass.
    /// The Razor component appends the OnClick EventCallback after this call.
    /// </summary>
    IComponentInstance CreateActionButton(IJsonFormContext formContext, FormActionButtonContext actionButton);
}
