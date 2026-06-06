using Orbyss.Blazor.JsonForms.Context.Interfaces;

namespace Orbyss.Blazor.JsonForms.ComponentFactory;

/// <summary>
/// Creates component instances for Submit, Next and Previous buttons.
///
/// <para>
/// Implement this interface — instead of the full <see cref="IFormComponentFactory"/> — when you
/// only need to customise button rendering.
/// </para>
/// </summary>
public interface IButtonComponentFactory
{
    /// <summary>
    /// Creates a component instance for a Submit, Next or Previous button.
    /// The engine auto-wires Disabled.
    /// The Razor component appends the OnClicked EventCallback after this call.
    /// </summary>
    IComponentInstance CreateButton(FormButtonType buttonType, IJsonFormContext? formContext);
}
