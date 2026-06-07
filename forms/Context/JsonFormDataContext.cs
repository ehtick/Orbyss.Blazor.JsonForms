using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Orbyss.Blazor.JsonForms.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Core.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Core.Context.Models;
using Orbyss.Blazor.JsonForms.Core.Interpretation.Interfaces;
using Orbyss.Blazor.JsonForms.Interpretation.Interfaces;

namespace Orbyss.Blazor.JsonForms.Context;

public sealed class JsonFormDataContext(
    IJsonTransformer jsonTransformer,
    IFormElementContextFactory elementContextFactory,
    IJsonPathInterpreter jsonPathInterpreter)

    : IJsonFormDataContext
{
    private JToken? data;
    private JToken? dataSchema;

    public void Instantiate(JToken formData, JSchema dataSchema)
    {
        if (data is not null)
            throw new InvalidOperationException("Data context is already instantiated");

        data = JToken.Parse($"{formData}");
        this.dataSchema = JToken.Parse($"{dataSchema}");
    }

    public JSchema GetJsonSchema()
    {
        return JSchema.Parse($"{dataSchema}");
    }

    public bool Validate(IEnumerable<IFormElementContext> contexts)
    {
        var formData = JToken.Parse($"{data}");
        var schema = JToken.Parse($"{dataSchema}");

        var isvalid = true;

        foreach (var context in contexts)
        {
            if (!context.Validate(formData, schema))
            {
                isvalid = false;
            }
        }

        return isvalid;
    }

    public JToken? GetValue(FormControlContext formControlContext)
    {
        return GetFormData().SelectToken(formControlContext.AbsoluteDataJsonPath, false);
    }

    public void UpdateValue(FormControlContext formControlContext, JToken? value)
    {
        jsonTransformer.PutValue(formControlContext.AbsoluteDataJsonPath, GetFormData(), value ?? JValue.CreateNull());
    }

    public JToken GetFormData()
    {
        if (data is null)
            throw new InvalidOperationException("Form data is null");

        return data;
    }

    public void AddListItem(FormListContext listContext)
    {
        var listElementInterpretation = listContext.Interpretation;
        var newItemIndex = listContext.Items.Length;
        var newItemAbsolutePath = jsonPathInterpreter.AddIndexToPath(listContext.AbsoluteDataJsonPath, newItemIndex);
        var detail = elementContextFactory.Create(listElementInterpretation.GetListDetail(), newItemAbsolutePath);
        listContext.AddItem(detail);

        var itemExists = GetFormData().SelectToken(newItemAbsolutePath, false) is not null;
        if (!itemExists)
        {
            jsonTransformer.AddValue(listContext.AbsoluteDataJsonPath, GetFormData(), new JObject());
        }
    }

    public void RemoveListItem(FormListContext listContext, IFormElementContext listItemContext)
    {
        var removedItemIndex = listContext.RemoveItem(listItemContext.Id);
        var removedItemAbsolutePath = jsonPathInterpreter.AddIndexToPath(listContext.AbsoluteDataJsonPath, removedItemIndex);

        jsonTransformer.RemoveValue(removedItemAbsolutePath, GetFormData());
    }

    // ── Array (inline repeater) ───────────────────────────────────────────────

    public void InstantiateArray(FormArrayContext arrayContext)
    {
        var formData = GetFormData();
        var arrayData = formData.SelectToken(arrayContext.AbsoluteDataJsonPath, false);

        if (arrayData is null)
        {
            jsonTransformer.AddValue(arrayContext.AbsoluteDataJsonPath, formData, new JArray());
            arrayData = formData.SelectToken(arrayContext.AbsoluteDataJsonPath, true);
        }
        else if (arrayData is not JArray)
        {
            throw new InvalidOperationException(
                $"Expected a JSON array at path '{arrayContext.AbsoluteDataJsonPath}', but found '{arrayData.GetType().Name}'.");
        }

        var existingArray = (JArray)arrayData!;
        while (arrayContext.Items.Length < existingArray.Count)
        {
            AddArrayItemInternal(arrayContext);
        }
    }

    public void AddArrayItem(FormArrayContext arrayContext)
    {
        var newItemPath = jsonPathInterpreter.AddIndexToPath(
            arrayContext.AbsoluteDataJsonPath,
            arrayContext.Items.Length);

        if (GetFormData().SelectToken(newItemPath, false) is null)
            jsonTransformer.AddValue(arrayContext.AbsoluteDataJsonPath, GetFormData(), new JObject());

        AddArrayItemInternal(arrayContext);
    }

    public void AddArrayItem(FormArrayContext arrayContext, JToken itemData)
    {
        jsonTransformer.AddValue(
            arrayContext.AbsoluteDataJsonPath,
            GetFormData(),
            itemData?.DeepClone() ?? new JObject());

        AddArrayItemInternal(arrayContext);
    }

    public void UpdateArrayItem(FormArrayContext arrayContext, Guid itemId, JToken itemData)
    {
        var item = arrayContext.Items.FirstOrDefault(x => x.Id == itemId)
            ?? throw new InvalidOperationException($"Array does not contain an item with id '{itemId}'.");

        var array = (JArray)(GetFormData().SelectToken(arrayContext.AbsoluteDataJsonPath, true)
            ?? throw new InvalidOperationException($"Expected a JSON array at '{arrayContext.AbsoluteDataJsonPath}'."));

        array[item.Index] = itemData?.DeepClone() ?? new JObject();

        // Rebuild item contexts so any structural change in the replaced item is reflected.
        RebuildArrayItems(arrayContext);
    }

    public JToken? GetArrayItemData(FormArrayContext arrayContext, Guid itemId)
    {
        var item = arrayContext.Items.FirstOrDefault(x => x.Id == itemId);
        if (item is null) return null;

        var itemPath = jsonPathInterpreter.AddIndexToPath(arrayContext.AbsoluteDataJsonPath, item.Index);
        return GetFormData().SelectToken(itemPath, false);
    }

    public void RemoveArrayItem(FormArrayContext arrayContext, Guid itemId)
    {
        var removedIndex = arrayContext.RemoveItemById(itemId);
        var removedPath  = jsonPathInterpreter.AddIndexToPath(arrayContext.AbsoluteDataJsonPath, removedIndex);
        jsonTransformer.RemoveValue(removedPath, GetFormData());

        // Rebuild all remaining item contexts so their data paths match the new indices.
        RebuildArrayItems(arrayContext);
    }

    public void MoveArrayItem(FormArrayContext arrayContext, int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex) return;

        var formData = GetFormData();
        var array    = (JArray)(formData.SelectToken(arrayContext.AbsoluteDataJsonPath, true)
            ?? throw new InvalidOperationException($"Expected a JSON array at '{arrayContext.AbsoluteDataJsonPath}'."));

        var item = array[fromIndex];
        array.RemoveAt(fromIndex);
        array.Insert(toIndex, item);

        // Rebuild all item contexts so their data paths match the new order.
        arrayContext.ClearItems();
        RebuildArrayItems(arrayContext);
    }

    private void AddArrayItemInternal(FormArrayContext arrayContext)
    {
        var index    = arrayContext.Items.Length;
        var itemPath = jsonPathInterpreter.AddIndexToPath(arrayContext.AbsoluteDataJsonPath, index);
        var elementContext = elementContextFactory.Create(
            arrayContext.Interpretation.ItemsInterpretation, itemPath);
        arrayContext.AddItem(new FormArrayItemContext(index, elementContext));
    }

    private void RebuildArrayItems(FormArrayContext arrayContext)
    {
        arrayContext.ClearItems();
        var formData  = GetFormData();
        var arrayData = formData.SelectToken(arrayContext.AbsoluteDataJsonPath, false) as JArray;
        if (arrayData is null) return;

        for (var i = 0; i < arrayData.Count; i++)
        {
            var itemPath       = jsonPathInterpreter.AddIndexToPath(arrayContext.AbsoluteDataJsonPath, i);
            var elementContext = elementContextFactory.Create(
                arrayContext.Interpretation.ItemsInterpretation, itemPath);
            arrayContext.AddItem(new FormArrayItemContext(i, elementContext));
        }
    }

    public void InstantiateList(FormListContext listContext)
    {
        var formData = GetFormData();
        var listData = formData.SelectToken(listContext.AbsoluteDataJsonPath, false);
        if (listData is null)
        {
            jsonTransformer.AddValue(listContext.AbsoluteDataJsonPath, formData, new JArray());
            listData = formData.SelectToken(listContext.AbsoluteDataJsonPath, true);
        }
        else if (listData is not JArray)
        {
            throw new InvalidOperationException($"Expected a JSON array at path '{listContext.AbsoluteDataJsonPath}', but instead found a '{listData.GetType().Name}'");
        }

        var list = (JArray)listData!;
        while (listContext.Items.Length < list.Count)
        {
            AddListItem(listContext);
        }

        if (listContext.Items.Length != list.Count)
        {
            throw new InvalidOperationException("List context contains more items than in the data while being instantiated");
        }
    }
}