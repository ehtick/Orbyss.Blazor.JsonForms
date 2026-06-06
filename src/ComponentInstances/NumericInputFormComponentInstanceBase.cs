namespace Orbyss.Blazor.JsonForms.ComponentInstances;

/// <summary>
/// Base class for numeric input component instances (e.g. sliders).
/// Adds PrefixText and SuffixText which can be set programmatically or via UI schema options.
/// </summary>
public abstract class NumericInputFormComponentInstanceBase : InputFormComponentInstanceBase
{
    public string? PrefixText { get; set; }

    public string? SuffixText { get; set; }

    protected override IDictionary<string, object?> GetFormInputParameters()
    {
        var result = GetNumericFormInputParameters();
        AddIfNotContains(result, nameof(PrefixText), PrefixText);
        AddIfNotContains(result, nameof(SuffixText), SuffixText);
        return result;
    }

    protected virtual IDictionary<string, object?> GetNumericFormInputParameters()
    {
        return new Dictionary<string, object?>();
    }
}
