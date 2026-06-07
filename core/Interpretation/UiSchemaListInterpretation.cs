using Newtonsoft.Json.Linq;

namespace Orbyss.Blazor.JsonForms.Core.Interpretation;

public class UiSchemaListInterpretation(
    UiSchemaLabelInterpretation? labelInterpretation,
    bool readOnly,
    bool disabled,
    bool hidden,
    string relativeSchemaJsonPath,
    string absoluteSchemaJsonPath,
    string relativeItemsSchemaJsonPath,
    string absoluteItemsSchemaJsonPath,
    string listJsonPropertyName,
    string? absoluteParentObjectSchemaPath,
    IReadOnlyDictionary<string, JToken?> options,
    UiSchemaRuleInterpretation? rule)

    : UiSchemaControlInterpretationBase(
        labelInterpretation,
        readOnly,
        disabled,
        hidden,
        relativeSchemaJsonPath,
        absoluteSchemaJsonPath,
        listJsonPropertyName,
        absoluteParentObjectSchemaPath,
        options,
        rule)
{
    private UiSchemaElementInterpretationBase? listDetails;

    public override UiSchemaElementInterpretationType ElementType => UiSchemaElementInterpretationType.List;

    public string AbsoluteItemsSchemaJsonPath { get; } = absoluteItemsSchemaJsonPath;

    public string ListJsonPropertyName { get; } = listJsonPropertyName;

    public string RelativeItemsSchemaJsonPath { get; } = relativeItemsSchemaJsonPath;

    public UiSchemaElementInterpretationBase GetListDetail()
    {
        if (listDetails is null)
        {
            throw new ArgumentException("List details is not set for this list");
        }
        return listDetails;
    }

    public void SetListDetail(UiSchemaElementInterpretationBase details)
    {
        if (this.listDetails is not null)
        {
            throw new InvalidOperationException("List details are already set.");
        }

        this.listDetails = details;
    }
}
