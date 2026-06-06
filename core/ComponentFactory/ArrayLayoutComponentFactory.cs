using Orbyss.Blazor.JsonForms.Constants;
using Orbyss.Blazor.JsonForms.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Context.Models;
using Orbyss.Blazor.JsonForms.Utils;
using System.Linq.Expressions;

namespace Orbyss.Blazor.JsonForms.ComponentFactory;

/// <summary>
/// Default implementation of <see cref="IArrayLayoutComponentFactory"/>.
/// Manages the ArrayLayout component type slot for inline array repeater sections.
/// </summary>
public class ArrayLayoutComponentFactory : ComponentFactoryBase, IArrayLayoutComponentFactory
{
    // ── Array layout type slot ────────────────────────────────────────────────

    public Type? ArrayLayoutComponentType { get; set; }

    // ── Public SetParameter ───────────────────────────────────────────────────

    /// <summary>
    /// Registers a static parameter value for <typeparamref name="TComponent"/>.
    /// Throws for engine-restricted parameter names. Returns <c>this</c> for chaining.
    /// </summary>
    public new ArrayLayoutComponentFactory SetParameter<TComponent, TValue>(
        Expression<Func<TComponent, TValue>> selector,
        TValue value)
    {
        base.SetParameter(selector, value);
        return this;
    }

    // ── IArrayLayoutComponentFactory ──────────────────────────────────────────

    /// <inheritdoc />
    public IComponentInstance CreateArrayLayout(IJsonFormContext formContext, FormArrayContext arrayContext)
    {
        var componentType = ArrayLayoutComponentType
            ?? throw new InvalidOperationException(
                $"No component registered for ArrayLayout slot. " +
                $"Set {nameof(ArrayLayoutComponentType)} on {nameof(ArrayLayoutComponentFactory)}.");

        var instance = new FormComponentInstance(componentType);

        // Layer 1 — engine auto-wire
        instance.Parameters[FormComponentParameterKeys.ArrayContext] = arrayContext;
        instance.Parameters[FormComponentParameterKeys.AddLabel]     = formContext.GetArrayAddLabel(arrayContext.Id);
        instance.Parameters[FormComponentParameterKeys.Culture]      = FormCulture.Instance;

        // Layer 2 — factory SetParameter assignments
        ApplyAssignedParameters(instance, componentType);

        RemoveUndeclaredParameters.Remove(componentType, instance.Parameters);
        return instance;
    }
}
