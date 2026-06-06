using Orbyss.Blazor.JsonForms.Constants;
using System.Globalization;

namespace Orbyss.Blazor.JsonForms.ComponentFactory;

/// <summary>
/// Chainable extension methods on <see cref="IComponentInstance"/> for setting common
/// parameters without magic strings. Each method writes the corresponding
/// <see cref="FormComponentParameterKeys"/> key into <see cref="IComponentInstance.Parameters"/>.
/// </summary>
public static class ComponentInstanceExtensions
{
    // ── Display ───────────────────────────────────────────────────────────────

    /// <summary>Sets the <c>Label</c> parameter.</summary>
    public static IComponentInstance SetLabel(this IComponentInstance instance, string? label)
    {
        instance.Parameters[FormComponentParameterKeys.Label] = label;
        return instance;
    }

    /// <summary>Sets the <c>Title</c> parameter (list components).</summary>
    public static IComponentInstance SetTitle(this IComponentInstance instance, string? title)
    {
        instance.Parameters[FormComponentParameterKeys.Title] = title;
        return instance;
    }

    /// <summary>Sets the <c>HelperText</c> parameter.</summary>
    public static IComponentInstance SetHelperText(this IComponentInstance instance, string? helperText)
    {
        instance.Parameters[FormComponentParameterKeys.HelperText] = helperText;
        return instance;
    }

    /// <summary>Sets the <c>HelperIconText</c> parameter.</summary>
    public static IComponentInstance SetHelperIconText(this IComponentInstance instance, string? helperIconText)
    {
        instance.Parameters[FormComponentParameterKeys.HelperIconText] = helperIconText;
        return instance;
    }

    /// <summary>Sets the <c>ErrorHelperText</c> parameter (pass <c>null</c> to clear the error).</summary>
    public static IComponentInstance SetErrorHelperText(this IComponentInstance instance, string? errorHelperText)
    {
        instance.Parameters[FormComponentParameterKeys.ErrorHelperText] = errorHelperText;
        return instance;
    }

    /// <summary>Sets the <c>Error</c> parameter (list-level validation message).</summary>
    public static IComponentInstance SetError(this IComponentInstance instance, string? error)
    {
        instance.Parameters[FormComponentParameterKeys.Error] = error;
        return instance;
    }

    /// <summary>Sets the <c>Text</c> parameter (button label).</summary>
    public static IComponentInstance SetText(this IComponentInstance instance, string? text)
    {
        instance.Parameters[FormComponentParameterKeys.Text] = text;
        return instance;
    }

    // ── State ─────────────────────────────────────────────────────────────────

    /// <summary>Sets the <c>Disabled</c> parameter.</summary>
    public static IComponentInstance SetDisabled(this IComponentInstance instance, bool disabled)
    {
        instance.Parameters[FormComponentParameterKeys.Disabled] = disabled;
        return instance;
    }

    /// <summary>Sets the <c>ReadOnly</c> parameter.</summary>
    public static IComponentInstance SetReadOnly(this IComponentInstance instance, bool readOnly)
    {
        instance.Parameters[FormComponentParameterKeys.ReadOnly] = readOnly;
        return instance;
    }

    // ── Styling ───────────────────────────────────────────────────────────────

    /// <summary>Sets the <c>Class</c> parameter (additional CSS class(es)).</summary>
    public static IComponentInstance SetClass(this IComponentInstance instance, string? cssClass)
    {
        instance.Parameters[FormComponentParameterKeys.Class] = cssClass;
        return instance;
    }

    /// <summary>Sets the <c>Style</c> parameter (inline CSS).</summary>
    public static IComponentInstance SetStyle(this IComponentInstance instance, string? style)
    {
        instance.Parameters[FormComponentParameterKeys.Style] = style;
        return instance;
    }

    // ── Culture / localisation ────────────────────────────────────────────────

    /// <summary>Sets the <c>Culture</c> parameter used for numeric and date formatting.</summary>
    public static IComponentInstance SetCulture(this IComponentInstance instance, CultureInfo culture)
    {
        instance.Parameters[FormComponentParameterKeys.Culture] = culture;
        return instance;
    }

    // ── Value ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Sets the <c>Value</c> parameter directly.
    /// Only call this outside of initial factory creation (e.g. refresh callbacks).
    /// The engine sets this automatically for all input controls via the factory.
    /// </summary>
    public static IComponentInstance SetValue(this IComponentInstance instance, object? value)
    {
        instance.Parameters[FormComponentParameterKeys.Value] = value;
        return instance;
    }

    // ── Enum / multiselect ────────────────────────────────────────────────────

    /// <summary>Sets the <c>Items</c> parameter (dropdown / multiselect option list).</summary>
    public static IComponentInstance SetItems(this IComponentInstance instance, object? items)
    {
        instance.Parameters[FormComponentParameterKeys.Items] = items;
        return instance;
    }

    /// <summary>Sets the <c>MultiSelect</c> parameter.</summary>
    public static IComponentInstance SetMultiSelect(this IComponentInstance instance, bool multiSelect)
    {
        instance.Parameters[FormComponentParameterKeys.MultiSelect] = multiSelect;
        return instance;
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    /// <summary>Sets the <c>HideSubmitButton</c> parameter on a navigation component.</summary>
    public static IComponentInstance SetHideSubmitButton(this IComponentInstance instance, bool hide)
    {
        instance.Parameters[FormComponentParameterKeys.HideSubmitButton] = hide;
        return instance;
    }

    // ── Array layout ──────────────────────────────────────────────────────────

    /// <summary>Sets the <c>ArrayContext</c> parameter on an array layout component.</summary>
    public static IComponentInstance SetArrayContext(this IComponentInstance instance, object? arrayContext)
    {
        instance.Parameters[FormComponentParameterKeys.ArrayContext] = arrayContext;
        return instance;
    }

    /// <summary>Sets the <c>AddLabel</c> parameter on an array layout component.</summary>
    public static IComponentInstance SetAddLabel(this IComponentInstance instance, string? addLabel)
    {
        instance.Parameters[FormComponentParameterKeys.AddLabel] = addLabel;
        return instance;
    }

    // ── Generic ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Sets an arbitrary parameter by name. Prefer the typed overloads for known parameters.
    /// </summary>
    public static IComponentInstance SetParameter(
        this IComponentInstance instance, string key, object? value)
    {
        instance.Parameters[key] = value;
        return instance;
    }
}
