namespace Orbyss.Blazor.JsonForms.ComponentFactory;

/// <summary>
/// Default implementation of <see cref="IComponentInstance"/>: a resolved component type paired
/// with a case-insensitive parameter dictionary that the Razor component can freely extend
/// before passing to <c>DynamicComponent</c>.
/// </summary>
public sealed class FormComponentInstance : IComponentInstance
{
    public FormComponentInstance(Type componentType)
    {
        ComponentType = componentType;
        Parameters = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public Type ComponentType { get; }

    /// <inheritdoc />
    public IDictionary<string, object?> Parameters { get; }
}
