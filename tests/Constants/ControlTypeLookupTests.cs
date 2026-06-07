using Newtonsoft.Json.Linq;
using Orbyss.Blazor.JsonForms.Core.Constants;
using Orbyss.Blazor.JsonForms.Core.Interpretation;
using Orbyss.Blazor.JsonForms.Core.Models;

namespace Orbyss.Blazor.JsonForms.Tests.Constants;

public sealed class ControlTypeLookupTests
{
    // ── GetForControlType ────────────────────────────────────────────────────

    [Xunit.Theory]
    [Xunit.InlineData(ControlType.String,           typeof(string))]
    [Xunit.InlineData(ControlType.Number,           typeof(double?))]
    [Xunit.InlineData(ControlType.Integer,          typeof(int?))]
    [Xunit.InlineData(ControlType.Boolean,          typeof(bool))]
    [Xunit.InlineData(ControlType.Enum,             typeof(string))]
    [Xunit.InlineData(ControlType.DateTime,         typeof(DateTime?))]
    [Xunit.InlineData(ControlType.DateOnly,         typeof(DateOnly?))]
    public void When_GetForControlType_Then_Returns_CorrectClrType(ControlType controlType, Type expectedType)
    {
        var result = ControlTypeLookup.GetForControlType(controlType);

        Assert.That(result, Is.EqualTo(expectedType));
    }

    // ── ConvertFromJToken — null / JSON null ──────────────────────────────────

    [Xunit.Fact]
    public void When_Token_IsNull_Then_Returns_Null_For_String()
    {
        Assert.That(ControlTypeLookup.ConvertFromJToken(null, ControlType.String), Is.Null);
    }

    [Xunit.Fact]
    public void When_Token_IsNull_Then_Returns_False_For_Boolean()
    {
        // Boolean has a non-nullable CLR type — null token → false
        var result = ControlTypeLookup.ConvertFromJToken(null, ControlType.Boolean);

        Assert.That(result, Is.EqualTo(false));
    }

    [Xunit.Fact]
    public void When_Token_IsJsonNull_Then_Returns_Null_For_Number()
    {
        var result = ControlTypeLookup.ConvertFromJToken(JValue.CreateNull(), ControlType.Number);

        Assert.That(result, Is.Null);
    }

    // ── ConvertFromJToken — value types ───────────────────────────────────────

    [Xunit.Fact]
    public void When_Token_IsString_Then_Returns_String()
    {
        var result = ControlTypeLookup.ConvertFromJToken(new JValue("hello"), ControlType.String);

        Assert.That(result, Is.EqualTo("hello"));
    }

    [Xunit.Fact]
    public void When_Token_IsNumber_Then_Returns_Double()
    {
        var result = ControlTypeLookup.ConvertFromJToken(new JValue(3.14), ControlType.Number);

        Assert.That(result, Is.EqualTo(3.14));
    }

    [Xunit.Fact]
    public void When_Token_IsInteger_Then_Returns_Int()
    {
        var result = ControlTypeLookup.ConvertFromJToken(new JValue(42), ControlType.Integer);

        Assert.That(result, Is.EqualTo(42));
    }

    [Xunit.Fact]
    public void When_Token_IsTrue_Then_Returns_True_For_Boolean()
    {
        var result = ControlTypeLookup.ConvertFromJToken(new JValue(true), ControlType.Boolean);

        Assert.That(result, Is.EqualTo(true));
    }

    [Xunit.Fact]
    public void When_Token_IsEnumString_Then_Returns_String()
    {
        var result = ControlTypeLookup.ConvertFromJToken(new JValue("active"), ControlType.Enum);

        Assert.That(result, Is.EqualTo("active"));
    }

    [Xunit.Fact]
    public void When_Token_IsEnumList_Then_Returns_IEnumerableOfString()
    {
        var token  = JArray.FromObject(new[] { "a", "b", "c" });
        var result = ControlTypeLookup.ConvertFromJToken(token, ControlType.EnumList);

        Assert.That(result, Is.InstanceOf<IEnumerable<string>>());
        Assert.That((IEnumerable<string>)result!, Is.EquivalentTo(new[] { "a", "b", "c" }));
    }

    [Xunit.Fact]
    public void When_Token_IsDateTimeUtcTicks_Then_Returns_DateTimeUtcTicks()
    {
        var ticks  = DateTime.UtcNow.Ticks;
        var result = ControlTypeLookup.ConvertFromJToken(new JValue(ticks), ControlType.DateTimeUtcTicks);

        Assert.That(result, Is.InstanceOf<DateTimeUtcTicks>());
        Assert.That(((DateTimeUtcTicks)result!).UtcTicks, Is.EqualTo(ticks));
    }

    [Xunit.Fact]
    public void When_Token_IsDateOnlyString_Then_Returns_DateOnly()
    {
        var result = ControlTypeLookup.ConvertFromJToken(new JValue("2024-06-15"), ControlType.DateOnly);

        Assert.That(result, Is.EqualTo(new DateOnly(2024, 6, 15)));
    }
}

