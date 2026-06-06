using Orbyss.Blazor.JsonForms.Constants;
using Orbyss.Blazor.JsonForms.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Context.Models;
using Orbyss.Blazor.JsonForms.Utils;
using System.Linq.Expressions;

namespace Orbyss.Blazor.JsonForms.ComponentFactory;

/// <summary>
/// Default implementation of <see cref="IActionButtonComponentFactory"/>.
/// Manages the ActionButton component type slot for schema-driven action buttons.
/// Supports UI schema alias overrides via <see cref="RegisterAlias"/>.
/// </summary>
public class ActionButtonComponentFactory : ComponentFactoryBase, IActionButtonComponentFactory
{
    // ── Action button type slot ───────────────────────────────────────────────

    public Type? ActionButtonComponentType { get; set; }

    // ── Public SetParameter ───────────────────────────────────────────────────

    /// <summary>
    /// Registers a static parameter value for <typeparamref name="TComponent"/>.
    /// Throws for engine-restricted parameter names. Returns <c>this</c> for chaining.
    /// </summary>
    public new ActionButtonComponentFactory SetParameter<TComponent, TValue>(
        Expression<Func<TComponent, TValue>> selector,
        TValue value)
    {
        base.SetParameter(selector, value);
        return this;
    }

    // ── IActionButtonComponentFactory ─────────────────────────────────────────

    /// <inheritdoc />
    public IComponentInstance CreateActionButton(IJsonFormContext formContext, FormActionButtonContext actionButton)
    {
        var alias = actionButton.Interpretation.GetOption(FormUiSchemaOptionKeys.Component)?.ToString();
        var componentType = (alias is not null ? ResolveAlias(alias) : null)
            ?? ActionButtonComponentType
            ?? throw new InvalidOperationException(
                $"No component registered for ActionButton slot. " +
                $"Set {nameof(ActionButtonComponentType)} on {nameof(ActionButtonComponentFactory)}.");

        var instance = new FormComponentInstance(componentType);

        // Layer 1 — engine auto-wire
        instance.Parameters[FormComponentParameterKeys.Label]    = formContext.GetLabel(actionButton.Id);
        instance.Parameters[FormComponentParameterKeys.Disabled] = actionButton.Disabled;
        instance.Parameters[FormComponentParameterKeys.Culture]  = FormCulture.Instance;

        // Layer 2 — factory SetParameter assignments
        ApplyAssignedParameters(instance, componentType);
        MergeCssClass(instance, formContext.GetCssClass(actionButton.Id));

        // Layer 3 — UI schema "parameters" option
        ApplyUiSchemaParameters(instance, actionButton.Interpretation.GetOption(FormUiSchemaOptionKeys.Parameters));

        RemoveUndeclaredParameters.Remove(componentType, instance.Parameters);
        return instance;
    }
}
