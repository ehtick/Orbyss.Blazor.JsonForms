using Newtonsoft.Json.Schema;
using Orbyss.Blazor.JsonForms.Interpretation;
using Orbyss.Blazor.JsonForms.UiSchema;
using System.Text.Json.Nodes;

namespace Orbyss.Blazor.JsonForms.Tests.Interpretation;

[TestFixture]
public sealed class FormUiSchemaInterpreterArrayTests
{
    // JSON Schema for an array of address objects with street + city
    private const string ArraySchemaJson = """
        {
            "properties": {
                "addresses": {
                    "type": "array",
                    "minItems": 1,
                    "maxItems": 5,
                    "items": {
                        "type": "object",
                        "properties": {
                            "street": { "type": "string" },
                            "city":   { "type": "string" }
                        }
                    }
                }
            }
        }
        """;

    [Test]
    public void When_InterpretArrayLayout_Then_ReturnsArrayLayoutInterpretation()
    {
        // Arrange
        var schema = JSchema.Parse(ArraySchemaJson);
        var uiSchema = new FormUiSchema(
            UiSchemaElementType.VerticalLayout,
            null, null,
            [
                new FormUiSchemaElement(
                    UiSchemaElementType.ArrayLayout,
                    null, null, [],
                    "#/properties/addresses",
                    null, null)
            ],
            null
        );

        // Act
        var result = GetSut().Interpret(uiSchema, schema);

        // Assert
        var vertical = (UiSchemaVerticalLayoutInterpretation)result.Pages[0].InterpretedElements[0];
        var array = vertical.Rows[0] as UiSchemaArrayLayoutInterpretation;

        Assert.That(array, Is.Not.Null);
        Assert.That(array!.ElementType, Is.EqualTo(UiSchemaElementInterpretationType.ArrayLayout));
        // FromElementScope("#/properties/addresses") → "$.properties.addresses" (raw schema path, not data path)
        Assert.That(array.RelativeSchemaJsonPath, Is.EqualTo("$.properties.addresses"));
        Assert.That(array.AbsoluteSchemaJsonPath, Is.EqualTo("$.properties.addresses"));
        Assert.That(array.RelativeItemsSchemaJsonPath, Is.EqualTo("$.properties.addresses.items"));
        Assert.That(array.AbsoluteItemsSchemaJsonPath, Is.EqualTo("$.properties.addresses.items"));
    }

    [Test]
    public void When_InterpretArrayLayout_WithoutItemsUiElement_Then_AutoGeneratesHorizontalLayoutFromSchema()
    {
        // Arrange — no explicit "items" UI element; interpreter should generate one from JSON Schema
        var schema = JSchema.Parse(ArraySchemaJson);
        var uiSchema = new FormUiSchema(
            UiSchemaElementType.VerticalLayout,
            null, null,
            [
                new FormUiSchemaElement(
                    UiSchemaElementType.ArrayLayout,
                    null, null, [],
                    "#/properties/addresses",
                    null, null)
            ],
            null
        );

        // Act
        var result = GetSut().Interpret(uiSchema, schema);

        var vertical = (UiSchemaVerticalLayoutInterpretation)result.Pages[0].InterpretedElements[0];
        var array = (UiSchemaArrayLayoutInterpretation)vertical.Rows[0];

        // Items interpretation should be a HorizontalLayout with controls for street + city
        var horizontal = array.ItemsInterpretation as UiSchemaHorizontalLayoutInterpretation;
        Assert.That(horizontal, Is.Not.Null);
        Assert.That(horizontal!.Columns, Has.Length.EqualTo(2));

        var controlPaths = horizontal.Columns
            .OfType<UiSchemaControlInterpretation>()
            .Select(c => c.JsonPropertyName)
            .ToArray();

        Assert.That(controlPaths, Does.Contain("street"));
        Assert.That(controlPaths, Does.Contain("city"));
    }

    [Test]
    public void When_InterpretArrayLayout_WithExplicitItemsUiElement_Then_UsesProvidedLayout()
    {
        // Arrange — explicit items element with only street (not city)
        var schema = JSchema.Parse(ArraySchemaJson);
        var explicitItems = new FormUiSchemaElement(
            UiSchemaElementType.HorizontalLayout,
            null, null,
            [
                new FormUiSchemaElement(
                    UiSchemaElementType.Control,
                    null, null, [],
                    "#/properties/street",
                    null, null)
            ],
            null, null, null);

        var arrayElement = new FormUiSchemaElement(
            UiSchemaElementType.ArrayLayout,
            null, null, [],
            "#/properties/addresses",
            null, null)
            with { Items = explicitItems };

        var uiSchema = new FormUiSchema(
            UiSchemaElementType.VerticalLayout,
            null, null, [arrayElement], null);

        // Act
        var result = GetSut().Interpret(uiSchema, schema);

        var vertical = (UiSchemaVerticalLayoutInterpretation)result.Pages[0].InterpretedElements[0];
        var array = (UiSchemaArrayLayoutInterpretation)vertical.Rows[0];

        var horizontal = array.ItemsInterpretation as UiSchemaHorizontalLayoutInterpretation;
        Assert.That(horizontal, Is.Not.Null);
        Assert.That(horizontal!.Columns, Has.Length.EqualTo(1));

        var col = horizontal.Columns[0] as UiSchemaControlInterpretation;
        Assert.That(col!.JsonPropertyName, Is.EqualTo("street"));
    }

    [Test]
    public void When_InterpretArrayLayout_WithAddLabelOption_Then_ParsesAddLabel()
    {
        // Arrange
        var schema = JSchema.Parse(ArraySchemaJson);
        var uiSchema = new FormUiSchema(
            UiSchemaElementType.VerticalLayout,
            null, null,
            [
                new FormUiSchemaElement(
                    UiSchemaElementType.ArrayLayout,
                    null, null, [],
                    "#/properties/addresses",
                    null,
                    new JsonObject { ["addLabel"] = "addAddress" })
            ],
            null
        );

        // Act
        var result = GetSut().Interpret(uiSchema, schema);

        var vertical = (UiSchemaVerticalLayoutInterpretation)result.Pages[0].InterpretedElements[0];
        var array = (UiSchemaArrayLayoutInterpretation)vertical.Rows[0];

        Assert.That(array.AddLabel, Is.EqualTo("addAddress"));
    }

    [Test]
    public void When_InterpretArrayLayout_WithoutScope_Then_ThrowsException()
    {
        // Arrange — ArrayLayout with no scope
        var schema = JSchema.Parse(ArraySchemaJson);
        var uiSchema = new FormUiSchema(
            UiSchemaElementType.VerticalLayout,
            null, null,
            [
                new FormUiSchemaElement(
                    UiSchemaElementType.ArrayLayout,
                    null, null, [],
                    null,
                    null, null)
            ],
            null
        );

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => GetSut().Interpret(uiSchema, schema));
    }

    private static FormUiSchemaInterpreter GetSut() =>
        new(new JsonPathInterpreter(), new ControlTypeInterpreter());
}
