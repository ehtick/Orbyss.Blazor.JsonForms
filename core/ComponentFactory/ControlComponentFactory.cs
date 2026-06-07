using Newtonsoft.Json.Linq;
using System.Linq.Expressions;
using Orbyss.Blazor.JsonForms.Core.Constants;
using Orbyss.Blazor.JsonForms.Core.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Core.Interpretation;
using Orbyss.Blazor.JsonForms.Core.ComponentFactory.SubFactories;
using Orbyss.Blazor.JsonForms.Core.Utils;
using Orbyss.Blazor.JsonForms.Core.Context.Models;

namespace Orbyss.Blazor.JsonForms.Core.ComponentFactory;

/// <summary>
/// Default implementation of <see cref="IControlComponentFactory"/>.
/// Manages the 10 input component type slots, static parameter registrations, and alias
/// resolution for data-bound control fields.
///
/// <para>
/// All types assigned to input slots must implement <see cref="IFormComponent"/>. Validation
/// happens at assignment time so mistakes surface before the first render. The factory caches
/// one <see cref="IFormComponent"/> instance per component type to perform JToken conversion
/// without re-allocating on every render.
/// </para>
/// </summary>
public class ControlComponentFactory : ComponentFactoryBase, IControlComponentFactory
{
    // ── Input component type slots ────────────────────────────────────────────
    // All slots require IFormComponent so the factory can convert JToken to CLR values.

    private Type? _textInputComponentType;
    private Type? _numberInputComponentType;
    private Type? _integerInputComponentType;
    private Type? _booleanInputComponentType;
    private Type? _dropdownComponentType;
    private Type? _multiSelectComponentType;
    private Type? _dateTimeInputComponentType;
    private Type? _dateTimeUtcTicksInputComponentType;
    private Type? _dateOnlyInputComponentType;
    private Type? _dateOnlyUtcTicksInputComponentType;

    public Type? TextInputComponentType
    {
        get => _textInputComponentType;
        set { GuardInputComponent(value, nameof(TextInputComponentType)); _textInputComponentType = value; }
    }

    public Type? NumberInputComponentType
    {
        get => _numberInputComponentType;
        set { GuardInputComponent(value, nameof(NumberInputComponentType)); _numberInputComponentType = value; }
    }

    public Type? IntegerInputComponentType
    {
        get => _integerInputComponentType;
        set { GuardInputComponent(value, nameof(IntegerInputComponentType)); _integerInputComponentType = value; }
    }

    public Type? BooleanInputComponentType
    {
        get => _booleanInputComponentType;
        set { GuardInputComponent(value, nameof(BooleanInputComponentType)); _booleanInputComponentType = value; }
    }

    /// <summary>Single-select enum dropdown.</summary>
    public Type? DropdownComponentType
    {
        get => _dropdownComponentType;
        set { GuardInputComponent(value, nameof(DropdownComponentType)); _dropdownComponentType = value; }
    }

    /// <summary>Multi-select enum list.</summary>
    public Type? MultiSelectComponentType
    {
        get => _multiSelectComponentType;
        set { GuardInputComponent(value, nameof(MultiSelectComponentType)); _multiSelectComponentType = value; }
    }

    public Type? DateTimeInputComponentType
    {
        get => _dateTimeInputComponentType;
        set { GuardInputComponent(value, nameof(DateTimeInputComponentType)); _dateTimeInputComponentType = value; }
    }

    public Type? DateTimeUtcTicksInputComponentType
    {
        get => _dateTimeUtcTicksInputComponentType;
        set { GuardInputComponent(value, nameof(DateTimeUtcTicksInputComponentType)); _dateTimeUtcTicksInputComponentType = value; }
    }

    public Type? DateOnlyInputComponentType
    {
        get => _dateOnlyInputComponentType;
        set { GuardInputComponent(value, nameof(DateOnlyInputComponentType)); _dateOnlyInputComponentType = value; }
    }

    public Type? DateOnlyUtcTicksInputComponentType
    {
        get => _dateOnlyUtcTicksInputComponentType;
        set { GuardInputComponent(value, nameof(DateOnlyUtcTicksInputComponentType)); _dateOnlyUtcTicksInputComponentType = value; }
    }

    // ── Public SetParameter (shadows protected base, returns concrete type for chaining) ──

    /// <summary>
    /// Registers a static parameter value for <typeparamref name="TComponent"/>.
    /// Throws for engine-restricted parameter names. Returns <c>this</c> for chaining.
    /// </summary>
    public new ControlComponentFactory SetParameter<TComponent, TValue>(
        Expression<Func<TComponent, TValue>> selector,
        TValue value)
    {
        base.SetParameter(selector, value);
        return this;
    }

    // ── IFormComponent converter cache ────────────────────────────────────────

    private readonly Dictionary<Type, IFormComponent> _converterCache = new();

    private object? GetComponentValue(Type componentType, JToken? token, ControlType controlType)
    {
        // If the component implements IFormComponent, delegate conversion to it.
        // Otherwise fall back to the static ControlTypeLookup (e.g. for alias-resolved components
        // that do not inherit from FormInputComponentBase<TValue>).
        if (!typeof(IFormComponent).IsAssignableFrom(componentType))
            return ControlTypeLookup.ConvertFromJToken(token, controlType);

        if (!_converterCache.TryGetValue(componentType, out var converter))
        {
            converter = (IFormComponent)Activator.CreateInstance(componentType)!;
            _converterCache[componentType] = converter;
        }

