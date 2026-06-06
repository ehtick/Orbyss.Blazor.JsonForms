using Orbyss.Blazor.JsonForms.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Interpretation;

namespace Orbyss.Blazor.JsonForms.Context.Models;

public sealed class FormPageContext(
    UiSchemaPageInterpretation pageInterpretation,
    IFormElementContext[] elementContexts)
{
    private readonly Guid id = Guid.NewGuid();
    private bool? disabledOverwrite;
    private bool? hiddenOverwrite;

    public IFormElementContext[] ElementContexts { get; } = elementContexts;

    public UiSchemaLabelInterpretation? LabelInterpretation { get; } = pageInterpretation.LabelInterpretation;

    public Guid Id => id;

    public IFormElementContext? FindContextById(Guid id)
    {
        return FindInElements(ElementContexts, id);
    }

    public IEnumerable<IFormElementContext> FindContexts(Func<IFormElementContext, bool> predicate)
    {
        var result = new List<IFormElementContext>();
        FindInElements(ElementContexts, predicate, result);
        return result;
    }

    public bool Disabled => disabledOverwrite ?? pageInterpretation.Disabled;

    public bool Hidden => hiddenOverwrite ?? pageInterpretation.Hidden;

    public UiSchemaRuleInterpretation? Rule { get; } = pageInterpretation.Rule;

    public void SetHidden(bool? value)
    {
        hiddenOverwrite = value;
    }

    public void SetDisabled(bool? value)
    {
        disabledOverwrite = value;
    }

    private IFormElementContext? FindInElement(IFormElementContext elementContext, Guid id)
    {
        return elementContext.Interpretation.ElementType switch
        {
            UiSchemaElementInterpretationType.VerticalLayout  => FindInVerticalLayout((FormVerticalLayoutContext)elementContext, id),
            UiSchemaElementInterpretationType.HorizontalLayout => FindInHorizontalLayout((FormHorizontalLayoutContext)elementContext, id),
            UiSchemaElementInterpretationType.List            => FindInList((FormListContext)elementContext, id),
            UiSchemaElementInterpretationType.ArrayLayout     => FindInArray((FormArrayContext)elementContext, id),
            UiSchemaElementInterpretationType.Control         => elementContext.Id == id ? elementContext : null,
            UiSchemaElementInterpretationType.ActionButton    => elementContext.Id == id ? elementContext : null,

            _ => throw new NotSupportedException($"Element type '{elementContext.Interpretation.ElementType} is not supported'")
        };
    }

    private void FindInElement(IFormElementContext elementContext, Func<IFormElementContext, bool> predicate, List<IFormElementContext> result)
    {
        switch (elementContext.Interpretation.ElementType)
        {
            case UiSchemaElementInterpretationType.VerticalLayout:
                FindInVerticalLayout((FormVerticalLayoutContext)elementContext, predicate, result); break;
            case UiSchemaElementInterpretationType.HorizontalLayout:
                FindInHorizontalLayout((FormHorizontalLayoutContext)elementContext, predicate, result); break;
            case UiSchemaElementInterpretationType.List:
                FindInList((FormListContext)elementContext, predicate, result); break;
            case UiSchemaElementInterpretationType.ArrayLayout:
                FindInArray((FormArrayContext)elementContext, predicate, result); break;
            case UiSchemaElementInterpretationType.Control:
            case UiSchemaElementInterpretationType.ActionButton:
                if (predicate(elementContext))
                {
                    result.Add(elementContext);
                }
                break;

            default:
                throw new NotSupportedException($"Element type '{elementContext.Interpretation.ElementType} is not supported'");
        }
    }
    private IFormElementContext? FindInList(FormListContext listContext, Guid id)
    {
        if (listContext.Id == id)
            return listContext;

        return FindInElements(listContext.Items, id);
    }

    private void FindInList(FormListContext listContext, Func<IFormElementContext, bool> predicate, List<IFormElementContext> result)
    {
        if (predicate(listContext))
            result.Add(listContext);

        FindInElements(listContext.Items, predicate, result);
    }

    private IFormElementContext? FindInVerticalLayout(FormVerticalLayoutContext verticalLayoutContext, Guid id)
    {
        if (verticalLayoutContext.Id == id)
            return verticalLayoutContext;

        return FindInElements(verticalLayoutContext.Rows, id);
    }

    private void FindInVerticalLayout(FormVerticalLayoutContext verticalLayoutContext, Func<IFormElementContext, bool> predicate, List<IFormElementContext> result)
    {
        if (predicate(verticalLayoutContext))
            result.Add(verticalLayoutContext);

        FindInElements(verticalLayoutContext.Rows, predicate, result);
    }

    private void FindInHorizontalLayout(FormHorizontalLayoutContext horizontalLayoutContext, Func<IFormElementContext, bool> predicate, List<IFormElementContext> result)
    {
        if (predicate(horizontalLayoutContext))
            result.Add(horizontalLayoutContext);

        FindInElements(horizontalLayoutContext.Columns, predicate, result);
    }

    private IFormElementContext? FindInHorizontalLayout(FormHorizontalLayoutContext horizontalLayoutContext, Guid id)
    {
        if (horizontalLayoutContext.Id == id)
            return horizontalLayoutContext;

        return FindInElements(horizontalLayoutContext.Columns, id);
    }

    private IFormElementContext? FindInArray(FormArrayContext arrayContext, Guid id)
    {
        if (arrayContext.Id == id)
            return arrayContext;

        foreach (var item in arrayContext.Items)
        {
            var result = FindInElement(item.ElementContext, id);
            if (result is not null)
                return result;
        }
        return null;
    }

    private void FindInArray(FormArrayContext arrayContext, Func<IFormElementContext, bool> predicate, List<IFormElementContext> result)
    {
        if (predicate(arrayContext))
            result.Add(arrayContext);

        foreach (var item in arrayContext.Items)
            FindInElement(item.ElementContext, predicate, result);
    }

    private IFormElementContext? FindInElements(IEnumerable<IFormElementContext> elementContexts, Guid id)
    {
        foreach (var context in elementContexts)
        {
            var result = FindInElement(context, id);
            if (result is not null)
                return result;
        }

        return null;
    }

    private void FindInElements(IEnumerable<IFormElementContext> elementContexts, Func<IFormElementContext, bool> predicate, List<IFormElementContext> result)
    {
        foreach (var context in elementContexts)
        {
            FindInElement(context, predicate, result);            
        }
    }
}