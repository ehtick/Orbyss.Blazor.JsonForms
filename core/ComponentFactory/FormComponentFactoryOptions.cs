using Orbyss.Blazor.JsonForms.Core.ComponentFactory.SubFactories;

namespace Orbyss.Blazor.JsonForms.Core.ComponentFactory;

/// <summary>
/// Configuration pipeline for the component sub-factories. Each callback receives the concrete
/// sub-factory so consumers can assign component-type slots, register static parameters
/// (<c>SetParameter</c>), and register UI-schema aliases (<c>RegisterAlias</c>) in one place.
///
/// <para>
/// Used in two scopes:
/// <list type="bullet">
///   <item>
///     <b>Application defaults</b> — passed to <c>AddJsonForms(configureFactories: …)</c>, applied to
///     each sub-factory the first time it is created. These become the baseline for every form.
///   </item>
///   <item>
///     <b>Per-form overrides</b> — passed to <c>&lt;JsonForm ConfigureFactories="…"/&gt;</c>. Because
///     the sub-factories are registered <b>transient</b>, each form gets its own instances; the
///     per-form callbacks run on those instances via <see cref="ApplyTo"/> on top of the application
///     defaults, so a form can override a component type, add parameters, or register an alias
///     without affecting any other form.
///   </item>
/// </list>
/// </para>
///
/// <para>
/// A callback left <c>null</c> leaves that sub-factory unchanged. To replace a sub-factory wholesale
/// (rather than configure the default), register your own implementation of the relevant sub-factory
/// interface in DI — <c>AddJsonForms</c> only supplies a default when none is present.
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

    /// <summary>
    /// Applies the configured callbacks to a concrete set of sub-factory instances. The engine calls
    /// this with the per-form (transient) sub-factories so the overrides are scoped to a single form.
    ///
    /// <para>
    /// Each callback targets the concrete configurable factory (e.g. <see cref="ControlComponentFactory"/>),
    /// so the resolved instance must derive from it — the bundled defaults and any subclass of them do.
    /// A factory that implements the sub-factory interface directly without deriving from the
    /// configurable base cannot be configured this way; build it fully and pass it as
    /// <c>&lt;JsonForm ComponentFactory="…"/&gt;</c> instead.
    /// </para>
    /// </summary>
    public void ApplyTo(
        IControlComponentFactory control,
        IButtonComponentFactory button,
        INavigationComponentFactory navigation,
        IListComponentFactory list,
        IActionButtonComponentFactory actionButton,
        IArrayLayoutComponentFactory arrayLayout)
    {
        if (ConfigureControls is not null)      ConfigureControls(AsConfigurable<ControlComponentFactory>(control));
        if (ConfigureButtons is not null)       ConfigureButtons(AsConfigurable<ButtonComponentFactory>(button));
        if (ConfigureNavigation is not null)    ConfigureNavigation(AsConfigurable<NavigationComponentFactory>(navigation));
        if (ConfigureList is not null)          ConfigureList(AsConfigurable<ListComponentFactory>(list));
        if (ConfigureActionButtons is not null) ConfigureActionButtons(AsConfigurable<ActionButtonComponentFactory>(actionButton));
        if (ConfigureArrayLayout is not null)   ConfigureArrayLayout(AsConfigurable<ArrayLayoutComponentFactory>(arrayLayout));
    }

    private static TFactory AsConfigurable<TFactory>(object factory) where TFactory : class
        => factory as TFactory
            ?? throw new InvalidOperationException(
                $"Per-form factory configuration requires the registered factory to derive from " +
                $"'{typeof(TFactory).Name}', but the registered '{factory.GetType().Name}' does not. " +
                $"Derive your factory from '{typeof(TFactory).Name}', or pass a fully-built " +
                $"IFormComponentFactory via <JsonForm ComponentFactory=\"…\"/> instead of ConfigureFactories.");
}
