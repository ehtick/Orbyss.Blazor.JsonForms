using Microsoft.AspNetCore.Components;

namespace Orbyss.Blazor.JsonForms.ComponentInstances;

public abstract class NavigationFormComponentInstanceBase : FormComponentInstanceBase
{
    public EventCallback? OnSubmitClicked { get; internal set; }

    public bool HideSubmitButton { get; internal set; }

    protected override IDictionary<string, object?> GetParametersCore()
    {
        var result = GetNavigationParameters();
        result[nameof(OnSubmitClicked)] = OnSubmitClicked;
        result[nameof(HideSubmitButton)] = HideSubmitButton;
        return result;
    }

    protected virtual IDictionary<string, object?> GetNavigationParameters()
    {
        return new Dictionary<string, object?>();
    }
}