using Newtonsoft.Json.Linq;
using Orbyss.Blazor.JsonForms.Constants;
using Orbyss.Blazor.JsonForms.Interpretation;
using Orbyss.Components.Json.Models;

namespace Orbyss.Blazor.JsonForms.Tests.Constants;

[TestFixture]
public sealed class ControlTypeLookupTests
{
    // ── GetForControlType ────────────────────────────────────────────────────

    [Test]
    [TestCase(ControlType.String,           typeof(string))]
    [TestCase(ControlType.Number,           typeof(double?))]
    [TestCase(ControlType.Integer,          typeof(int?))]
    [TestCase(ControlType.Boolean,          typeof(bool))]
    [TestCase(ControlType.Enum,             typeof(string))]
    [TestCase(ControlType.DateTime,         typeof(DateTime?))]
    [TestCase(ControlType.DateOnly,         typeof(DateOnly?))]
    public void When_GetForControlType_Then_Returns_CorrectClrType(ControlType controlType, Type expectedType)
    {
        var result = ControlTypeLookup.GetForControlType(controlType);

        Assert.That(result, Is.EqualTo(expectedType));
    }

    // ── ConvertFromJToken — null / JSON null ──────────────────────────────────

    [Test]
    public void When_Token_IsNull_Then_Returns_Null_For_String()
    {
        Assert.That(ControlTypeLookup.ConvertFromJToken(null, ControlType.String), Is.Null);
    }

    [Test]
    public void When_Token_IsNull_Then_Returns_False_For_Boolean()
    {
        // Boolean has a non-nullable CLR type — null token → false
        var result = ControlTypeLookup.ConvertFromJToken(null, ControlType.Boolean);

        Assert.That(result, Is.EqualTo(false));
    }

    [Test]
    public void When_Token_IsJsonNull_Then_Returns_Null_For_Number()
    {
        var result = ControlTypeLookup.ConvertFromJToken(JValue.CreateNull(), ControlType.Number);

        Assert.That(result, Is.Null);
    }

    // ── ConvertFromJToken — value types ───────────────────────────────────────

    [Test]
    public void When_Token_IsString_Then_Returns_String()
    {
        var result = ControlTypeLookup.ConvertFromJToken(new JValue("hello"), ControlType.String);

        Assert.That(result, Is.EqualTo("hello"));
    }

    [Test]
    public void When_Token_IsNumber_Then_Returns_Double()
    {
        var result = ControlTypeLookup.ConvertFromJToken(new JValue(3.14), ControlType.Number);

        Assert.That(result, Is.EqualTo(3.14));
    }

    [Test]
    public void When_Token_IsInteger_Then_Returns_Int()
    {
        var result = ControlTypeLookup.ConvertFromJToken(new JValue(42), ControlType.Integer);

        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void When_Token_IsTrue_Then_Returns_True_For_Boolean()
    {
        var result = ControlTypeLookup.ConvertFromJToken(new JValue(true), ControlType.Boolean);

        Assert.That(result, Is.EqualTo(true));
    }

    [Test]
    public void When_Token_IsEnumString_Then_Returns_String()
    {
        var result = ControlTypeLookup.ConvertFromJToken(new JValue("active"), ControlType.Enum);

        Assert.That(result, Is.EqualTo("active"));
    }

    [Test]
    public void When_Token_IsEnumList_Then_Returns_IEnumerableOfString()
    {
        var token  = JArray.FromObject(new[] { "a", "b", "c" });
        var result = ControlTypeLookup.ConvertFromJToken(token, ControlType.EnumList);

        Assert.That(result, Is.InstanceOf<IEnumerable<string>>());
        Assert.That((IEnumerable<string>)result!, Is.EquivalentTo(new[] { "a", "b", "c" }));
    }

    [Test]
    public void When_Token_IsDateTimeUtcTicks_Then_Returns_DateTimeUtcTicks()
    {
        var ticks  = DateTime.UtcNow.Ticks;
        var result = ControlTypeLookup.ConvertFromJToken(new JValue(ticks), ControlType.DateTimeUtcTicks);

        Assert.That(result, Is.InstanceOf<DateTimeUtcTicks>());
        Assert.That(((DateTimeUtcTicks)result!).UtcTicks, Is.EqualTo(ticks));
    }

    [Test]
    public void When_Token_IsDateOnlyString_Then_Returns_DateOnly()
    {
        var result = ControlTypeLookup.ConvertFromJToken(new JValue("2024-06-15"), ControlType.DateOnly);

        Assert.That(result, Is.EqualTo(new DateOnly(2024, 6, 15)));
    }
}
