using Orbyss.Blazor.JsonForms.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Context.Models;

namespace Orbyss.Blazor.JsonForms.ComponentFactory;

/// <summary>
/// Composite <see cref="IFormComponentFactory"/> that delegates every <c>Create*</c> call to
/// an injected sub-factory.
///
/// <para>
/// Each sub-factory interface handles one category of UI element (controls, buttons, navigation,
/// lists, action buttons, array layouts). Replace any single sub-factory in your DI container to
/// customise that category without touching the others. The defaults are the concrete sub-factory
/// classes bundled with this library.
/// </para>
///
/// <para>
/// <b>DI registration example</b>:
/// <code>
/// services.AddSingleton&lt;IControlComponentFactory&gt;(sp =&gt; new ControlComponentFactory
/// {
///     TextInputComponentType = typeof(MyTextBox),
///     NumberInputComponentType = typeof(MyNumberInput),
/// });
/// services.AddSingleton&lt;IButtonComponentFactory&gt;(sp =&gt; new ButtonComponentFactory
/// {
///     SubmitButtonComponentType = typeof(MyButton),
/// });
/// // … register the remaining sub-factories …
/// services.AddSingleton&lt;IFormComponentFactory, FormComponentFactory&gt;();
/// </code>
/// </para>
/// </summary>
public class FormComponentFactory(
    IControlComponentFactory    controlFactory,
    IButtonComponentFactory     buttonFactory,
    INavigationComponentFactory navigationFactory,
    IListComponentFactory       listFactory,
    IActionButtonComponentFactory actionButtonFactory,
    IArrayLayoutComponentFactory  arrayLayoutFactory)
    : IFormComponentFactory
{
    /// <inheritdoc />
    public IComponentInstance CreateControl(IJsonFormContext formContext, FormControlContext control)
        => controlFactory.CreateControl(formContext, control);

    /// <inheritdoc />
    public IComponentInstance CreateButton(FormButtonType buttonType, IJsonFormContext? formContext)
        => buttonFactory.CreateButton(buttonType, formContext);

    /// <inheritdoc />
    public IComponentInstance CreateNavigation(IJsonFormContext formContext)
        => navigationFactory.CreateNavigation(formContext);

    /// <inheritdoc />
    public IComponentInstance CreateList(IJsonFormContext formContext, FormListContext list)
        => listFactory.CreateList(formContext, list);

    /// <inheritdoc />
    public IComponentInstance CreateListItem(IFormElementContext? listItem = null)
        => listFactory.CreateListItem(listItem);

    /// <inheritdoc />
    public IComponentInstance CreateActionButton(IJsonFormContext formContext, FormActionButtonContext actionButton)
        => actionButtonFactory.CreateActionButton(formContext, actionButton);

    /// <inheritdoc />
    public IComponentInstance CreateArrayLayout(IJsonFormContext formContext, FormArrayContext arrayContext)
        => arrayLayoutFactory.CreateArrayLayout(formContext, arrayContext);
}
