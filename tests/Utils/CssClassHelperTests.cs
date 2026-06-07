using Orbyss.Blazor.JsonForms.Core.Utils;

namespace Orbyss.Blazor.JsonForms.Tests.Utils;

public sealed class CssClassHelperTests
{
    // ── append behaviour ─────────────────────────────────────────────────────

    [Xunit.Fact]
    public void When_OptionClass_IsNull_Then_Returns_DefaultClass()
    {
        var result = CssClassHelper.Merge("orbyss-form-text-input", null);
        Assert.That(result, Is.EqualTo("orbyss-form-text-input"));
    }

    [Xunit.Fact]
    public void When_OptionClass_IsWhitespace_Then_Returns_DefaultClass()
    {
        var result = CssClassHelper.Merge("orbyss-form-text-input", "   ");
        Assert.That(result, Is.EqualTo("orbyss-form-text-input"));
    }

    [Xunit.Fact]
    public void When_OptionClass_IsProvided_Then_Appended_To_DefaultClass()
    {
        var result = CssClassHelper.Merge("orbyss-form-text-input", "highlighted");
        Assert.That(result, Is.EqualTo("orbyss-form-text-input highlighted"));
    }

    [Xunit.Fact]
    public void When_DefaultClass_IsNull_And_OptionClass_IsProvided_Then_Returns_OptionClass()
    {
        var result = CssClassHelper.Merge(null, "highlighted");
        Assert.That(result, Is.EqualTo("highlighted"));
    }

    [Xunit.Fact]
    public void When_BothAreNull_Then_Returns_Null()
    {
        var result = CssClassHelper.Merge(null, null);
        Assert.That(result, Is.Null);
    }

    // ── replace behaviour (! prefix) ─────────────────────────────────────────

    [Xunit.Fact]
    public void When_OptionClass_StartsWith_Bang_Then_Replaces_DefaultClass()
    {
        var result = CssClassHelper.Merge("orbyss-form-text-input", "!my-custom-input");
        Assert.That(result, Is.EqualTo("my-custom-input"));
    }

    [Xunit.Fact]
    public void When_OptionClass_IsBangOnly_Then_Returns_Null()
    {
        var result = CssClassHelper.Merge("orbyss-form-text-input", "!");
        Assert.That(result, Is.Null);
    }

    [Xunit.Fact]
    public void When_OptionClass_IsBangWithWhitespace_Then_Returns_Null()
    {
        var result = CssClassHelper.Merge("orbyss-form-text-input", "!   ");
        Assert.That(result, Is.Null);
    }

    [Xunit.Fact]
    public void When_OptionClass_IsBang_And_DefaultIsNull_Then_Replaces_With_Given_Class()
    {
        var result = CssClassHelper.Merge(null, "!my-class");
        Assert.That(result, Is.EqualTo("my-class"));
    }

    [Xunit.Fact]
    public void When_OptionClass_HasLeadingWhitespace_After_Bang_Then_Trims_Result()
    {
        var result = CssClassHelper.Merge("default", "!  trimmed-class  ");
        Assert.That(result, Is.EqualTo("trimmed-class"));
    }
}

