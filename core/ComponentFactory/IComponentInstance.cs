namespace Orbyss.Blazor.JsonForms.Core.ComponentFactory;

/// <summary>
/// Represents the resolved output of a <see cref="IFormComponentFactory"/> <c>Create*</c> call:
/// a component type and its initial parameter set.
///
/// <para>
/// The Razor component receives the instance, appends any EventCallbacks that capture
/// <c>this</c> (because closures can only be created inside the Razor component), then
/// passes <see cref="ComponentType"/> and <see cref="Parameters"/> to a
/// <c>DynamicComponent</c>.
/// </para>
///
/// <para>
/// <see cref="Parameters"/> is mutable so that both the factory and the Razor component
/// can write into the same dictionary without extra allocations.
/// </para>
/// </summary>
public interface IComponentInstance
{
    /// <summary>The concrete Blazor component type to render.</summary>
    Type ComponentType { get; }

    /// <summary>
    /// The fully-merged parameter bag (engine auto-wire → factory static assignments →
    /// UI schema overrides). Undeclared parameters are stripped before the dictionary is
    /// returned.
    ///
    /// <para>
    /// The Razor component appends EventCallbacks here before passing the dictionary to
    /// <c>DynamicComponent.Parameters</c>.
    /// </para>
    /// </summary>
    IDictionary<string, object?> Parameters { get; }
}
