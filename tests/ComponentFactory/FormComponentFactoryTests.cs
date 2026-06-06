using Newtonsoft.Json.Linq;
using Orbyss.Blazor.JsonForms.ComponentBases;
using Orbyss.Blazor.JsonForms.ComponentFactory;
using Orbyss.Blazor.JsonForms.Constants;
using Orbyss.Blazor.JsonForms.Core.ComponentFactory;
using System.Linq.Expressions;

namespace Orbyss.Blazor.JsonForms.Tests.ComponentFactory;

[TestFixture]
public sealed class FormComponentFactoryTests
{
    // ── Test component types ─────────────────────────────────────────────────
    // Input components must extend FormInputComponentBase<T> (implements IFormComponent).

    private sealed class TextComponent : FormInputComponentBase<string?>
    {
        public string? Placeholder { get; set; }
    }

    // A component with properties whose names match engine-restricted parameter names.
    // Used in GuardRestricted tests — declared as a non-input type so the slot guard
    // does not fire before GuardRestricted gets a chance to run.
    private sealed class ComponentWithRestrictedProps
    {
        public string? Value        { get; set; }
        public string? ValueChanged { get; set; }
        public string? Checked      { get; set; }
        public string? CheckedChanged { get; set; }
        public string? Values       { get; set; }
        public string? ValuesChanged { get; set; }
    }

    // A non-input component (button) — no IFormComponent constraint on non-input slots.
    private sealed class ButtonComponent
    {
        public string? Text     { get; set; }
        public bool    Disabled { get; set; }
    }

    // ── ControlComponentFactory — input type slots ────────────────────────────

    [Test]
    public void When_ControlFactory_InputSlots_AreNull_ByDefault()
    {
        var factory = new ControlComponentFactory();

        Assert.That(factory.TextInputComponentType,    Is.Null);
        Assert.That(factory.NumberInputComponentType,  Is.Null);
        Assert.That(factory.IntegerInputComponentType, Is.Null);
        Assert.That(factory.BooleanInputComponentType, Is.Null);
        Assert.That(factory.DropdownComponentType,     Is.Null);
        Assert.That(factory.MultiSelectComponentType,  Is.Null);
        Assert.That(factory.DateTimeInputComponentType,         Is.Null);
        Assert.That(factory.DateTimeUtcTicksInputComponentType, Is.Null);
        Assert.That(factory.DateOnlyInputComponentType,         Is.Null);
        Assert.That(factory.DateOnlyUtcTicksInputComponentType, Is.Null);
    }

    [Test]
    public void When_ControlFactory_InputSlot_IsSet_Then_Returns_AssignedType()
    {
        // TextComponent extends FormInputComponentBase<string?> → implements IFormComponent → valid
        var factory = new ControlComponentFactory { TextInputComponentType = typeof(TextComponent) };

        Assert.That(factory.TextInputComponentType, Is.EqualTo(typeof(TextComponent)));
    }

    [Test]
    public void When_ControlFactory_MultipleSlots_AreSet_Then_EachSlot_IsIndependent()
    {
        var factory = new ControlComponentFactory
        {
            TextInputComponentType = typeof(TextComponent)
        };

        Assert.That(factory.TextInputComponentType,   Is.EqualTo(typeof(TextComponent)));
        Assert.That(factory.NumberInputComponentType, Is.Null);
    }

    // ── IFormComponent validation ────────────────────────────────────────────

    [Test]
    public void When_InputSlot_IsSet_To_NonIFormComponent_Type_Then_Throws()
    {
        // ButtonComponent does NOT implement IFormComponent
        var factory = new ControlComponentFactory();

        Assert.Throws<InvalidOperationException>(() =>
        {
            factory.TextInputComponentType = typeof(ButtonComponent);
        });
    }

    // ── ButtonComponentFactory — non-input slots ─────────────────────────────

    [Test]
    public void When_ButtonFactory_Slots_AreNull_ByDefault()
    {
        var factory = new ButtonComponentFactory();

        Assert.That(factory.SubmitButtonComponentType,   Is.Null);
        Assert.That(factory.NextButtonComponentType,     Is.Null);
        Assert.That(factory.PreviousButtonComponentType, Is.Null);
    }

