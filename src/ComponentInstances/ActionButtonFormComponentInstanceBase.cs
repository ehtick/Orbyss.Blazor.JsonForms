using Microsoft.AspNetCore.Components;

namespace Orbyss.Blazor.JsonForms.ComponentInstances;

/// <summary>
/// Base class for action button component instances.
/// The engine sets <see cref="Label"/> and <see cref="Disabled"/> automatically.
/// <see cref="OnClick"/> is wired by <c>FormActionButton.razor</c> to invoke the registered action.
/// </summary>
public abstract class ActionButtonFormComponentInstanceBase : FormComponentInstanceBase
{
    public string? Label { get; internal set; }

    public bool Disabled { get; internal set; }

    /// <summary>
    /// Wired by the form engine to invoke the registered action handler when clicked.
    /// </summary>
    public EventCallback OnClick { get; internal set; }

    protected virtual IDictionary<string, object?> GetActionButtonParameters()
        => new Dictionary<string, object?>();

    protected override sealed IDictionary<string, object?> GetParametersCore()
    {
        var result = GetActionButtonParameters();

        result[nameof(Label)] = Label;
        result[nameof(Disabled)] = Disabled;
        result[nameof(OnClick)] = OnClick;

        return result;
    }
}
