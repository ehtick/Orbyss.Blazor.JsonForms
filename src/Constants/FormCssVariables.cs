namespace Orbyss.Blazor.JsonForms.Constants;

/// <summary>
/// CSS custom property names used by Orbyss JSON Forms components.
/// The hosting application can set these variables anywhere in its own stylesheet
/// to theme the form without touching component code.
/// </summary>
/// <example>
/// Override in your app's CSS:
/// <code>
/// :root {
///     --orbyss-form-primary:        #1976d2;
///     --orbyss-form-primary-text:   #ffffff;
///     --orbyss-form-error:          #d32f2f;
///     --orbyss-form-border-radius:  4px;
///     --orbyss-form-spacing:        1rem;
///     --orbyss-form-font-family:    inherit;
///     --orbyss-form-label-font-size: 0.875rem;
///     --orbyss-form-input-height:   2.5rem;
/// }
/// </code>
/// </example>
public static class FormCssVariables
{
    /// <summary>Primary brand colour, used for active/focused elements.</summary>
    public const string Primary = "--orbyss-form-primary";

    /// <summary>Text colour shown on top of the primary background.</summary>
    public const string PrimaryText = "--orbyss-form-primary-text";

    /// <summary>Error / validation failure colour.</summary>
    public const string Error = "--orbyss-form-error";

    /// <summary>Border-radius applied to inputs and buttons.</summary>
    public const string BorderRadius = "--orbyss-form-border-radius";

    /// <summary>Base spacing unit (gap, padding) between form elements.</summary>
    public const string Spacing = "--orbyss-form-spacing";

    /// <summary>Font family inherited by form elements.</summary>
    public const string FontFamily = "--orbyss-form-font-family";

    /// <summary>Font size for field labels and helper text.</summary>
    public const string LabelFontSize = "--orbyss-form-label-font-size";

    /// <summary>Height of single-line input controls.</summary>
    public const string InputHeight = "--orbyss-form-input-height";

    /// <summary>
    /// Gap between rows inside the outer form wrapper.
    /// Defaults to <see cref="Spacing"/>. Override independently for tighter vertical rhythm.
    /// </summary>
    public const string RowGap = "--orbyss-form-row-gap";

    /// <summary>
    /// Gap between columns inside a horizontal row.
    /// Defaults to <see cref="Spacing"/>. Override independently for tighter side-by-side fields.
    /// </summary>
    public const string ColumnGap = "--orbyss-form-column-gap";

    /// <summary>
    /// Gap between buttons in the button row, and the top margin above it.
    /// Defaults to <see cref="Spacing"/>. Override independently for tighter button placement.
    /// </summary>
    public const string ButtonGap = "--orbyss-form-button-gap";
}