    [Test]
    public void When_NonInputSlot_IsSet_To_NonIFormComponent_Type_Then_DoesNotThrow()
    {
        // Non-input slots (buttons, navigation, list, etc.) have no IFormComponent requirement
        Assert.DoesNotThrow(() =>
        {
            _ = new ButtonComponentFactory
            {
                SubmitButtonComponentType   = typeof(ButtonComponent),
                NextButtonComponentType     = typeof(ButtonComponent),
                PreviousButtonComponentType = typeof(ButtonComponent)
            };
        });

        Assert.DoesNotThrow(() => { _ = new NavigationComponentFactory    { NavigationComponentType    = typeof(ButtonComponent) }; });
        Assert.DoesNotThrow(() => { _ = new ListComponentFactory          { ListComponentType          = typeof(ButtonComponent) }; });
        Assert.DoesNotThrow(() => { _ = new ActionButtonComponentFactory  { ActionButtonComponentType  = typeof(ButtonComponent) }; });
        Assert.DoesNotThrow(() => { _ = new ArrayLayoutComponentFactory   { ArrayLayoutComponentType   = typeof(ButtonComponent) }; });
    }

    // ── SetParameter — single registration ───────────────────────────────────
    // ControlComponentFactory.SetParameter is public on the concrete class.

    [Test]
    public void When_SetParameter_Called_Then_Parameter_IsRetrievable()
    {
        var factory = new ControlComponentFactory();
        factory.SetParameter<TextComponent, string?>(x => x.Placeholder, "Enter text…");

        var parameters = factory.GetAssignedParameters(typeof(TextComponent));

        Assert.That(parameters, Has.Count.EqualTo(1));
        Assert.That(parameters[0].ParameterName, Is.EqualTo("Placeholder"));
        Assert.That(parameters[0].Value, Is.EqualTo("Enter text…"));
    }

    [Test]
    public void When_SetParameter_CalledMultipleTimes_Then_AllParameters_AreRetrievable()
    {
        var factory = new ControlComponentFactory();
        factory.SetParameter<TextComponent, string?>(x => x.Placeholder, "Enter…");
        factory.SetParameter<TextComponent, bool>(x => x.Disabled, false);

        var parameters = factory.GetAssignedParameters(typeof(TextComponent));

        Assert.That(parameters, Has.Count.EqualTo(2));
        Assert.That(parameters.Any(p => p.ParameterName == "Placeholder"), Is.True);
        Assert.That(parameters.Any(p => p.ParameterName == "Disabled"),    Is.True);
    }

    [Test]
    public void When_SetParameter_For_DifferentTypes_Then_Parameters_AreIsolatedPerType()
    {
        var factory = new ControlComponentFactory();
        factory.SetParameter<TextComponent, string?>(x => x.Placeholder, "text");
        factory.SetParameter<ButtonComponent, string?>(x => x.Text, "Submit");

        var textParams   = factory.GetAssignedParameters(typeof(TextComponent));
        var buttonParams = factory.GetAssignedParameters(typeof(ButtonComponent));

        Assert.That(textParams,   Has.Count.EqualTo(1));
        Assert.That(buttonParams, Has.Count.EqualTo(1));
        Assert.That(textParams[0].ParameterName,   Is.EqualTo("Placeholder"));
        Assert.That(buttonParams[0].ParameterName, Is.EqualTo("Text"));
    }

    [Test]
    public void When_SetParameter_Returns_This_For_Chaining()
    {
        var factory = new ControlComponentFactory();
        var returned = factory.SetParameter<TextComponent, string?>(x => x.Placeholder, "Enter…");

        Assert.That(returned, Is.SameAs(factory));
    }

    // ── GetAssignedParameters — unknown type returns empty ───────────────────

    [Test]
    public void When_GetAssignedParameters_For_UnregisteredType_Then_Returns_Empty()
    {
        var factory = new ControlComponentFactory();

        var parameters = factory.GetAssignedParameters(typeof(TextComponent));

        Assert.That(parameters, Is.Empty);
    }

    // ── GuardRestricted — throws for engine-owned parameters ─────────────────

