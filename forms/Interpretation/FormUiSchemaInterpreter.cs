using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Orbyss.Blazor.JsonForms.Constants;
using Orbyss.Blazor.JsonForms.Interpretation.Interfaces;
using Orbyss.Blazor.JsonForms.UiSchema;
using Orbyss.Blazor.JsonForms.Utils;

namespace Orbyss.Blazor.JsonForms.Interpretation;

public sealed class FormUiSchemaInterpreter(IJsonPathInterpreter jsonPathInterpreter, IControlTypeInterpreter controlTypeInterpreter) : IFormUiSchemaInterpreter
{
    public UiSchemaInterpretation Interpret(FormUiSchema uiSchema, JSchema jsonSchema)
    {
        if (uiSchema.Type == UiSchemaElementType.Categorization)
        {
            return new UiSchemaInterpretation(
                pages: InterpretCategories(uiSchema, jsonSchema)
            );
        }

        var rootFormUiSchemaElement = new FormUiSchemaElement(
            uiSchema.Type,
            uiSchema.Label,
            I18n: null,
            uiSchema.Elements,
            uiSchema.Scope,
            Rule: null,
            uiSchema.Options
        );

        var rootElement = Interpret(rootFormUiSchemaElement, jsonSchema, null);
        var rootPage = new UiSchemaPageInterpretation(
            IsReadOnly(rootFormUiSchemaElement),
            IsDisabled(rootFormUiSchemaElement),
            IsHidden(rootFormUiSchemaElement),
            rootElement,
            null,
            GetRule(rootFormUiSchemaElement)
        );

        return new UiSchemaInterpretation(page: rootPage);
    }

    private UiSchemaPageInterpretation[] InterpretCategories(FormUiSchema uiSchema, JSchema jsonSchema)
    {
        if (uiSchema.Elements.All(x => x.Type == UiSchemaElementType.Category) == false)
        {
            throw new InvalidOperationException($"For a UI Schema of type categorization, all direct child elements must be of type Category");
        }

        var pages = new List<UiSchemaPageInterpretation>();

        foreach (var categoryElement in uiSchema.Elements)
        {
            var pageElements = InterpretUiSchemaLayouts(categoryElement.Elements, jsonSchema);
            var page = new UiSchemaPageInterpretation(
                IsReadOnly(categoryElement),
                IsDisabled(categoryElement),
                IsHidden(categoryElement),
                pageElements,
                (UiSchemaLabelInterpretation)categoryElement,
                GetRule(categoryElement)
            );
            pages.Add(page);
        }

        return [.. pages];
    }

    private UiSchemaElementInterpretationBase[] InterpretUiSchemaLayouts(FormUiSchemaElement[] elements, JSchema jsonSchema)
    {
        var result = new UiSchemaElementInterpretationBase[elements.Length];
        for (var i = 0; i < elements.Length; i++)
        {
            result[i] = Interpret(elements[i], jsonSchema, null);
        }
        return result;
    }

    private UiSchemaElementInterpretationBase Interpret(FormUiSchemaElement element, JSchema jsonSchema, string? parentAbsoluteSchemaJsonPath)
    {
        return element.Type switch
        {
            UiSchemaElementType.HorizontalLayout => InterpretHorizontalLayout(element, jsonSchema, parentAbsoluteSchemaJsonPath),
            UiSchemaElementType.VerticalLayout => InterpretVerticalLayout(element, jsonSchema, parentAbsoluteSchemaJsonPath),
            UiSchemaElementType.Group => InterpretVerticalLayout(element, jsonSchema, parentAbsoluteSchemaJsonPath),
            UiSchemaElementType.Control => InterpretControl(element, jsonSchema, parentAbsoluteSchemaJsonPath),
            UiSchemaElementType.ListWithDetail => InterpretList(element, jsonSchema, parentAbsoluteSchemaJsonPath),
            UiSchemaElementType.ActionButton => InterpretActionButton(element),
            UiSchemaElementType.ArrayLayout => InterpretArrayLayout(element, jsonSchema, parentAbsoluteSchemaJsonPath),
            _ => throw new NotSupportedException()
        };
    }

