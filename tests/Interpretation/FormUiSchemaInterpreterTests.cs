using Newtonsoft.Json.Schema;
using Orbyss.Blazor.JsonForms.Interpretation;
using Orbyss.Blazor.JsonForms.Core.Interpretation;
using Orbyss.Blazor.JsonForms.Core.UiSchema;
using System.Text.Json.Nodes;

namespace Orbyss.Blazor.JsonForms.Tests.Interpretation;

[TestFixture]
public sealed class FormUiSchemaInterpreterTests
{ 
    [Test]
    public void When_Interpret_Then_Returns_UiSchemaInterpretation()
    {
        // Arrange
        const string schemaJson = "{\"properties\":{\"firstName\":{\"type\":\"string\"}}}";
        var schema = JSchema.Parse(schemaJson);
        var uiSchema = new FormUiSchema(
          UiSchemaElementType.VerticalLayout,
          null,
          null,
          [
              new FormUiSchemaElement(UiSchemaElementType.Control, null, null, [], "#/properties/firstName", null, null)
          ],
          null
        );
        var sut = GetSut();

        // Act
        var result = sut.Interpret(uiSchema, schema);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Pages, Has.Length.EqualTo(1));
        Assert.That(result.Pages[0].InterpretedElements, Has.Length.EqualTo(1));

        var firstVerticalLayoutElement = result.Pages[0].InterpretedElements[0] as UiSchemaVerticalLayoutInterpretation;
        Assert.That(firstVerticalLayoutElement, Is.Not.Null);

