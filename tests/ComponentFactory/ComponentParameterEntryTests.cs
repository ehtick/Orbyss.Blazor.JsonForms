using Orbyss.Blazor.JsonForms.ComponentFactory;
using System.Linq.Expressions;

namespace Orbyss.Blazor.JsonForms.Tests.ComponentFactory;

[TestFixture]
public sealed class ComponentParameterEntryTests
{
    // ── Dummy component type used as the expression target ───────────────────

    private sealed class DummyComponent
    {
        public string? Label { get; set; }
        public int MaxLength { get; set; }
        public bool Disabled { get; set; }
    }

    // ── ResolveParameterName — direct property access ────────────────────────

    [Test]
    public void When_Created_With_StringProperty_Then_ParameterName_IsResolvedCorrectly()
    {
        var entry = new ComponentParameterEntry<DummyComponent, string?>(
            x => x.Label,
            "Hello"
        );

        Assert.That(entry.ParameterName, Is.EqualTo("Label"));
    }

    [Test]
    public void When_Created_With_IntProperty_Then_ParameterName_IsResolvedCorrectly()
    {
        var entry = new ComponentParameterEntry<DummyComponent, int>(
            x => x.MaxLength,
            100
        );

        Assert.That(entry.ParameterName, Is.EqualTo("MaxLength"));
    }

    [Test]
    public void When_Created_With_BoolProperty_Then_ParameterName_IsResolvedCorrectly()
    {
        var entry = new ComponentParameterEntry<DummyComponent, bool>(
            x => x.Disabled,
            true
        );

        Assert.That(entry.ParameterName, Is.EqualTo("Disabled"));
    }

    // ── Value is stored correctly ────────────────────────────────────────────

    [Test]
    public void When_Created_Then_Value_IsStored()
    {
        var entry = new ComponentParameterEntry<DummyComponent, string?>(
            x => x.Label,
            "TestValue"
        );

        Assert.That(entry.Value, Is.EqualTo("TestValue"));
    }

    [Test]
    public void When_Created_With_NullValue_Then_Value_IsNull()
    {
        var entry = new ComponentParameterEntry<DummyComponent, string?>(
            x => x.Label,
            null
        );

        Assert.That(entry.Value, Is.Null);
    }

    // ── Cast expression (x => (object)x.Property) ───────────────────────────

    [Test]
    public void When_Created_With_CastExpression_Then_ParameterName_IsResolvedCorrectly()
    {
        // Cast the bool to object — covers the UnaryExpression path in ResolveParameterName
        Expression<Func<DummyComponent, object>> expr = x => x.Disabled;
        var entry = new ComponentParameterEntry<DummyComponent, object>(expr, true);

        Assert.That(entry.ParameterName, Is.EqualTo("Disabled"));
    }

    // ── Invalid expression throws ────────────────────────────────────────────

    [Test]
    public void When_Created_With_NonPropertyExpression_Then_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            _ = new ComponentParameterEntry<DummyComponent, int>(
                x => x.MaxLength + 1,   // arithmetic — not a member access
                0
            );
        });
    }

    // ── Abstract base class surface ──────────────────────────────────────────

    [Test]
    public void When_Accessed_Via_BaseClass_Then_ParameterName_And_Value_Accessible()
    {
        ComponentParameterEntry entry = new ComponentParameterEntry<DummyComponent, int>(
            x => x.MaxLength,
            42
        );

        Assert.That(entry.ParameterName, Is.EqualTo("MaxLength"));
        Assert.That(entry.Value, Is.EqualTo(42));
    }
}
