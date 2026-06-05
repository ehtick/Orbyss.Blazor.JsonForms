namespace Orbyss.Blazor.JsonForms.Constants;

public static class FormUiSchemaOptionKeys
{
    public const string Detail = "detail";

    public const string ReadOnly = "readonly";

    public const string Disabled = "disabled";

    public const string Hidden = "hidden";

    /// <summary>
    /// Custom option key for a CSS class to apply to the control component.
    /// Merged with any programmatically assigned class: schema class comes first.
    /// Example: "cssClass": "my-custom-class highlighted"
    /// </summary>
    public const string CssClass = "cssClass";

    /// <summary>
    /// Custom option key for a helper icon tooltip label.
    /// Value is resolved through the translation context (i18n key or literal string).
    /// Example: "helperIconLabel": "my.i18n.key"
    /// </summary>
    public const string HelperIconLabel = "helperIconLabel";
}