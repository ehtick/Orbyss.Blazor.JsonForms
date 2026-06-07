using Orbyss.Blazor.JsonForms.Core.ComponentFactory.SubFactories;
using Orbyss.Blazor.JsonForms.Core.Constants;
using Orbyss.Blazor.JsonForms.Core.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Core.Utils;
using System.Linq.Expressions;

namespace Orbyss.Blazor.JsonForms.Core.ComponentFactory;

/// <summary>
/// Default implementation of <see cref="IButtonComponentFactory"/>.
/// Manages the Submit, Next, and Previous button component type slots.
/// No <see cref="IFormComponent"/> constraint — button components do not perform JToken conversion.
/// </summary>
public class ButtonComponentFactory : ComponentFactoryBase, IButtonComponentFactory
{
    // ── Button type slots (no IFormComponent constraint) ──────────────────────

    public Type? SubmitButtonComponentType   { get; set; }
    public Type? NextButtonComponentType     { get; set; }
    public Type? PreviousButtonComponentType { get; set; }

    // ── Public SetParameter ───────────────────────────────────────────────────

    /// <summary>
    /// Registers a static parameter value for <typeparamref name="TComponent"/>.
    /// Throws for engine-restricted parameter names. Returns <c>this</c> for chaining.
    /// </summary>
    public new ButtonComponentFactory SetParameter<TComponent, TValue>(
        Expression<Func<TComponent, TValue>> selector,
        TValue value)
    {
        base.SetParameter(selector, value);
        return this;
    }

    // ── IButtonComponentFactory ───────────────────────────────────────────────

    /// <inheritdoc />
    public virtual IComponentInstance CreateButton(FormButtonType buttonType, IJsonFormContext? formContext)
    {
        var componentType = buttonType switch
        {
            FormButtonType.Submit   => SubmitButtonComponentType,
            FormButtonType.Next     => NextButtonComponentType,
            FormButtonType.Previous => PreviousButtonComponentType,
            _ => throw new ArgumentOutOfRangeException(nameof(buttonType), buttonType, null)
        } ?? throw new InvalidOperationException(
            $"No component registered for button type '{buttonType}'. " +
            $"Set the corresponding *ButtonComponentType slot on {nameof(ButtonComponentFactory)}.");

        var instance = new FormComponentInstance(componentType);

        // Layer 1 — engine auto-wire
        instance.Parameters[FormComponentParameterKeys.Disabled] = formContext?.Disabled ?? false;
        instance.Parameters[FormComponentParameterKeys.Culture]  = FormCulture.Instance;

        // Layer 2 — factory SetParameter assignments
        ApplyAssignedParameters(instance, componentType);

        RemoveUndeclaredParameters.Remove(componentType, instance.Parameters);
        return instance;
    }
}
