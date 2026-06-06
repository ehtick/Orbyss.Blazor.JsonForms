using Orbyss.Blazor.JsonForms.Context.Models;

namespace Orbyss.Blazor.JsonForms.ComponentInstances;

/// <summary>
/// Base class for array-layout (inline repeater) component instances.
/// The engine sets <see cref="ArrayContext"/> and <see cref="AddLabel"/> automatically.
/// The component receives these as Blazor parameters and is responsible for rendering items,
/// the add button, remove buttons, and drag-to-reorder handles.
/// </summary>
public abstract class ArrayLayoutFormComponentInstanceBase : FormComponentInstanceBase
{
    /// <summary>The live array context — exposes the item list and interpretation.</summary>
    public FormArrayContext? ArrayContext { get; internal set; }

    /// <summary>
    /// Resolved "add item" label (already translated).
    /// <c>null</c> means no translation was found; the component should fall back to <c>+</c>.
    /// </summary>
    public string? AddLabel { get; internal set; }

    protected virtual IDictionary<string, object?> GetArrayLayoutParameters()
        => new Dictionary<string, object?>();

    protected override sealed IDictionary<string, object?> GetParametersCore()
    {
        var result = GetArrayLayoutParameters();
        result[nameof(ArrayContext)] = ArrayContext;
        result[nameof(AddLabel)]     = AddLabel;
        return result;
    }
}
