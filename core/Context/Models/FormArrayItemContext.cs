using Orbyss.Blazor.JsonForms.Core.Context.Interfaces;

namespace Orbyss.Blazor.JsonForms.Core.Context.Models;

/// <summary>
/// Represents a single item in an inline array repeater.
/// Holds the item's index (used for data path resolution) and the pre-built element context
/// for the item's fields.
/// </summary>
public sealed class FormArrayItemContext
{
    private readonly Guid id = Guid.NewGuid();

    public FormArrayItemContext(int index, IFormElementContext elementContext)
    {
        Index = index;
        ElementContext = elementContext;
    }

    /// <summary>Stable identifier used by the Blazor render tree and drag-to-reorder logic.</summary>
    public Guid Id => id;

    /// <summary>Zero-based position of this item in the backing <c>JArray</c>.</summary>
    public int Index { get; }

    /// <summary>The fully-wired element context for this item's fields.</summary>
    public IFormElementContext ElementContext { get; }
}
