namespace Orbyss.Blazor.JsonForms.Core.Interpretation;

public sealed class UiSchemaHorizontalLayoutInterpretation()
    : UiSchemaElementInterpretationBase(null)
{
    public override UiSchemaElementInterpretationType ElementType => UiSchemaElementInterpretationType.HorizontalLayout;

    public UiSchemaElementInterpretationBase[] Columns { get; private set; } = [];

    public void SetColumns(UiSchemaElementInterpretationBase[] columns)
    {
        if (Columns.Length > 0)
        {
            throw new InvalidOperationException("Columns are already set");
        }

        Columns = columns;
    }
}
