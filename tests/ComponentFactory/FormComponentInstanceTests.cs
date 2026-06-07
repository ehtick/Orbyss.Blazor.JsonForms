using Orbyss.Blazor.JsonForms.Core.ComponentFactory;

namespace Orbyss.Blazor.JsonForms.Tests.ComponentFactory;

[TestFixture]
public sealed class FormComponentInstanceTests
{
    // ── FormComponentInstance ────────────────────────────────────────────────

    [Test]
    public void When_Created_Then_ComponentType_IsSet()
    {
        var instance = new FormComponentInstance(typeof(string));

        Assert.That(instance.ComponentType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void When_Created_Then_Parameters_IsEmpty()
    {
        var instance = new FormComponentInstance(typeof(string));

        Assert.That(instance.Parameters, Is.Empty);
    }

    [Test]
    public void When_Parameter_Added_Then_IsRetrievable()
    {
        var instance = new FormComponentInstance(typeof(string));
        instance.Parameters["Label"] = "First name";

        Assert.That(instance.Parameters["Label"], Is.EqualTo("First name"));
    }

    [Test]
    public void When_Parameter_Added_With_DifferentCase_Then_Overwrites()
    {
        // Parameters dictionary is case-insensitive
        var instance = new FormComponentInstance(typeof(string));
        instance.Parameters["Label"] = "First name";
        instance.Parameters["label"] = "Last name";

        Assert.That(instance.Parameters, Has.Count.EqualTo(1));
        Assert.That(instance.Parameters["LABEL"], Is.EqualTo("Last name"));
    }

    [Test]
    public void When_Treated_As_IComponentInstance_Then_Members_AreAccessible()
    {
        IComponentInstance instance = new FormComponentInstance(typeof(int));
        instance.Parameters["X"] = 42;

        Assert.That(instance.ComponentType, Is.EqualTo(typeof(int)));
        Assert.That(instance.Parameters["X"], Is.EqualTo(42));
    }
}
