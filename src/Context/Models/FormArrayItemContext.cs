using Orbyss.Blazor.JsonForms.Context.Interfaces;

namespace Orbyss.Blazor.JsonForms.Context.Models;

/// <summary>
/// Represents a single item in an inline array repeater.
/// Holds the item's index (used for data path resolution) and the pre-built element context
/// for the item's fields (e.g. a <c>FormHorizontalLayoutContext</c> with control contexts whose
/// <c>AbsoluteDataJsonPath</c> values already include the array index — <c>$.addresses[0].street</c>).
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