        var firstRowControlElement = firstVerticalLayoutElement.Rows[0] as UiSchemaControlInterpretation;
        Assert.That(firstRowControlElement, Is.Not.Null);
        Assert.That(firstRowControlElement.Label?.Label, Is.EqualTo("firstName"));
    }

    [Test]
    public void When_Interpret_And_CategorizationChildElements_NotContainOnlyCategories_Then_ThrowsException()
    {
        // Arrange
        const string schemaJson = "{\"properties\":{\"firstName\":{\"type\":\"string\"}}}";
        var schema = JSchema.Parse(schemaJson);
        var uiSchema = new FormUiSchema(
          UiSchemaElementType.Categorization,
          null,
          null,
          [
              new FormUiSchemaElement(UiSchemaElementType.Control, null, null, [], "#/properties/firstName", null, null)
          ],
          null
        );
        var sut = GetSut();

        // Act & Assert
        var e = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = sut.Interpret(uiSchema, schema);
        });
        Assert.That(e.Message, Is.EqualTo("For a UI Schema of type categorization, all direct child elements must be of type Category"));
    }

    [Test]
    public void When_Interpret_And_HorizontalLayout_DoesNotHave_ChildElements_Then_ThrowsException()
    {
        // Arrange
        const string schemaJson = "{\"properties\":{\"firstName\":{\"type\":\"string\"}}}";
        var schema = JSchema.Parse(schemaJson);
        var uiSchema = new FormUiSchema(
          UiSchemaElementType.HorizontalLayout,
          null,
          null,
          [
          ],
          null
        );
        var sut = GetSut();

        // Act & Assert
        var e = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = sut.Interpret(uiSchema, schema);
        });
        Assert.That(e.Message, Is.EqualTo("Horizontal layout element must have elements defined"));
    }

    [Test]
    public void When_Interpret_And_VerticalLayout_DoesNotHave_ChildElements_Then_ThrowsException()
    {
        // Arrange
        const string schemaJson = "{\"properties\":{\"firstName\":{\"type\":\"string\"}}}";
        var schema = JSchema.Parse(schemaJson);
        var uiSchema = new FormUiSchema(
          UiSchemaElementType.VerticalLayout,
          null,
          null,
          [
          ],
          null
        );
        var sut = GetSut();

        // Act & Assert
        var e = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = sut.Interpret(uiSchema, schema);
        });
        Assert.That(e.Message, Is.EqualTo("Vertical layout element must have elements defined"));
    }

    [Test]
    public void When_Interpret_Then_Sets_Disabled_ReadOnly_And_Hidden_From_Options()
    {
        // Arrange
        const string schemaJson = "{\"properties\":{\"firstName\":{\"type\":\"string\"}}}";
        var schema = JSchema.Parse(schemaJson);
        var uiSchema = new FormUiSchema(
            UiSchemaElementType.VerticalLayout,
            null,
            null,
            [
                new FormUiSchemaElement(
                    UiSchemaElementType.Control,
                    null,
                    null,
                    [],
                    "#/properties/firstName",
                    null,
                    new JsonObject
                    {
                        ["hidden"] = true,
                        ["disabled"] = true,
                        ["readonly"] = true
                    }
                )
            ],
            new JsonObject
            {
                ["hidden"] = true,
                ["disabled"] = true,
                ["readonly"] = true
            }
        );
        var sut = GetSut();

        // Act
        var result = sut.Interpret(uiSchema, schema);

        // Assert
        Assert.That(result, Is.Not.Null);

        var page = result.Pages[0];
        Assert.That(page.Disabled, Is.True);
        Assert.That(page.Hidden, Is.True);
        Assert.That(page.ReadOnly, Is.True);

        var firstVerticalLayoutElement = result.Pages[0].InterpretedElements[0] as UiSchemaVerticalLayoutInterpretation;
        Assert.That(firstVerticalLayoutElement, Is.Not.Null);

        var firstRowControlElement = firstVerticalLayoutElement.Rows[0] as UiSchemaControlInterpretation;
        Assert.That(firstRowControlElement, Is.Not.Null);
        Assert.That(firstRowControlElement.Disabled, Is.True);
        Assert.That(firstRowControlElement.Hidden, Is.True);
        Assert.That(firstRowControlElement.ReadOnly, Is.True);
    }

    // ── Options extraction — GetOption on interpreted models ─────────────────

    [Test]
    public void When_Control_HasOptions_Then_GetOption_Returns_Correct_Value()
    {
        // Arrange
        const string schemaJson = "{\"properties\":{\"premium\":{\"type\":\"number\"}}}";
        var schema = JSchema.Parse(schemaJson);
        var uiSchema = new FormUiSchema(
            UiSchemaElementType.VerticalLayout,
            null, null,
            [
                new FormUiSchemaElement(
                    UiSchemaElementType.Control,
                    null, null, [],
                    "#/properties/premium",
                    null,
                    new JsonObject
                    {
                        ["component"] = "slider",
                        ["step"]      = 100
                    }
                )
            ],
            null
        );
        var sut = GetSut();

        // Act
        var result = sut.Interpret(uiSchema, schema);
        var layout = result.Pages[0].InterpretedElements[0] as UiSchemaVerticalLayoutInterpretation;
        var control = layout!.Rows[0] as UiSchemaControlInterpretation;

        // Assert
        Assert.That(control, Is.Not.Null);
        Assert.That($"{control!.GetOption("component")}", Is.EqualTo("slider"));
        Assert.That($"{control!.GetOption("step")}",      Is.EqualTo("100"));
    }

    [Test]
    public void When_Control_HasNoOptions_Then_GetOption_Returns_Null()
    {
        // Arrange
        const string schemaJson = "{\"properties\":{\"name\":{\"type\":\"string\"}}}";
        var schema = JSchema.Parse(schemaJson);
        var uiSchema = new FormUiSchema(
            UiSchemaElementType.VerticalLayout,
            null, null,
            [
                new FormUiSchemaElement(UiSchemaElementType.Control, null, null, [], "#/properties/name", null, null)
            ],
            null
        );
        var sut = GetSut();

        // Act
        var result = sut.Interpret(uiSchema, schema);
        var layout  = result.Pages[0].InterpretedElements[0] as UiSchemaVerticalLayoutInterpretation;
        var control = layout!.Rows[0] as UiSchemaControlInterpretation;

        // Assert
        Assert.That(control, Is.Not.Null);
        Assert.That(control!.GetOption("component"), Is.Null);
        Assert.That(control!.GetOption("step"),      Is.Null);
    }

    [Test]
    public void When_Control_HasJObjectOptions_Then_GetOption_Works()
    {
        // Arrange — use a Newtonsoft JObject directly rather than System.Text.Json JsonObject
        const string schemaJson = "{\"properties\":{\"amount\":{\"type\":\"number\"}}}";
        var schema = JSchema.Parse(schemaJson);
        var options = new Newtonsoft.Json.Linq.JObject
        {
            ["placeholder"] = "Enter amount",
            ["min"]         = 0
        };
        var uiSchema = new FormUiSchema(
            UiSchemaElementType.VerticalLayout,
            null, null,
            [
                new FormUiSchemaElement(UiSchemaElementType.Control, null, null, [], "#/properties/amount", null, options)
            ],
            null
        );
        var sut = GetSut();

        // Act
        var result  = sut.Interpret(uiSchema, schema);
        var layout  = result.Pages[0].InterpretedElements[0] as UiSchemaVerticalLayoutInterpretation;
        var control = layout!.Rows[0] as UiSchemaControlInterpretation;

        // Assert
        Assert.That(control, Is.Not.Null);
        Assert.That($"{control!.GetOption("placeholder")}", Is.EqualTo("Enter amount"));
        Assert.That($"{control!.GetOption("min")}",         Is.EqualTo("0"));
    }

    [Test]
    public void When_Interpret_Then_UiSchemaInterpretation_HasOnlyPages()
    {
        // Verifies the new simplified UiSchemaInterpretation (no FormUiSchema reference)
        const string schemaJson = "{\"properties\":{\"x\":{\"type\":\"string\"}}}";
        var schema = JSchema.Parse(schemaJson);
        var uiSchema = new FormUiSchema(
            UiSchemaElementType.VerticalLayout,
            null, null,
            [new FormUiSchemaElement(UiSchemaElementType.Control, null, null, [], "#/properties/x", null, null)],
            null
        );
        var sut = GetSut();

        var result = sut.Interpret(uiSchema, schema);

        Assert.That(result.Pages, Has.Length.EqualTo(1));
        // The type only exposes Pages — no FormUiSchema property exists
        var type = result.GetType();
        var formUiSchemaProp = type.GetProperty("FormUiSchema");
        Assert.That(formUiSchemaProp, Is.Null,
            "UiSchemaInterpretation must not expose a FormUiSchema property");
    }

    [Test]
    public void When_ActionButton_HasOptions_Then_GetOption_Returns_Correct_Value()
    {
        // Arrange
        const string schemaJson = "{}";
        var schema = JSchema.Parse(schemaJson);
        var uiSchema = new FormUiSchema(
            UiSchemaElementType.VerticalLayout,
            null, null,
            [
                new FormUiSchemaElement(
                    UiSchemaElementType.ActionButton,
                    "Save",
                    null, [],
                    null,
                    null,
                    new JsonObject
                    {
                        ["actionKey"] = "save-form",
                        ["variant"]   = "primary"
                    }
                )
            ],
            null
        );
        var sut = GetSut();

        // Act
        var result  = sut.Interpret(uiSchema, schema);
        var layout  = result.Pages[0].InterpretedElements[0] as UiSchemaVerticalLayoutInterpretation;
        var button  = layout!.Rows[0] as UiSchemaActionButtonInterpretation;

        // Assert
        Assert.That(button, Is.Not.Null);
        Assert.That($"{button!.GetOption("variant")}", Is.EqualTo("primary"));
    }

    private static FormUiSchemaInterpreter GetSut()
    {
        return new FormUiSchemaInterpreter(
            new JsonPathInterpreter(),
            new ControlTypeInterpreter()
        );
    }
}