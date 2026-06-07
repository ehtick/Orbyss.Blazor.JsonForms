using Orbyss.Blazor.JsonForms.Core.Context.Interfaces;

namespace Orbyss.Blazor.JsonForms.Core.ComponentFactory.SubFactories;

/// <summary>
/// Creates the component instance for the navigation wrapper (multi-page forms).
///
/// <para>
/// Implement this interface — instead of the full <see cref="IFormComponentFactory"/> — when you
/// only need to customise the navigation component.
/// </para>
/// </summary>
public interface INavigationComponentFactory
{
    /// <summary>
    /// Creates a component instance for the navigation component.
    /// The Razor component appends OnSubmitClicked and HideSubmitButton after this call.
    /// </summary>
    IComponentInstance CreateNavigation(IJsonFormContext formContext);
}