    private UiSchemaHorizontalLayoutInterpretation InterpretHorizontalLayout(FormUiSchemaElement horizontalLayoutElement, JSchema jsonSchema, string? parentAbsoluteSchemaJsonPath)
    {
        if (!horizontalLayoutElement.HasChildElements)
            throw new InvalidOperationException("Horizontal layout element must have elements defined");

        var result = new UiSchemaHorizontalLayoutInterpretation();

        var columns = horizontalLayoutElement.Elements
            .Select(x => Interpret(x, jsonSchema, parentAbsoluteSchemaJsonPath))
            .ToArray();
        result.SetColumns(columns);

        return result;
    }

    private UiSchemaVerticalLayoutInterpretation InterpretVerticalLayout(FormUiSchemaElement verticalLayoutElement, JSchema jsonSchema, string? parentAbsoluteSchemaJsonPath)
    {
        if (!verticalLayoutElement.HasChildElements)
            throw new InvalidOperationException("Vertical layout element must have elements defined");

        var result = new UiSchemaVerticalLayoutInterpretation((UiSchemaLabelInterpretation)verticalLayoutElement);

        var rows = verticalLayoutElement.Elements
            .Select(X => Interpret(X, jsonSchema, parentAbsoluteSchemaJsonPath))
            .ToArray();
        result.SetRows(rows);

        return result;
    }

    private UiSchemaControlInterpretation InterpretControl(FormUiSchemaElement primitiveControlElement, JSchema jsonSchema, string? parentAbsoluteSchemaJsonPath)
    {
        if (primitiveControlElement.HasChildElements)
            throw new InvalidOperationException("Elements of type 'Control' cannot have child elements");

        var elementSchemaJsonPath = jsonPathInterpreter.FromElementScope(primitiveControlElement.Scope);
        var absoluteSchemaJsonPath = !string.IsNullOrWhiteSpace(parentAbsoluteSchemaJsonPath)
            ? jsonPathInterpreter.JoinJsonPaths(parentAbsoluteSchemaJsonPath, elementSchemaJsonPath)
            : elementSchemaJsonPath;
        var controlJsonPropertyName = jsonPathInterpreter.GetJsonPropertyNameFromPath(absoluteSchemaJsonPath);
        var absoluteParentObjectSchemaPath = jsonPathInterpreter.GetParentPathFromSchemaPath(absoluteSchemaJsonPath);
        var controlType = controlTypeInterpreter.Interpret(jsonSchema, absoluteSchemaJsonPath, absoluteParentObjectSchemaPath);
        var (minimum, maximum) = GetNumericConstraints(jsonSchema, absoluteSchemaJsonPath);

        return new UiSchemaControlInterpretation(
            controlType,
            (UiSchemaLabelInterpretation)primitiveControlElement,
            IsReadOnly(primitiveControlElement),
            IsDisabled(primitiveControlElement),
            IsHidden(primitiveControlElement),
            elementSchemaJsonPath,
            absoluteSchemaJsonPath,
            controlJsonPropertyName,
            absoluteParentObjectSchemaPath,
            ExtractOptions(primitiveControlElement),
            GetRule(primitiveControlElement),
            minimum,
            maximum
        );
    }

    private UiSchemaRuleInterpretation? GetRule(FormUiSchemaElement element)
    {
        if (element.Rule is not null)
        {
            return new UiSchemaRuleInterpretation(
                jsonPathInterpreter.FromElementScope(element.Rule.Condition.Scope),
                JSchema.Parse(
                    ObjectJsonConverter.Serialize(element.Rule.Condition.Schema)
                ),
                element.Rule.Effect
            );
        }

        return null;
    }

