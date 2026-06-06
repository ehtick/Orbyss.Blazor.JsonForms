namespace Orbyss.Blazor.JsonForms.Constants;

/// <summary>
/// CSS class names applied by the Orbyss JSON Forms engine.
/// These classes can be targeted in your own stylesheet to customise form appearance,
/// and are used as the default <c>Class</c> values on component instances.
/// </summary>
public static class FormCssClasses
{
    /// <summary>Root class on the outermost form wrapper.</summary>
    public const string Form = "orbyss-form";

    /// <summary>Applied to horizontal row layout containers.</summary>
    public const string Row = "orbyss-form-row";

    /// <summary>Applied to column layout containers inside a row.</summary>
    public const string Column = "orbyss-form-column";

    /// <summary>Default class on text input controls.</summary>
    public const string TextInput = "orbyss-form-text-input";

    /// <summary>Default class on integer and number input controls.</summary>
    public const string NumberInput = "orbyss-form-number-input";

    /// <summary>Default class on dropdown / enum controls.</summary>
    public const string Dropdown = "orbyss-form-dropdown";

    /// <summary>Default class on boolean toggle controls.</summary>
    public const string BooleanInput = "orbyss-form-boolean-input";

    /// <summary>Default class on date/time picker controls.</summary>
    public const string DateInput = "orbyss-form-date-input";

    /// <summary>Default class on slider controls.</summary>
    public const string Slider = "orbyss-form-slider";

    /// <summary>Default class on enum block selectors.</summary>
    public const string EnumBlocks = "orbyss-form-enum-blocks";

    /// <summary>Default class on action button elements.</summary>
    public const string ActionButton = "orbyss-form-action-button";

    /// <summary>Row container that holds submit / navigation buttons, right-aligned.</summary>
    public const string ButtonRow = "orbyss-form-button-row";

    /// <summary>Column container that wraps an individual submit / navigation button.</summary>
    public const string ButtonColumn = "orbyss-form-button-column";

    /// <summary>
    /// Modifier applied to <see cref="ButtonRow"/> when buttons should be spaced to opposite ends
    /// (e.g. a stepper with both Previous and Next visible at the same time).
    /// </summary>
    public const string ButtonRowSpaceBetween = "orbyss-form-button-row--space-between";
}
