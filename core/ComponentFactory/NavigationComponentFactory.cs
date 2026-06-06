using Orbyss.Blazor.JsonForms.Constants;
using Orbyss.Blazor.JsonForms.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Utils;
using System.Linq.Expressions;

namespace Orbyss.Blazor.JsonForms.ComponentFactory;

/// <summary>
/// Default implementation of <see cref="INavigationComponentFactory"/>.
/// Manages the single Navigation component type slot used for multi-page form navigation.
/// </summary>
public class NavigationComponentFactory : ComponentFactoryBase, INavigationComponentFactory
{
    // ── Navigation type slot ──────────────────────────────────────────────────

    public Type? NavigationComponentType { get; set; }

    // ── Public SetParameter ───────────────────────────────────────────────────

    /// <summary>
    /// Registers a static parameter value for <typeparamref name="TComponent"/>.
    /// Throws for engine-restricted parameter names. Returns <c>this</c> for chaining.
    /// </summary>
    public new NavigationComponentFactory SetParameter<TComponent, TValue>(
        Expression<Func<TComponent, TValue>> selector,
        TValue value)
    {
        base.SetParameter(selector, value);
        return this;
    }

    // ── INavigationComponentFactory ───────────────────────────────────────────

    /// <inheritdoc />
    public IComponentInstance CreateNavigation(IJsonFormContext formContext)
    {
        var componentType = NavigationComponentType
            ?? throw new InvalidOperationException(
                $"No component registered for Navigation slot. " +
                $"Set {nameof(NavigationComponentType)} on {nameof(NavigationComponentFactory)}.");

        var instance = new FormComponentInstance(componentType);

        // Layer 1 — engine auto-wire
        instance.Parameters[FormComponentParameterKeys.Culture] = FormCulture.Instance;

        // Layer 2 — factory SetParameter assignments
        ApplyAssignedParameters(instance, componentType);

        RemoveUndeclaredParameters.Remove(componentType, instance.Parameters);
        return instance;
    }
}
