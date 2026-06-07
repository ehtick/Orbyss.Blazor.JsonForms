using Orbyss.Blazor.JsonForms.Core.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Core.Context.Models;

namespace Orbyss.Blazor.JsonForms.Core.ComponentFactory.SubFactories;

/// <summary>
/// Creates component instances for list containers and list items.
///
/// <para>
/// Implement this interface — instead of the full <see cref="IFormComponentFactory"/> — when you
/// only need to customise list rendering.
/// </para>
/// </summary>
public interface IListComponentFactory
{
    /// <summary>
    /// Creates a component instance for a list container.
    /// The engine auto-wires Title, Disabled and ReadOnly.
    /// The Razor component appends OnAddItemClicked and ChildContent after this call.
    /// </summary>
    IComponentInstance CreateList(IJsonFormContext formContext, FormListContext list);

    /// <summary>
    /// Creates a component instance for a single list item wrapper.
    /// The Razor component appends OnRemoveItemClicked and ChildContent after this call.
    /// </summary>
    IComponentInstance CreateListItem(IFormElementContext? listItem = null);
}
