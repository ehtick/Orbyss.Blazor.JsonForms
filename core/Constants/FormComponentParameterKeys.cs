namespace Orbyss.Blazor.JsonForms.Constants;

/// <summary>
/// Well-known Blazor parameter names used by the engine when wiring component instances.
/// Use these constants everywhere a parameter key is needed — avoids magic strings across
/// factory implementations, Razor components, and UI schema parsing.
/// </summary>
public static class FormComponentParameterKeys
{
    // ── Engine-restricted: value binding (cannot be overridden by consumers) ──

    /// <summary>The current bound value. Set exclusively by the engine from form data.</summary>
    public const string Value = "Value";

    /// <summary>Callback fired when the component's value changes. Wired to <c>IJsonFormContext.UpdateValue</c>.</summary>
    public const string ValueChanged = "ValueChanged";

    /// <summary>Boolean equivalent of <see cref="Value"/> for checkbox / switch components.</summary>
    public const string Checked = "Checked";

    /// <summary>Callback equivalent of <see cref="ValueChanged"/> for checkbox / switch components.</summary>
    public const string CheckedChanged = "CheckedChanged";

    /// <summary>Multi-select equivalent of <see cref="Value"/> for enum-list components.</summary>
    public const string Values = "Values";

    /// <summary>Callback equivalent of <see cref="ValueChanged"/> for enum-list components.</summary>
    public const string ValuesChanged = "ValuesChanged";

    /// <summary>
    /// All engine-owned parameter names as a case-insensitive set for fast lookup.
    /// Attempting to set any of these via <c>SetParameter</c> or UI schema <c>"parameters"</c>
    /// throws <see cref="InvalidOperationException"/>.
    /// </summary>
    public static readonly IReadOnlySet<string> Restricted = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Value, ValueChanged, Checked, CheckedChanged, Values, ValuesChanged
    };

    // ── Standard input parameters ─────────────────────────────────────────────

    /// <summary>Display label shown above or beside the control.</summary>
    public const string Label = "Label";

    /// <summary>When <c>true</c> the control is greyed-out and ignores user interaction.</summary>
    public const string Disabled = "Disabled";

    /// <summary>When <c>true</c> the control displays its value but prevents editing.</summary>
    public const string ReadOnly = "ReadOnly";

    /// <summary>Additional CSS class(es) appended to the component's root element.</summary>
    public const string Class = "Class";

    /// <summary>Inline CSS style applied to the component's root element.</summary>
    public const string Style = "Style";

    /// <summary>Culture used for formatting numeric and date values.</summary>
    public const string Culture = "Culture";

    /// <summary>Secondary descriptive text rendered below the control.</summary>
    public const string HelperText = "HelperText";

    /// <summary>Tooltip text shown on a helper icon beside the control.</summary>
    public const string HelperIconText = "HelperIconText";

    /// <summary>Validation error text rendered below the control; set to <c>null</c> to clear.</summary>
    public const string ErrorHelperText = "ErrorHelperText";

    /// <summary>Collection of translated options rendered by dropdown / multiselect components.</summary>
    public const string Items = "Items";

    /// <summary>When <c>true</c> the dropdown allows multiple selections.</summary>
    public const string MultiSelect = "MultiSelect";

    /// <summary>Static text displayed before the input field (numeric inputs).</summary>
    public const string PrefixText = "PrefixText";

    /// <summary>Static text displayed after the input field (numeric inputs).</summary>
    public const string SuffixText = "SuffixText";

    /// <summary>Placeholder text shown inside an empty input field.</summary>
    public const string Placeholder = "Placeholder";

    // ── EventCallbacks wired by Razor components ──────────────────────────────
    // These are added by the Razor component AFTER Create* returns — they capture 'this'.

    /// <summary>Fired when the component's committed value changes. Triggers form-level side-effects.</summary>
    public const string OnValueChanged = "OnValueChanged";

    /// <summary>
    /// Fired on every raw input event (e.g. each keystroke). Components that do not declare
    /// this parameter will have it stripped automatically by <c>RemoveUndeclaredParameters</c>.
    /// </summary>
    public const string OnInputChanged = "OnInputChanged";

    // ── Button parameters ─────────────────────────────────────────────────────

    /// <summary>Button label text.</summary>
    public const string Text = "Text";

    /// <summary>Fired when the button is clicked (Submit / Next / Previous buttons).</summary>
    public const string OnClicked = "OnClicked";

    // ── Navigation parameters ─────────────────────────────────────────────────

    /// <summary>Callback invoked when the user submits the final page of a multi-page form.</summary>
    public const string OnSubmitClicked = "OnSubmitClicked";

    /// <summary>When <c>true</c> the navigation component hides its submit button.</summary>
    public const string HideSubmitButton = "HideSubmitButton";

    // ── List parameters ───────────────────────────────────────────────────────

    /// <summary>Heading text rendered at the top of a list component.</summary>
    public const string Title = "Title";

    /// <summary>Validation error message displayed on a list component.</summary>
    public const string Error = "Error";

    /// <summary>Callback invoked when the user requests a new list item to be added.</summary>
    public const string OnAddItemClicked = "OnAddItemClicked";

    // ── List-item parameters ──────────────────────────────────────────────────

    /// <summary>Callback invoked when the user requests the current list item to be removed.</summary>
    public const string OnRemoveItemClicked = "OnRemoveItemClicked";

    // ── Action button parameters ──────────────────────────────────────────────

    /// <summary>Callback invoked when an action button is clicked.</summary>
    public const string OnClick = "OnClick";

    // ── Array-layout parameters ───────────────────────────────────────────────

    /// <summary>The <c>FormArrayContext</c> describing the repeating array section.</summary>
    public const string ArrayContext = "ArrayContext";

    /// <summary>Label shown on the "add item" button inside an array layout.</summary>
    public const string AddLabel = "AddLabel";

    // ── Render fragment ───────────────────────────────────────────────────────

    /// <summary>Child content render fragment used by wrapper components (list, list-item).</summary>
    public const string ChildContent = "ChildContent";
}
