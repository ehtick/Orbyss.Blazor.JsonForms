using Orbyss.Blazor.JsonForms.Core.ComponentFactory.SubFactories;
using Orbyss.Blazor.JsonForms.Core.Constants;
using Orbyss.Blazor.JsonForms.Core.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Core.Context.Models;
using Orbyss.Blazor.JsonForms.Core.Utils;
using System.Linq.Expressions;

namespace Orbyss.Blazor.JsonForms.Core.ComponentFactory;

/// <summary>
/// Default implementation of <see cref="IListComponentFactory"/>.
/// Manages the List container and ListItem component type slots.
/// </summary>
public class ListComponentFactory : ComponentFactoryBase, IListComponentFactory
{
    // ── List type slots ───────────────────────────────────────────────────────

    public Type? ListComponentType     { get; set; }
    public Type? ListItemComponentType { get; set; }

    // ── Public SetParameter ───────────────────────────────────────────────────

    /// <summary>
    /// Registers a static parameter value for <typeparamref name="TComponent"/>.
    /// Throws for engine-restricted parameter names. Returns <c>this</c> for chaining.
    /// </summary>
    public new ListComponentFactory SetParameter<TComponent, TValue>(
        Expression<Func<TComponent, TValue>> selector,
        TValue value)
    {
        base.SetParameter(selector, value);
        return this;
    }

    // ── IListComponentFactory ─────────────────────────────────────────────────

    /// <inheritdoc />
    public virtual IComponentInstance CreateList(IJsonFormContext formContext, FormListContext list)
    {
        var componentType = ListComponentType
            ?? throw new InvalidOperationException(
                $"No component registered for List slot. " +
                $"Set {nameof(ListComponentType)} on {nameof(ListComponentFactory)}.");

        var instance = new FormComponentInstance(componentType);

        // Layer 1 — engine auto-wire
        instance.Parameters[FormComponentParameterKeys.Title]    = formContext.GetLabel(list.Id);
        instance.Parameters[FormComponentParameterKeys.Disabled] = formContext.Disabled || list.Disabled;
        instance.Parameters[FormComponentParameterKeys.ReadOnly] = formContext.ReadOnly || list.ReadOnly;
        instance.Parameters[FormComponentParameterKeys.Culture]  = FormCulture.Instance;

        // Layer 2 — factory SetParameter assignments
        ApplyAssignedParameters(instance, componentType);
        MergeCssClass(instance, formContext.GetCssClass(list.Id));

        RemoveUndeclaredParameters.Remove(componentType, instance.Parameters);
        return instance;
    }

    /// <inheritdoc />
    public virtual IComponentInstance CreateListItem(IFormElementContext? listItem = null)
    {
        var componentType = ListItemComponentType
            ?? throw new InvalidOperationException(
                $"No component registered for ListItem slot. " +
                $"Set {nameof(ListItemComponentType)} on {nameof(ListComponentFactory)}.");

        var instance = new FormComponentInstance(componentType);

        // Layer 1 — engine auto-wire
        if (listItem is not null)
        {
            instance.Parameters[FormComponentParameterKeys.Disabled] = listItem.Disabled;
            instance.Parameters[FormComponentParameterKeys.ReadOnly] = listItem.ReadOnly;
        }
        instance.Parameters[FormComponentParameterKeys.Culture] = FormCulture.Instance;

        // Layer 2 — factory SetParameter assignments
        ApplyAssignedParameters(instance, componentType);

        RemoveUndeclaredParameters.Remove(componentType, instance.Parameters);
        return instance;
    }
}