        return converter.ConvertFromJToken(token);
    }

    // ── IControlComponentFactory ──────────────────────────────────────────────

    /// <inheritdoc />
    public virtual IComponentInstance CreateControl(IJsonFormContext formContext, FormControlContext control)
    {
        var interpretation = control.Interpretation;

        // Resolve component type: UI schema alias → slot default (override ResolveComponentType
        // to pick a variant by control type for an alias that maps to several components).
        var alias = interpretation.GetOption(FormUiSchemaOptionKeys.Component)?.ToString();
        var componentType = ResolveComponentType(alias, interpretation.ControlType)
            ?? throw new InvalidOperationException(
                $"No component registered for control type '{interpretation.ControlType}'. " +
                $"Set the corresponding *ComponentType slot on {nameof(ControlComponentFactory)}.");

        var instance = new FormComponentInstance(componentType);

        // Layer 1 — engine auto-wire
        instance.Parameters[FormComponentParameterKeys.Value]           = GetComponentValue(componentType, formContext.GetValue(control.Id), interpretation.ControlType);
        instance.Parameters[FormComponentParameterKeys.Label]           = formContext.GetLabel(control.Id);
        instance.Parameters[FormComponentParameterKeys.Disabled]        = formContext.Disabled || control.Disabled;
        instance.Parameters[FormComponentParameterKeys.ReadOnly]        = formContext.ReadOnly || control.ReadOnly;
        instance.Parameters[FormComponentParameterKeys.Culture]         = FormCulture.Instance;
        instance.Parameters[FormComponentParameterKeys.HelperIconText]  = formContext.GetHelperIconText(control.Id);
        instance.Parameters[FormComponentParameterKeys.ErrorHelperText] = null;

        var helperText = formContext.GetHelperText(control.Id);
        if (helperText is not null)
            instance.Parameters[FormComponentParameterKeys.HelperText] = helperText;

        if (interpretation.ControlType is ControlType.Enum or ControlType.EnumList)
        {
            instance.Parameters[FormComponentParameterKeys.Items]       = formContext.GetTranslatedEnumItems(control.Id);
            instance.Parameters[FormComponentParameterKeys.MultiSelect] = interpretation.ControlType == ControlType.EnumList;
        }

        if (interpretation.ControlType is ControlType.Number or ControlType.Integer)
        {
            var prefix = formContext.GetPrefixText(control.Id);
            if (prefix is not null) instance.Parameters[FormComponentParameterKeys.PrefixText] = prefix;

            var suffix = formContext.GetSuffixText(control.Id);
            if (suffix is not null) instance.Parameters[FormComponentParameterKeys.SuffixText] = suffix;
        }

        // Layer 2 — factory SetParameter assignments
        ApplyAssignedParameters(instance, componentType);

        // Merge CSS class: factory-registered + schema (schema supplements, does not replace)
        MergeCssClass(instance, formContext.GetCssClass(control.Id));

        // Layer 3 — UI schema "parameters" option
        ApplyUiSchemaParameters(instance, interpretation.GetOption(FormUiSchemaOptionKeys.Parameters));

        RemoveUndeclaredParameters.Remove(componentType, instance.Parameters);
        return instance;
    }

    // ── Component-type resolution ─────────────────────────────────────────────

    /// <summary>
    /// Resolves the component type for a control: the UI-schema <c>component</c> alias
    /// (<see cref="RegisterAlias"/>) takes precedence over the control-type slot.
    ///
    /// <para>
    /// Override this when a single alias must map to different components depending on the control
    /// type — e.g. a <c>"slider"</c> alias that resolves to an integer slider for
    /// <see cref="ControlType.Integer"/> and a number slider for <see cref="ControlType.Number"/>,
    /// which a flat <see cref="RegisterAlias"/> (one type per key) cannot express. Return
    /// <c>null</c> to fall through to the engine's "no component registered" error.
    /// </para>
    /// </summary>
    protected virtual Type? ResolveComponentType(string? alias, ControlType controlType)
        => (alias is not null ? ResolveAlias(alias) : null)
            ?? GetSlotTypeForControlType(controlType);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Type? GetSlotTypeForControlType(ControlType controlType) => controlType switch
    {
        ControlType.String           => TextInputComponentType,
        ControlType.Number           => NumberInputComponentType,
        ControlType.Integer          => IntegerInputComponentType,
        ControlType.Boolean          => BooleanInputComponentType,
        ControlType.Enum             => DropdownComponentType,
        ControlType.EnumList         => MultiSelectComponentType,
        ControlType.DateTime         => DateTimeInputComponentType,
        ControlType.DateTimeUtcTicks => DateTimeUtcTicksInputComponentType,
        ControlType.DateOnly         => DateOnlyInputComponentType,
        ControlType.DateOnlyUtcTicks => DateOnlyUtcTicksInputComponentType,
        _ => null
    };

    private static void GuardInputComponent(Type? type, string slotName)
    {
        if (type is not null && !typeof(IFormComponent).IsAssignableFrom(type))
            throw new InvalidOperationException(
                $"Component type '{type.Name}' registered for slot '{slotName}' must implement " +
                $"'{nameof(IFormComponent)}'. Inherit from FormInputComponentBase<TValue> for the " +
                $"standard wiring, or implement IFormComponent directly with a custom ConvertFromJToken.");
    }
}
