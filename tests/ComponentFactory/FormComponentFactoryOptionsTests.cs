using Microsoft.Extensions.DependencyInjection;
using Orbyss.Blazor.JsonForms.Core.ComponentBases;
using Orbyss.Blazor.JsonForms.Core.ComponentFactory;
using Orbyss.Blazor.JsonForms.Core.ComponentFactory.SubFactories;
using Orbyss.Blazor.JsonForms.Core.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Core.Context.Models;
using Orbyss.Blazor.JsonForms.Extensions;

namespace Orbyss.Blazor.JsonForms.Tests.ComponentFactory;

public sealed class FormComponentFactoryOptionsTests
{
    private sealed class DummyInput : FormInputComponentBase<string?> { }
    private sealed class OtherInput : FormInputComponentBase<string?> { }

    // Implements the interface WITHOUT deriving from the configurable ControlComponentFactory.
    private sealed class FakeControlFactory : IControlComponentFactory
    {
        public IComponentInstance CreateControl(IJsonFormContext formContext, FormControlContext control)
            => throw new NotSupportedException();
    }

    private static (IButtonComponentFactory, INavigationComponentFactory, IListComponentFactory,
                    IActionButtonComponentFactory, IArrayLayoutComponentFactory) Defaults()
        => (new ButtonComponentFactory(), new NavigationComponentFactory(), new ListComponentFactory(),
            new ActionButtonComponentFactory(), new ArrayLayoutComponentFactory());

    // ── ApplyTo (per-form pipeline) ───────────────────────────────────────────

    [Xunit.Fact]
    public void When_ApplyTo_Then_ConfigureControls_OverridesSlot()
    {
        var control = new ControlComponentFactory { TextInputComponentType = typeof(DummyInput) };
        var (b, n, l, a, ar) = Defaults();
        var options = new FormComponentFactoryOptions
        {
            ConfigureControls = c => c.TextInputComponentType = typeof(OtherInput)
        };

        options.ApplyTo(control, b, n, l, a, ar);

        Assert.That(control.TextInputComponentType, Is.EqualTo(typeof(OtherInput)));
    }

    [Xunit.Fact]
    public void When_ApplyTo_And_OverrideAfterDefault_Then_PerFormParameterWinsLast()
    {
        var control = new ControlComponentFactory();
        control.SetParameter<DummyInput, string?>(x => x.Class, "app-default");   // application baseline
        var (b, n, l, a, ar) = Defaults();
        var options = new FormComponentFactoryOptions
        {
            ConfigureControls = c => c.SetParameter<DummyInput, string?>(x => x.Class, "per-form")
        };

        options.ApplyTo(control, b, n, l, a, ar);

        // Entries apply in order; the per-form one is appended last, so it wins (last write wins).
        var entries = control.GetAssignedParameters(typeof(DummyInput));
        Assert.That(entries, Has.Count.EqualTo(2));
        Assert.That(entries[^1].Value, Is.EqualTo("per-form"));
    }

    [Xunit.Fact]
    public void When_ApplyTo_And_RegisterAlias_Then_AliasResolves()
    {
        var control = new ControlComponentFactory();
        var (b, n, l, a, ar) = Defaults();
        var options = new FormComponentFactoryOptions
        {
            ConfigureControls = c => c.RegisterAlias("rating", typeof(OtherInput))
        };

        options.ApplyTo(control, b, n, l, a, ar);

        Assert.That(control.ResolveAlias("rating"), Is.EqualTo(typeof(OtherInput)));
    }

    [Xunit.Fact]
    public void When_ApplyTo_And_FactoryNotDerivedFromConfigurableBase_Then_Throws()
    {
        var (b, n, l, a, ar) = Defaults();
        var options = new FormComponentFactoryOptions { ConfigureControls = _ => { } };

        Assert.Throws<InvalidOperationException>(
            () => options.ApplyTo(new FakeControlFactory(), b, n, l, a, ar));
    }

    [Xunit.Fact]
    public void When_ApplyTo_And_NoCallbacks_Then_NothingChanges()
    {
        var control = new ControlComponentFactory { TextInputComponentType = typeof(DummyInput) };
        var (b, n, l, a, ar) = Defaults();

        new FormComponentFactoryOptions().ApplyTo(control, b, n, l, a, ar);

        Assert.That(control.TextInputComponentType, Is.EqualTo(typeof(DummyInput)));
    }

    // ── Transient registration (form-scoped instances) ────────────────────────

    [Xunit.Fact]
    public void When_AddJsonForms_Then_SubFactoriesAreTransient()
    {
        var services = new ServiceCollection();
        services.AddJsonForms();
        using var provider = services.BuildServiceProvider();

        var first  = provider.GetService<IControlComponentFactory>();
        var second = provider.GetService<IControlComponentFactory>();

        Assert.That(first, Is.Not.Null);
        Assert.That(second, Is.Not.Null);
        Assert.That(first, Is.Not.SameAs(second));   // a fresh instance per resolution → per form
    }

    [Xunit.Fact]
    public void When_AddJsonForms_WithConfiguredDefaults_Then_EveryInstanceStartsFromThoseDefaults()
    {
        var services = new ServiceCollection();
        services.AddJsonForms(configureFactories: o =>
            o.ConfigureControls = c => c.TextInputComponentType = typeof(DummyInput));
        using var provider = services.BuildServiceProvider();

        var a = (ControlComponentFactory)provider.GetService<IControlComponentFactory>()!;
        var b = (ControlComponentFactory)provider.GetService<IControlComponentFactory>()!;

        Assert.That(a.TextInputComponentType, Is.EqualTo(typeof(DummyInput)));
        Assert.That(b.TextInputComponentType, Is.EqualTo(typeof(DummyInput)));
    }
}

