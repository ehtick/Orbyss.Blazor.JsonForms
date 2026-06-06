namespace Orbyss.Blazor.JsonForms.Constants;

public static class ComponentInstanceParameterKeys
{
    public const string OnValueChanged = "OnValueChanged";

    /// <summary>
    /// Fired on every raw input event (e.g. each keystroke). Components that do not
    /// declare this parameter will have it stripped automatically by RemoveUndeclaredParameters.
    /// </summary>
    public const string OnInputChanged = "OnInputChanged";
}