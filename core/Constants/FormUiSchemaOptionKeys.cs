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

    /// <summary>
    /// Helper text shown below the control.
    /// Resolved via the translation context (i18n key or literal string).
    /// Schema-defined value overwrites any programmatically set helper text.
    /// </summary>
    public const string HelperTextLabel = "helperTextLabel";

    /// <summary>
    /// Per-enum metadata object keyed by enum value.
    /// Each entry may contain a <c>helperText</c> property that is rendered below the enum item label.
    /// </summary>
    public const string EnumItemOptions = "enumItemOptions";

    /// <summary>
    /// Prefix text prepended to numeric display values (e.g. <c>€</c>).
    /// Resolved via the translation context. Schema-defined value overwrites any programmatically set prefix.
    /// </summary>
    public const string PrefixLabel = "prefixLabel";

    /// <summary>
    /// Suffix text appended to numeric display values (e.g. <c>m</c>, <c>kg</c>).
    /// Resolved via the translation context. Schema-defined value overwrites any programmatically set suffix.
    /// </summary>
    public const string SuffixLabel = "suffixLabel";

    /// <summary>
    /// Key identifying the registered action handler for an <c>ActionButton</c> element.
    /// Must match a key passed to <c>JsonFormContextOptions.RegisterAction</c>.
    /// Example: "actionKey": "calculate-premium"
    /// </summary>
    public const string ActionKey = "actionKey";

    /// <summary>
    /// Translation key used as the label for the "add item" button on an <c>ArrayLayout</c> element.
    /// Resolved via the translation schema. When omitted the default label <c>+</c> is shown.
    /// Example: "addLabel": "addAddress"
    /// </summary>
    public const string AddLabel = "addLabel";

    /// <summary>
    /// Alias key registered on <c>FormComponentFactory</c> that selects a custom component type
    /// for this specific control, overriding the factory's default slot type.
    /// Example: "component": "slider"
    /// </summary>
    public const string Component = "component";

    /// <summary>
    /// JSON object of Blazor parameter name → value pairs applied on top of the factory's
    /// registered parameters for this specific control (layer 3 in the parameter precedence chain).
    /// Parameter name matching is case-insensitive.
    /// Example: "parameters": { "Step": 100, "Tooltip": true }
    /// </summary>
    public const string Parameters = "parameters";
}
