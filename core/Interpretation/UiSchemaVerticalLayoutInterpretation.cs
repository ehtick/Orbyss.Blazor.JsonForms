namespace Orbyss.Blazor.JsonForms.Core.Interpretation;

public sealed class UiSchemaVerticalLayoutInterpretation(UiSchemaLabelInterpretation? labelInterpretation)
    : UiSchemaElementInterpretationBase(labelInterpretation)
{
    public override UiSchemaElementInterpretationType ElementType => UiSchemaElementInterpretationType.VerticalLayout;

    public UiSchemaElementInterpretationBase[] Rows { get; private set; } = [];

    public void SetRows(UiSchemaElementInterpretationBase[] rows)
    {
        if (Rows.Length > 0)
        {
            throw new InvalidOperationException("Rows are already set");
        }

        Rows = rows;
    }
}
