namespace Orbyss.Blazor.JsonForms.Interpretation;

public sealed class UiSchemaHorizontalLayoutInterpretation()
    : UiSchemaElementInterpretationBase(null)
{
    public override UiSchemaElementInterpretationType ElementType => UiSchemaElementInterpretationType.HorizontalLayout;

    public UiSchemaElementInterpretationBase[] Columns { get; private set; } = [];

    internal void SetColumns(UiSchemaElementInterpretationBase[] columns)
    {
        if (Columns.Length > 0)
        {
            throw new InvalidOperationException("Columns are already set");
        }

        Columns = columns;
    }
}