    [Test]
    [TestCase(nameof(ComponentWithRestrictedProps.Value))]
    [TestCase(nameof(ComponentWithRestrictedProps.ValueChanged))]
    [TestCase(nameof(ComponentWithRestrictedProps.Checked))]
    [TestCase(nameof(ComponentWithRestrictedProps.CheckedChanged))]
    [TestCase(nameof(ComponentWithRestrictedProps.Values))]
    [TestCase(nameof(ComponentWithRestrictedProps.ValuesChanged))]
    public void When_SetParameter_With_RestrictedName_Then_Throws(string restrictedName)
    {
        var factory = new ControlComponentFactory();

        // Use a direct property selector on a component that declares the restricted property name.
        // GuardRestricted fires after the parameter name is resolved from the expression.
        switch (restrictedName)
        {
            case nameof(ComponentWithRestrictedProps.Value):
                Assert.Throws<InvalidOperationException>(() =>
                    factory.SetParameter<ComponentWithRestrictedProps, string?>(x => x.Value, null));
                break;
            case nameof(ComponentWithRestrictedProps.ValueChanged):
                Assert.Throws<InvalidOperationException>(() =>
                    factory.SetParameter<ComponentWithRestrictedProps, string?>(x => x.ValueChanged, null));
                break;
            case nameof(ComponentWithRestrictedProps.Checked):
                Assert.Throws<InvalidOperationException>(() =>
                    factory.SetParameter<ComponentWithRestrictedProps, string?>(x => x.Checked, null));
                break;
            case nameof(ComponentWithRestrictedProps.CheckedChanged):
                Assert.Throws<InvalidOperationException>(() =>
                    factory.SetParameter<ComponentWithRestrictedProps, string?>(x => x.CheckedChanged, null));
                break;
            case nameof(ComponentWithRestrictedProps.Values):
                Assert.Throws<InvalidOperationException>(() =>
                    factory.SetParameter<ComponentWithRestrictedProps, string?>(x => x.Values, null));
                break;
            case nameof(ComponentWithRestrictedProps.ValuesChanged):
                Assert.Throws<InvalidOperationException>(() =>
                    factory.SetParameter<ComponentWithRestrictedProps, string?>(x => x.ValuesChanged, null));
                break;
        }
    }

    // ── FormComponentParameterKeys.Restricted set coverage ───────────────────

    [Test]
    public void Restricted_Set_Contains_All_Expected_Names()
    {
        var expected = new[]
        {
            FormComponentParameterKeys.Value,
            FormComponentParameterKeys.ValueChanged,
            FormComponentParameterKeys.Checked,
            FormComponentParameterKeys.CheckedChanged,
            FormComponentParameterKeys.Values,
            FormComponentParameterKeys.ValuesChanged,
        };

        foreach (var name in expected)
        {
            Assert.That(FormComponentParameterKeys.Restricted.Contains(name),
                Is.True, $"Expected '{name}' to be in Restricted set");
        }
    }

    [Test]
    public void Restricted_Set_Is_CaseInsensitive()
    {
        Assert.That(FormComponentParameterKeys.Restricted.Contains("value"),        Is.True);
        Assert.That(FormComponentParameterKeys.Restricted.Contains("VALUE"),        Is.True);
        Assert.That(FormComponentParameterKeys.Restricted.Contains("valuechanged"), Is.True);
        Assert.That(FormComponentParameterKeys.Restricted.Contains("CHECKED"),      Is.True);
    }

    // ── RegisterAlias / ResolveAlias ──────────────────────────────────────────

    [Test]
    public void When_RegisterAlias_Then_ResolveAlias_Returns_RegisteredType()
    {
        var factory = new ControlComponentFactory();
        factory.RegisterAlias("slider", typeof(TextComponent));

        var resolved = factory.ResolveAlias("slider");

        Assert.That(resolved, Is.EqualTo(typeof(TextComponent)));
    }

    [Test]
    public void When_ResolveAlias_CaseInsensitive_Then_Matches()
    {
        var factory = new ControlComponentFactory();
        factory.RegisterAlias("Slider", typeof(TextComponent));

        Assert.That(factory.ResolveAlias("slider"), Is.EqualTo(typeof(TextComponent)));
        Assert.That(factory.ResolveAlias("SLIDER"), Is.EqualTo(typeof(TextComponent)));
        Assert.That(factory.ResolveAlias("Slider"), Is.EqualTo(typeof(TextComponent)));
    }

    [Test]
    public void When_ResolveAlias_For_UnknownKey_Then_Returns_Null()
    {
        var factory = new ControlComponentFactory();

        Assert.That(factory.ResolveAlias("unknown"), Is.Null);
    }

    [Test]
    public void When_RegisterAlias_SameKey_Twice_Then_SecondRegistration_Wins()
    {
        var factory = new ControlComponentFactory();
        factory.RegisterAlias("widget", typeof(TextComponent));
        factory.RegisterAlias("widget", typeof(ButtonComponent));

        Assert.That(factory.ResolveAlias("widget"), Is.EqualTo(typeof(ButtonComponent)));
    }

    [Test]
    public void When_MultipleAliases_Registered_Then_Each_Resolves_Independently()
    {
        var factory = new ControlComponentFactory();
        factory.RegisterAlias("slider", typeof(TextComponent));
        factory.RegisterAlias("blocks", typeof(ButtonComponent));

        Assert.That(factory.ResolveAlias("slider"), Is.EqualTo(typeof(TextComponent)));
        Assert.That(factory.ResolveAlias("blocks"), Is.EqualTo(typeof(ButtonComponent)));
    }
}
