using Orbyss.Blazor.JsonForms.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Context.Models;

namespace Orbyss.Blazor.JsonForms.ComponentFactory;

/// <summary>
/// Creates component instances for inline array repeater layouts.
///
/// <para>
/// Implement this interface — instead of the full <see cref="IFormComponentFactory"/> — when you
/// only need to customise array layout rendering.
/// </para>
/// </summary>
public interface IArrayLayoutComponentFactory
{
    /// <summary>
    /// Creates a component instance for an inline array repeater.
    /// The engine auto-wires ArrayContext and AddLabel.
    /// </summary>
    IComponentInstance CreateArrayLayout(IJsonFormContext formContext, FormArrayContext arrayContext);
}