    private UiSchemaListInterpretation InterpretList(FormUiSchemaElement listWithDetailsElement, JSchema jsonSchema, string? parentAbsoluteSchemaJsonPath)
    {
        if (!listWithDetailsElement.HasOption(FormUiSchemaOptionKeys.Detail))
            throw new InvalidOperationException("ListWithDetails element must have options.detail defined");

        var listScope = listWithDetailsElement.Scope;
        var listItemsScope = string.Concat(listWithDetailsElement.Scope, "/items");

        var listSchemaJsonPath = jsonPathInterpreter.FromElementScope(listScope);
        var listItemsSchemaJsonPath = jsonPathInterpreter.FromElementScope(listItemsScope);

        var absoluteListSchemaJsonPath = !string.IsNullOrWhiteSpace(parentAbsoluteSchemaJsonPath)
            ? jsonPathInterpreter.JoinJsonPaths(parentAbsoluteSchemaJsonPath, listSchemaJsonPath)
            : listSchemaJsonPath;
        var absoluteListItemSchemaJsonPath = !string.IsNullOrWhiteSpace(parentAbsoluteSchemaJsonPath)
            ? jsonPathInterpreter.JoinJsonPaths(parentAbsoluteSchemaJsonPath, listItemsSchemaJsonPath)
            : listItemsSchemaJsonPath;
        var jsonPropertyName = jsonPathInterpreter.GetJsonPropertyNameFromPath(absoluteListSchemaJsonPath);
        var absoluteParentObjectSchemaPath = jsonPathInterpreter.GetParentPathFromSchemaPath(absoluteListSchemaJsonPath);

        var list = new UiSchemaListInterpretation(
            (UiSchemaLabelInterpretation)listWithDetailsElement,
            IsReadOnly(listWithDetailsElement),
            IsDisabled(listWithDetailsElement),
            IsHidden(listWithDetailsElement),
            listSchemaJsonPath,
            absoluteListSchemaJsonPath,
            listItemsSchemaJsonPath,
            absoluteListItemSchemaJsonPath,
            jsonPropertyName,
            absoluteParentObjectSchemaPath,
            ExtractOptions(listWithDetailsElement),
            GetRule(listWithDetailsElement)
        );

        var detailOption = listWithDetailsElement.GetOption(FormUiSchemaOptionKeys.Detail);
        var detailElement = DefaultJsonConverter.Deserialize<FormUiSchemaElement>($"{detailOption}");
        var detailInterpretation = Interpret(detailElement, jsonSchema, absoluteListItemSchemaJsonPath);

        list.SetListDetail(detailInterpretation);

        return list;
    }

    private UiSchemaActionButtonInterpretation InterpretActionButton(FormUiSchemaElement element)
    {
        var actionKey = $"{element.GetOption(FormUiSchemaOptionKeys.ActionKey) ?? string.Empty}";

        return new UiSchemaActionButtonInterpretation(
            (UiSchemaLabelInterpretation)element,
            IsDisabled(element),
            IsHidden(element),
            actionKey,
            ExtractOptions(element),
            GetRule(element)
        );
    }

    private UiSchemaArrayLayoutInterpretation InterpretArrayLayout(FormUiSchemaElement arrayElement, JSchema jsonSchema, string? parentAbsoluteSchemaJsonPath)
    {
        var arrayScope = arrayElement.Scope
            ?? throw new InvalidOperationException("ArrayLayout element must have a scope defined");

        var arrayItemsScope = string.Concat(arrayScope, "/items");

        var arraySchemaJsonPath      = jsonPathInterpreter.FromElementScope(arrayScope);
        var arrayItemsSchemaJsonPath = jsonPathInterpreter.FromElementScope(arrayItemsScope);

        var absoluteArraySchemaJsonPath = !string.IsNullOrWhiteSpace(parentAbsoluteSchemaJsonPath)
            ? jsonPathInterpreter.JoinJsonPaths(parentAbsoluteSchemaJsonPath, arraySchemaJsonPath)
            : arraySchemaJsonPath;

        var absoluteArrayItemsSchemaJsonPath = !string.IsNullOrWhiteSpace(parentAbsoluteSchemaJsonPath)
            ? jsonPathInterpreter.JoinJsonPaths(parentAbsoluteSchemaJsonPath, arrayItemsSchemaJsonPath)
            : arrayItemsSchemaJsonPath;

        var jsonPropertyName            = jsonPathInterpreter.GetJsonPropertyNameFromPath(absoluteArraySchemaJsonPath);
        var absoluteParentObjectSchemaPath = jsonPathInterpreter.GetParentPathFromSchemaPath(absoluteArraySchemaJsonPath);

        var addLabel = arrayElement.HasOption(FormUiSchemaOptionKeys.AddLabel)
            ? $"{arrayElement.GetOption(FormUiSchemaOptionKeys.AddLabel)}"
            : null;

        // Use the explicit items UI element when provided; otherwise auto-generate a HorizontalLayout
        // from the JSON Schema items.properties so simple arrays work without any extra UI schema.
        var itemsUiElement = arrayElement.Items
            ?? GenerateItemsHorizontalLayout(jsonSchema, absoluteArrayItemsSchemaJsonPath);

        var itemsInterpretation = Interpret(itemsUiElement, jsonSchema, absoluteArrayItemsSchemaJsonPath);

        return new UiSchemaArrayLayoutInterpretation(
            (UiSchemaLabelInterpretation)arrayElement,
            IsReadOnly(arrayElement),
            IsDisabled(arrayElement),
            IsHidden(arrayElement),
            arraySchemaJsonPath,
            absoluteArraySchemaJsonPath,
            arrayItemsSchemaJsonPath,
            absoluteArrayItemsSchemaJsonPath,
            jsonPropertyName,
            absoluteParentObjectSchemaPath,
            addLabel,
            itemsInterpretation,
            ExtractOptions(arrayElement),
            GetRule(arrayElement)
        );
    }

