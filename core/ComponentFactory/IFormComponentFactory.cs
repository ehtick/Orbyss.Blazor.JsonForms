using Orbyss.Blazor.JsonForms.Core.ComponentFactory.SubFactories;
using Orbyss.Blazor.JsonForms.Core.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Core.Context.Models;

namespace Orbyss.Blazor.JsonForms.Core.ComponentFactory;

/// <summary>
/// Composite factory the engine cascades through the Razor component tree as a single
/// <c>[CascadingParameter]</c>. It owns no creation logic itself — each <c>Create*</c> call
/// delegates to the matching per-slot sub-factory (controls, buttons, navigation, lists,
/// action buttons, array layouts).
///
/// <para>
/// This interface deliberately does <b>not</b> inherit the sub-factory interfaces: the composite
/// is not a control factory or a button factory, it merely routes to them. The method signatures
/// are restated here so the delegation surface is explicit and self-documenting.
/// </para>
///
/// <para>
/// Consumers who need to customise only one category implement the relevant sub-factory interface
/// (e.g. <see cref="IControlComponentFactory"/>) and register it in DI — <see cref="FormComponentFactory"/>
/// picks it up via constructor injection without affecting the other slots. Consumers who need a
/// fully custom router implement this interface directly.
/// </para>
/// </summary>
public interface IFormComponentFactory
{
    /// <inheritdoc cref="IControlComponentFactory.CreateControl" />
    IComponentInstance CreateControl(IJsonFormContext formContext, FormControlContext control);

    /// <inheritdoc cref="IButtonComponentFactory.CreateButton" />
    IComponentInstance CreateButton(FormButtonType buttonType, IJsonFormContext? formContext);

    /// <inheritdoc cref="INavigationComponentFactory.CreateNavigation" />
    IComponentInstance CreateNavigation(IJsonFormContext formContext);

    /// <inheritdoc cref="IListComponentFactory.CreateList" />
    IComponentInstance CreateList(IJsonFormContext formContext, FormListContext list);

    /// <inheritdoc cref="IListComponentFactory.CreateListItem" />
    IComponentInstance CreateListItem(IFormElementContext? listItem = null);

    /// <inheritdoc cref="IActionButtonComponentFactory.CreateActionButton" />
    IComponentInstance CreateActionButton(IJsonFormContext formContext, FormActionButtonContext actionButton);

    /// <inheritdoc cref="IArrayLayoutComponentFactory.CreateArrayLayout" />
    IComponentInstance CreateArrayLayout(IJsonFormContext formContext, FormArrayContext arrayContext);
}
