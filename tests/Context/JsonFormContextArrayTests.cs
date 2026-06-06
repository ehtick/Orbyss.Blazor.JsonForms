using Newtonsoft.Json.Linq;
using Orbyss.Blazor.JsonForms.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Context.Models;
using Orbyss.Blazor.JsonForms.Context.Utils;

namespace Orbyss.Blazor.JsonForms.Tests.Context;

/// <summary>
/// Tests covering ArrayLayout operations on <see cref="JsonFormContext"/>:
/// InstantiateArray, AddArrayItem, RemoveArrayItem, MoveArrayItem,
/// GetArrayAddLabel, and the OnArrayItem* event hooks.
/// </summary>
[TestFixture]
public sealed class JsonFormContextArrayTests
{
    // JSON Schema: object with an "addresses" array (street + city per item)
    private const string ArraySchema = """
        {
            "properties": {
                "addresses": {
                    "type": "array",
                    "minItems": 1,
                    "maxItems": 3,
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

    private const string TranslationSchema = """
        {
            "resources": {
                "en": {
                    "translation": {
                        "addAddress": { "label": "Add Address" }
                    }
                }
            }
        }
        """;

    // UI schema: ArrayLayout with auto-generated item layout
    private const string ArrayUiSchema = """
        {
            "type": "VerticalLayout",
            "elements": [
                {
                    "type": "ArrayLayout",
                    "scope": "#/properties/addresses"
                }
            ]
        }
        """;

    // UI schema with addLabel option
    private const string ArrayUiSchemaWithAddLabel = """
        {
            "type": "VerticalLayout",
            "elements": [
                {
                    "type": "ArrayLayout",
                    "scope": "#/properties/addresses",
                    "options": { "addLabel": "addAddress" }
                }
            ]
        }
        """;

    // ── helpers ───────────────────────────────────────────────────────────────

    private static (IJsonFormContext form, FormArrayContext array) BuildWithArray(
        string? uiSchema = null,
        JObject? data = null)
    {
        var opts = new JsonFormContextOptions(ArraySchema, uiSchema ?? ArrayUiSchema, TranslationSchema)
        {
            Data = data,
            Language = "en"
        };
        var form = JsonFormContextBuilder.BuildAndInstantiate(opts);
        var array = GetArrayContext(form);
        // Mirrors what FormArrayLayout.razor does in OnInitialized
        form.InstantiateArray(array.Id);
        return (form, array);
    }

    private static FormArrayContext GetArrayContext(IJsonFormContext form)
    {
        var page = form.GetPage(0);
        var vertical = (FormVerticalLayoutContext)page.ElementContexts[0];
        return (FormArrayContext)vertical.Rows.First();
    }

    // ── InstantiateArray ──────────────────────────────────────────────────────

    [Test]
    public void When_InstantiateArray_WithNoExistingData_Then_CreatesEmptyArray()
    {
        var (_, array) = BuildWithArray();

        Assert.That(array.Items, Is.Empty);
    }

    [Test]
    public void When_InstantiateArray_WithExistingItems_Then_LoadsItemContexts()
    {
        var data = JObject.Parse("""
            {
                "addresses": [
                    { "street": "Main St", "city": "Springfield" },
                    { "street": "Elm St",  "city": "Shelbyville" }
                ]
            }
            """);

        var (_, array) = BuildWithArray(data: data);

        Assert.That(array.Items, Has.Length.EqualTo(2));
        Assert.That(array.Items[0].Index, Is.EqualTo(0));
        Assert.That(array.Items[1].Index, Is.EqualTo(1));
    }

    // ── AddArrayItem ──────────────────────────────────────────────────────────

    [Test]
    public void When_AddArrayItem_Then_ItemCountIncreases()
    {
        var (form, array) = BuildWithArray();

        form.AddArrayItem(array.Id);

        Assert.That(array.Items, Has.Length.EqualTo(1));
    }

    [Test]
    public void When_AddArrayItem_Then_NewItemHasCorrectIndex()
    {
        var (form, array) = BuildWithArray();

        form.AddArrayItem(array.Id);
        form.AddArrayItem(array.Id);

        Assert.That(array.Items[0].Index, Is.EqualTo(0));
        Assert.That(array.Items[1].Index, Is.EqualTo(1));
    }

    [Test]
    public void When_AddArrayItem_Then_ItemContextHasCorrectDataPath()
    {
        var (form, array) = BuildWithArray();

        form.AddArrayItem(array.Id);

        // The item element context should be a HorizontalLayout whose child controls
        // are bound to $.addresses[0].street and $.addresses[0].city
        var horizontal = (FormHorizontalLayoutContext)array.Items[0].ElementContext;
        var paths = horizontal.Columns
            .OfType<FormControlContext>()
            .Select(c => c.AbsoluteDataJsonPath)
            .ToArray();

        Assert.That(paths, Does.Contain("$.addresses[0].street"));
        Assert.That(paths, Does.Contain("$.addresses[0].city"));
    }

    [Test]
    public void When_AddArrayItem_Then_DataContainsNewObject()
    {
        var (form, array) = BuildWithArray();

        form.AddArrayItem(array.Id);

        var data = (JObject)form.GetFormData();
        var addresses = (JArray)data["addresses"]!;
        Assert.That(addresses, Has.Count.EqualTo(1));
    }

    // ── RemoveArrayItem ───────────────────────────────────────────────────────

    [Test]
    public void When_RemoveArrayItem_Then_ItemCountDecreases()
    {
        var (form, array) = BuildWithArray();
        form.AddArrayItem(array.Id);
        form.AddArrayItem(array.Id);
        var idToRemove = array.Items[0].Id;

        form.RemoveArrayItem(array.Id, idToRemove);

        Assert.That(array.Items, Has.Length.EqualTo(1));
    }

    [Test]
    public void When_RemoveArrayItem_Then_RemainingItemIndicesAreRebased()
    {
        var (form, array) = BuildWithArray();
        form.AddArrayItem(array.Id);
        form.AddArrayItem(array.Id);
        form.AddArrayItem(array.Id);
        var idToRemove = array.Items[0].Id; // remove first

        form.RemoveArrayItem(array.Id, idToRemove);

        // After removal the two remaining items should be at index 0 and 1
        Assert.That(array.Items[0].Index, Is.EqualTo(0));
        Assert.That(array.Items[1].Index, Is.EqualTo(1));
    }

    [Test]
    public void When_RemoveArrayItem_Then_DataArrayIsUpdated()
    {
        var data = JObject.Parse("""
            {
                "addresses": [
                    { "street": "Main St", "city": "Springfield" },
                    { "street": "Elm St",  "city": "Shelbyville" }
                ]
            }
            """);
        var (form, array) = BuildWithArray(data: data);
        var idToRemove = array.Items[0].Id;

        form.RemoveArrayItem(array.Id, idToRemove);

        var addresses = (JArray)((JObject)form.GetFormData())["addresses"]!;
        Assert.That(addresses, Has.Count.EqualTo(1));
        Assert.That($"{addresses[0]["street"]}", Is.EqualTo("Elm St"));
    }

    // ── MoveArrayItem ─────────────────────────────────────────────────────────

    [Test]
    public void When_MoveArrayItem_Then_DataOrderChanges()
    {
        var data = JObject.Parse("""
            {
                "addresses": [
                    { "street": "First",  "city": "A" },
                    { "street": "Second", "city": "B" },
                    { "street": "Third",  "city": "C" }
                ]
            }
            """);
        var (form, array) = BuildWithArray(data: data);

        // Move index 0 to index 2
        form.MoveArrayItem(array.Id, fromIndex: 0, toIndex: 2);

        var addresses = (JArray)((JObject)form.GetFormData())["addresses"]!;
        Assert.That($"{addresses[0]["street"]}", Is.EqualTo("Second"));
        Assert.That($"{addresses[1]["street"]}", Is.EqualTo("Third"));
        Assert.That($"{addresses[2]["street"]}", Is.EqualTo("First"));
    }

    [Test]
    public void When_MoveArrayItem_Then_ContextIndicesAreRebased()
    {
        var data = JObject.Parse("""
            {
                "addresses": [
                    { "street": "First",  "city": "A" },
                    { "street": "Second", "city": "B" }
                ]
            }
            """);
        var (form, array) = BuildWithArray(data: data);

        form.MoveArrayItem(array.Id, fromIndex: 0, toIndex: 1);

        Assert.That(array.Items[0].Index, Is.EqualTo(0));
        Assert.That(array.Items[1].Index, Is.EqualTo(1));
    }

    [Test]
    public void When_MoveArrayItem_SameIndex_Then_NothingChanges()
    {
        var data = JObject.Parse("""
            { "addresses": [{ "street": "Main St", "city": "Springfield" }] }
            """);
        var (form, array) = BuildWithArray(data: data);

        Assert.DoesNotThrow(() => form.MoveArrayItem(array.Id, 0, 0));
        Assert.That(array.Items, Has.Length.EqualTo(1));
    }

    // ── GetArrayAddLabel ──────────────────────────────────────────────────────

    [Test]
    public void When_GetArrayAddLabel_And_AddLabelIsI18nKey_Then_ReturnsTranslatedLabel()
    {
        var (form, array) = BuildWithArray(uiSchema: ArrayUiSchemaWithAddLabel);

        var label = form.GetArrayAddLabel(array.Id);

        Assert.That(label, Is.EqualTo("Add Address"));
    }

    [Test]
    public void When_GetArrayAddLabel_And_NoAddLabelOption_Then_ReturnsNull()
    {
        var (form, array) = BuildWithArray();

        var label = form.GetArrayAddLabel(array.Id);

        Assert.That(label, Is.Null);
    }

    // ── OnArrayItemAdded event ────────────────────────────────────────────────

    [Test]
    public async Task When_AddArrayItem_And_HandlerRegistered_Then_HandlerIsCalledWithCorrectIndex()
    {
        var opts = new JsonFormContextOptions(ArraySchema, ArrayUiSchema, TranslationSchema);

        FormArrayContext? capturedArray = null;
        int capturedIndex = -1;
        opts.OnArrayItemAdded += (array, index, _) =>
        {
            capturedArray = array;
            capturedIndex = index;
            return Task.CompletedTask;
        };

        var form = JsonFormContextBuilder.BuildAndInstantiate(opts);
        var array = GetArrayContext(form);

        form.AddArrayItem(array.Id);
        form.AddArrayItem(array.Id); // second add should report index 1

        // The event fires synchronously via fire-and-forget; await a completed task to flush
        await Task.Yield();

        Assert.That(capturedArray, Is.Not.Null);
        Assert.That(capturedIndex, Is.EqualTo(1));
    }

    // ── OnArrayItemRemoved event ──────────────────────────────────────────────

    [Test]
    public async Task When_RemoveArrayItem_And_HandlerRegistered_Then_HandlerIsCalledWithRemovedIndex()
    {
        var opts = new JsonFormContextOptions(ArraySchema, ArrayUiSchema, TranslationSchema);

        int capturedIndex = -1;
        opts.OnArrayItemRemoved += (_, index, _) =>
        {
            capturedIndex = index;
            return Task.CompletedTask;
        };

        var form = JsonFormContextBuilder.BuildAndInstantiate(opts);
        var array = GetArrayContext(form);

        form.AddArrayItem(array.Id);
        form.AddArrayItem(array.Id);
        var idToRemove = array.Items[0].Id; // remove first → index 0

        form.RemoveArrayItem(array.Id, idToRemove);
        await Task.Yield();

        Assert.That(capturedIndex, Is.EqualTo(0));
    }

    // ── OnArrayItemMoved event ────────────────────────────────────────────────

    [Test]
    public async Task When_MoveArrayItem_And_HandlerRegistered_Then_HandlerIsCalledWithFromAndToIndex()
    {
        var data = JObject.Parse("""
            {
                "addresses": [
                    { "street": "First",  "city": "A" },
                    { "street": "Second", "city": "B" }
                ]
            }
            """);
        var opts = new JsonFormContextOptions(ArraySchema, ArrayUiSchema, TranslationSchema) { Data = data };

        int capturedFrom = -1;
        int capturedTo = -1;
        opts.OnArrayItemMoved += (_, from, to, _) =>
        {
            capturedFrom = from;
            capturedTo = to;
            return Task.CompletedTask;
        };

        var form = JsonFormContextBuilder.BuildAndInstantiate(opts);
        var array = GetArrayContext(form);

        form.MoveArrayItem(array.Id, fromIndex: 0, toIndex: 1);
        await Task.Yield();

        Assert.That(capturedFrom, Is.EqualTo(0));
        Assert.That(capturedTo, Is.EqualTo(1));
    }

    // ── Validation (minItems / maxItems) ──────────────────────────────────────

    [Test]
    public void When_Validate_And_ArrayBelowMinItems_Then_ValidationFails()
    {
        // Schema requires minItems: 1, start with empty array
        var (form, _) = BuildWithArray();

        var isValid = form.Validate();

        Assert.That(isValid, Is.False);
    }

    [Test]
    public void When_Validate_And_ArrayMeetsMinItems_Then_ValidationPasses()
    {
        var (form, array) = BuildWithArray();
        form.AddArrayItem(array.Id);

        var isValid = form.Validate();

        Assert.That(isValid, Is.True);
    }
}