    /// <summary>
    /// Builds a <c>HorizontalLayout</c> element that contains one <c>Control</c> per property
    /// declared in the JSON Schema at <paramref name="absoluteItemsSchemaJsonPath"/>.
    /// Used as the fallback when no <c>items</c> UI element is specified.
    /// </summary>
    private static FormUiSchemaElement GenerateItemsHorizontalLayout(JSchema jsonSchema, string absoluteItemsSchemaJsonPath)
    {
        try
        {
            var schemaToken = JToken.Parse($"{jsonSchema}").SelectToken(absoluteItemsSchemaJsonPath, false);
            if (schemaToken is not null)
            {
                var itemSchema = JSchema.Parse($"{schemaToken}");
                if (itemSchema.Properties.Count > 0)
                {
                    var controls = itemSchema.Properties.Keys
                        .Select(name => new FormUiSchemaElement(
                            UiSchemaElementType.Control,
                            Label: null,
                            I18n: null,
                            Elements: [],
                            Scope: $"#/properties/{name}",
                            Rule: null,
                            Options: null))
                        .ToArray();

                    return new FormUiSchemaElement(
                        UiSchemaElementType.HorizontalLayout,
                        Label: null,
                        I18n: null,
                        Elements: controls,
                        Scope: null,
                        Rule: null,
                        Options: null);
                }
            }
        }
        catch
        {
            // fall through to empty layout
        }

        return new FormUiSchemaElement(
            UiSchemaElementType.HorizontalLayout,
            Label: null,
            I18n: null,
            Elements: [],
            Scope: null,
            Rule: null,
            Options: null);
    }

    private static (double? minimum, double? maximum) GetNumericConstraints(JSchema jsonSchema, string absoluteSchemaJsonPath)
    {
        try
        {
            var schemaToken = JToken.Parse($"{jsonSchema}").SelectToken(absoluteSchemaJsonPath, false);
            if (schemaToken is null) return (null, null);
            var schema = JSchema.Parse($"{schemaToken}");
            return (schema.Minimum, schema.Maximum);
        }
        catch
        {
            return (null, null);
        }
    }

    /// <summary>
    /// Converts <see cref="FormUiSchemaElement.Options"/> (opaque <c>object?</c>) into a flat
    /// <c>IReadOnlyDictionary&lt;string, JToken?&gt;</c> suitable for interpretation models.
    /// Returns an empty dictionary when options are null or empty.
    /// </summary>
    private static IReadOnlyDictionary<string, JToken?> ExtractOptions(FormUiSchemaElement element)
    {
        if (element.Options is null)
            return new Dictionary<string, JToken?>();

        JObject jObj;
        if (element.Options is JObject j)
        {
            jObj = j;
        }
        else
        {
            var json = ObjectJsonConverter.Serialize(element.Options);
            jObj = JObject.Parse(json);
        }

        return jObj.Properties()
                   .ToDictionary(p => p.Name, p => (JToken?)p.Value);
    }

    private static bool IsDisabled(FormUiSchemaElement element)
    {
        return IsBooleanOptionValue(element, FormUiSchemaOptionKeys.Disabled);
    }

    private static bool IsHidden(FormUiSchemaElement element)
    {
        return IsBooleanOptionValue(element, FormUiSchemaOptionKeys.Hidden);
    }

    private static bool IsReadOnly(FormUiSchemaElement element)
    {
        return IsBooleanOptionValue(element, FormUiSchemaOptionKeys.ReadOnly);
    }

    private static bool IsBooleanOptionValue(FormUiSchemaElement element, string option)
    {
        return element.HasOption(option)
            && bool.Parse($"{element.GetOption(option)}");
    }
}
