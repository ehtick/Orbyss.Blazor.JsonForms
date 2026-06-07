using Newtonsoft.Json.Linq;
using Orbyss.Blazor.JsonForms.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Core.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Core.Context.Models;
using Orbyss.Blazor.JsonForms.Context.Utils;
using Orbyss.Blazor.JsonForms.Core;
using Orbyss.Blazor.JsonForms.Core.Models;

namespace Orbyss.Blazor.JsonForms.Tests.Context;

/// <summary>
/// Tests covering ArrayLayout operations on <see cref="JsonFormContext"/>:
/// InstantiateArray, AddArrayItem, RemoveArrayItem, MoveArrayItem,
/// GetArrayAddLabel, and the OnArrayItem* event hooks.
/// </summary>
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
        var opts = new JsonFormOptions(ArraySchema, uiSchema ?? ArrayUiSchema, TranslationSchema)
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

    [Xunit.Fact]
    public void When_InstantiateArray_WithNoExistingData_Then_CreatesEmptyArray()
    {
        var (_, array) = BuildWithArray();

        Assert.That(array.Items, Is.Empty);
    }

    [Xunit.Fact]
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

    [Xunit.Fact]
    public void When_AddArrayItem_Then_ItemCountIncreases()
    {
        var (form, array) = BuildWithArray();

        form.AddArrayItem(array.Id);

        Assert.That(array.Items, Has.Length.EqualTo(1));
    }

    [Xunit.Fact]
    public void When_AddArrayItem_Then_NewItemHasCorrectIndex()
    {
        var (form, array) = BuildWithArray();

        form.AddArrayItem(array.Id);
        form.AddArrayItem(array.Id);

        Assert.That(array.Items[0].Index, Is.EqualTo(0));
        Assert.That(array.Items[1].Index, Is.EqualTo(1));
    }

    [Xunit.Fact]
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

    [Xunit.Fact]
    public void When_AddArrayItem_Then_DataContainsNewObject()
    {
        var (form, array) = BuildWithArray();

        form.AddArrayItem(array.Id);

        var data = (JObject)form.GetFormData();
        var addresses = (JArray)data["addresses"]!;
        Assert.That(addresses, Has.Count.EqualTo(1));
    }

    // ── RemoveArrayItem ───────────────────────────────────────────────────────

    [Xunit.Fact]
    public void When_RemoveArrayItem_Then_ItemCountDecreases()
    {
        var (form, array) = BuildWithArray();
        form.AddArrayItem(array.Id);
        form.AddArrayItem(array.Id);
        var idToRemove = array.Items[0].Id;

        form.RemoveArrayItem(array.Id, idToRemove);

        Assert.That(array.Items, Has.Length.EqualTo(1));
    }

    [Xunit.Fact]
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

    [Xunit.Fact]
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

    [Xunit.Fact]
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

    [Xunit.Fact]
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

    [Xunit.Fact]
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

    [Xunit.Fact]
    public void When_GetArrayAddLabel_And_AddLabelIsI18nKey_Then_ReturnsTranslatedLabel()
    {
        var (form, array) = BuildWithArray(uiSchema: ArrayUiSchemaWithAddLabel);

        var label = form.GetArrayAddLabel(array.Id);

        Assert.That(label, Is.EqualTo("Add Address"));
    }

    [Xunit.Fact]
    public void When_GetArrayAddLabel_And_NoAddLabelOption_Then_ReturnsNull()
    {
        var (form, array) = BuildWithArray();

        var label = form.GetArrayAddLabel(array.Id);

        Assert.That(label, Is.Null);
    }

    // ── OnArrayItemAdded event ────────────────────────────────────────────────

    [Xunit.Fact]
    public async Task When_AddArrayItem_And_HandlerRegistered_Then_HandlerIsCalledWithCorrectIndex()
    {
        var opts = new JsonFormOptions(ArraySchema, ArrayUiSchema, TranslationSchema);

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

    [Xunit.Fact]
    public async Task When_RemoveArrayItem_And_HandlerRegistered_Then_HandlerIsCalledWithRemovedIndex()
    {
        var opts = new JsonFormOptions(ArraySchema, ArrayUiSchema, TranslationSchema);

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

    [Xunit.Fact]
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
        var opts = new JsonFormOptions(ArraySchema, ArrayUiSchema, TranslationSchema) { Data = data };

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

    [Xunit.Fact]
    public void When_Validate_And_ArrayBelowMinItems_Then_ValidationFails()
    {
        // Schema requires minItems: 1, start with empty array
        var (form, _) = BuildWithArray();

        var isValid = form.Validate();

        Assert.That(isValid, Is.False);
    }

    [Xunit.Fact]
    public void When_Validate_And_ArrayMeetsMinItems_Then_ValidationPasses()
    {
        var (form, array) = BuildWithArray();
        form.AddArrayItem(array.Id);

        var isValid = form.Validate();

        Assert.That(isValid, Is.True);
    }

    // ── AddArrayItem(data) — dialog-based add ─────────────────────────────────

    [Xunit.Fact]
    public void When_AddArrayItemWithData_Then_ItemDataIsSeeded()
    {
        var (form, array) = BuildWithArray();

        form.AddArrayItem(array.Id, JObject.Parse("""{ "street": "Baker St", "city": "London" }"""));

        var addresses = (JArray)((JObject)form.GetFormData())["addresses"]!;
        Assert.That(addresses, Has.Count.EqualTo(1));
        Assert.That($"{addresses[0]["street"]}", Is.EqualTo("Baker St"));
        Assert.That($"{addresses[0]["city"]}", Is.EqualTo("London"));
        Assert.That(array.Items, Has.Length.EqualTo(1));
    }

    [Xunit.Fact]
    public void When_AddArrayItemWithData_Then_SeedDataIsCloned()
    {
        var (form, array) = BuildWithArray();
        var seed = JObject.Parse("""{ "street": "Baker St", "city": "London" }""");

        form.AddArrayItem(array.Id, seed);
        // Mutating the original seed must not affect the stored item.
        seed["street"] = "Mutated";

        var addresses = (JArray)((JObject)form.GetFormData())["addresses"]!;
        Assert.That($"{addresses[0]["street"]}", Is.EqualTo("Baker St"));
    }

    // ── UpdateArrayItem — dialog-based edit ───────────────────────────────────

    [Xunit.Fact]
    public void When_UpdateArrayItem_Then_ItemDataIsReplaced()
    {
        var data = JObject.Parse("""{ "addresses": [{ "street": "Old", "city": "OldCity" }] }""");
        var (form, array) = BuildWithArray(data: data);
        var itemId = array.Items[0].Id;

        form.UpdateArrayItem(array.Id, itemId, JObject.Parse("""{ "street": "New", "city": "NewCity" }"""));

        var addresses = (JArray)((JObject)form.GetFormData())["addresses"]!;
        Assert.That(addresses, Has.Count.EqualTo(1));
        Assert.That($"{addresses[0]["street"]}", Is.EqualTo("New"));
        Assert.That($"{addresses[0]["city"]}", Is.EqualTo("NewCity"));
    }

    [Xunit.Fact]
    public void When_UpdateArrayItem_Then_OtherItemsAreUnchanged()
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
        var secondId = array.Items[1].Id;

        form.UpdateArrayItem(array.Id, secondId, JObject.Parse("""{ "street": "Changed", "city": "Z" }"""));

        var addresses = (JArray)((JObject)form.GetFormData())["addresses"]!;
        Assert.That($"{addresses[0]["street"]}", Is.EqualTo("First"));
        Assert.That($"{addresses[1]["street"]}", Is.EqualTo("Changed"));
    }

    // ── GetArrayItemData — pre-fill an edit dialog ────────────────────────────

    [Xunit.Fact]
    public void When_GetArrayItemData_Then_ReturnsCurrentItemData()
    {
        var data = JObject.Parse("""{ "addresses": [{ "street": "Main St", "city": "Springfield" }] }""");
        var (form, array) = BuildWithArray(data: data);
        var itemId = array.Items[0].Id;

        var itemData = form.GetArrayItemData(array.Id, itemId);

        Assert.That(itemData, Is.Not.Null);
        Assert.That($"{itemData!["street"]}", Is.EqualTo("Main St"));
        Assert.That($"{itemData["city"]}", Is.EqualTo("Springfield"));
    }

    [Xunit.Fact]
    public void When_CreateArrayItemFormOptions_Then_DefaultTranslationsAreCarriedForward()
    {
        var opts = new JsonFormOptions(ArraySchema, ArrayUiSchema, TranslationSchema)
        {
            Language = "en",
            DefaultTranslations =
            {
                ["en"] = new Dictionary<string, TranslationSection>(StringComparer.OrdinalIgnoreCase)
                {
                    ["orbyss.form.button.add"] = new("Add", Error: null, Enums: null, NestedSections: null)
                }
            }
        };

        var form = JsonFormContextBuilder.BuildAndInstantiate(opts);
        var array = GetArrayContext(form);
        form.InstantiateArray(array.Id);

        var itemOptions = form.CreateArrayItemFormOptions(array.Id);
        var childForm = JsonFormContextBuilder.BuildAndInstantiate(itemOptions);

        Assert.That(childForm.GetTranslatedLabel("orbyss.form.button.add"), Is.EqualTo("Add"));
    }

    // ── OnArrayItemUpdated event ──────────────────────────────────────────────

    [Xunit.Fact]
    public async Task When_UpdateArrayItem_And_HandlerRegistered_Then_HandlerIsCalledWithUpdatedIndex()
    {
        var data = JObject.Parse("""
            {
                "addresses": [
                    { "street": "First",  "city": "A" },
                    { "street": "Second", "city": "B" }
                ]
            }
            """);
        var opts = new JsonFormOptions(ArraySchema, ArrayUiSchema, TranslationSchema) { Data = data };

        int capturedIndex = -1;
        opts.OnArrayItemUpdated += (_, index, _) =>
        {
            capturedIndex = index;
            return Task.CompletedTask;
        };

        var form = JsonFormContextBuilder.BuildAndInstantiate(opts);
        var array = GetArrayContext(form);
        form.InstantiateArray(array.Id);
        var secondId = array.Items[1].Id;

        form.UpdateArrayItem(array.Id, secondId, JObject.Parse("""{ "street": "X", "city": "Y" }"""));
        await Task.Yield();

        Assert.That(capturedIndex, Is.EqualTo(1));
    }
}

