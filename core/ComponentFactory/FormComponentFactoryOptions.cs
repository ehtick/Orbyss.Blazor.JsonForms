using Orbyss.Blazor.JsonForms.Core.ComponentFactory;

namespace Orbyss.Blazor.JsonForms.ComponentFactory;

/// <summary>
/// Configuration surface for the default component sub-factories, applied during
/// <c>AddJsonForms</c>. Each callback receives the concrete sub-factory instance that the
/// DI container will register as a singleton, so consumers can assign component-type slots,
/// register static parameters (<c>SetParameter</c>), and register UI-schema aliases
/// (<c>RegisterAlias</c>) in one place.
///
/// <para>
/// A callback left <c>null</c> leaves that sub-factory at its defaults. To replace a sub-factory
/// wholesale (rather than configure the default), register your own implementation of the relevant
/// sub-factory interface in DI — <c>AddJsonForms</c> only supplies a default when none is present.
/// </para>
///
/// <example>
/// <code>
/// services.AddJsonForms(configure: o =>
/// {
///     o.ConfigureControls = controls =>
///     {
///         controls.TextInputComponentType   = typeof(MyTextBox);
///         controls.NumberInputComponentType = typeof(MyNumberInput);
///         controls.SetParameter&lt;MyTextBox, string?&gt;(x =&gt; x.Width, "100%");
///         controls.RegisterAlias("slider", typeof(MySlider));
///     };
///     o.ConfigureButtons = buttons =>
///         buttons.SubmitButtonComponentType = typeof(MyButton);
/// });
/// </code>
/// </example>
/// </summary>
public sealed class FormComponentFactoryOptions
{
    /// <summary>Configures the <see cref="ControlComponentFactory"/> (data-bound input controls).</summary>
    public Action<ControlComponentFactory>? ConfigureControls { get; set; }

    /// <summary>Configures the <see cref="ButtonComponentFactory"/> (Submit / Next / Previous buttons).</summary>
    public Action<ButtonComponentFactory>? ConfigureButtons { get; set; }

    /// <summary>Configures the <see cref="NavigationComponentFactory"/> (multi-page navigation wrapper).</summary>
    public Action<NavigationComponentFactory>? ConfigureNavigation { get; set; }

    /// <summary>Configures the <see cref="ListComponentFactory"/> (list containers and list items).</summary>
    public Action<ListComponentFactory>? ConfigureList { get; set; }

    /// <summary>Configures the <see cref="ActionButtonComponentFactory"/> (schema-driven action buttons).</summary>
    public Action<ActionButtonComponentFactory>? ConfigureActionButtons { get; set; }

    /// <summary>Configures the <see cref="ArrayLayoutComponentFactory"/> (inline array repeaters).</summary>
    public Action<ArrayLayoutComponentFactory>? ConfigureArrayLayout { get; set; }
}
