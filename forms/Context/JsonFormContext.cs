using Newtonsoft.Json.Linq;
using Orbyss.Blazor.JsonForms.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Core.Context.Models;
using Orbyss.Blazor.JsonForms.Core.Constants;
using Orbyss.Blazor.JsonForms.Interpretation.Interfaces;
using Orbyss.Blazor.JsonForms.Utils;
using Orbyss.Blazor.JsonForms.Core.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Core.Context.Notifications;
using Orbyss.Blazor.JsonForms.Core.Interpretation;
using Orbyss.Blazor.JsonForms.Core.Models;
using Orbyss.Blazor.JsonForms.Core;


namespace Orbyss.Blazor.JsonForms.Context;

public sealed class JsonFormContext(
    IJsonFormNotificationHandler notificationHandler,
    IJsonFormDataContext dataContext,
    IJsonFormTranslationContext translationContext,
    IFormUiSchemaInterpreter uiSchemaInterpreter,
    IFormElementContextFactory elementContextFactory,
    IFormRuleEnforcer ruleEnforcer,
    IEnumerable<IJsonFormDefaultTranslationProvider>? defaultTranslationProviders = null
)
    : IJsonFormContext
{
    private FormPageContext[] pages = [];
    private string? activeLanguage;
    private JObject options = [];
    private JsonFormOptions? initOptions;
    private DefaultTranslationResourcesDictionary defaultTranslations = [];

    private bool disabled;
    private bool readOnly;

    public IEnumerable<FormPageContext> GetPages()
    {
        return pages;
    }

    public string? ActiveLanguage => activeLanguage;

    public IJsonFormNotification FormNotification => notificationHandler;

    public int PageCount => pages.Length;

    public bool Disabled => disabled;

    public bool ReadOnly => readOnly;

    public void Instantiate(JsonFormOptions initOpts)
    {
        if (pages.Length > 0)
        {
            throw new InvalidOperationException("Context is already instantiated");
        }

        initOptions = initOpts;

        var data = initOpts.Data ?? new JObject();
        var dataSchema = initOpts.DataSchema;
        var translationSchema = initOpts.TranslationSchema;
        var uiSchema = initOpts.UiSchema;

        dataContext.Instantiate(data, dataSchema);
        defaultTranslations = MergeDefaultTranslations(initOpts.DefaultTranslations, defaultTranslationProviders);
        translationContext.Instantiate(translationSchema, dataSchema, defaultTranslations);

        disabled = initOpts.Disabled;
        activeLanguage = initOpts.Language;
        readOnly = initOpts.ReadOnly;

        var uiSchemaInterpretation = uiSchemaInterpreter.Interpret(uiSchema, dataSchema);
        options = uiSchema.Options?.ToJToken() as JObject ?? [];
        pages = elementContextFactory.CreatePages(uiSchemaInterpretation.Pages);

        EnforceRules();
    }

    public JToken? GetFormOption(string key)
    {
        if (options.ContainsKey(key))
        {
            return options[key];
        }

        return null;
    }

    public bool Validate(Guid? pageId = null)
    {
        var contextsToValidate = pageId.HasValue
            ? pages.FirstOrDefault(x => x.Id == pageId.Value)?.ElementContexts ?? throw new InvalidOperationException($"Page with id '{pageId}' does not exist")
            : pages.SelectMany(x => x.ElementContexts);

        var result = dataContext.Validate(contextsToValidate);

        notificationHandler.Notify(JsonFormNotificationType.OnDataValidated);

        return result;
    }

    public JToken? GetValue(Guid controlContextId)
    {
        var match = FindContextById(controlContextId);
        var controlContext = CastControl(match);
        return dataContext.GetValue(controlContext);
    }

    public void UpdateValue(Guid controlContextId, JToken? value)
    {
        var match = FindContextById(controlContextId);
        var controlContext = CastControl(match);
        dataContext.UpdateValue(controlContext, value);
        EnforceRules();
        notificationHandler.Notify(JsonFormNotificationType.OnDataChanged);
    }

    public JToken GetFormData()
    {
        RemoveHiddenElements();
        return dataContext.GetFormData();
    } 

    public void UpdateFormData(Action<JToken> updateDelegate)
    {
        var data = dataContext.GetFormData();
        updateDelegate(data);
        EnforceRules();
        Validate();
        notificationHandler.Notify(JsonFormNotificationType.OnDataChanged);
    }

    public string? GetDataContextError(Guid controlContextId)
    {
        var match = FindContextById(controlContextId);
        if (match is FormListContext list && list.Errors.Any())
        {
            return translationContext.TranslateErrors(ActiveLanguage, list.Errors, list.Interpretation);
        }

        if (match is FormControlContext control && control.Errors.Any())
        {
            return translationContext.TranslateErrors(ActiveLanguage, control.Errors, control.Interpretation);
        }

        return null;
    }

    public string? GetLabel(Guid contextId)
    {
        var page = pages.FirstOrDefault(x => x.Id == contextId);
        if (page is not null)
        {
            if (page.LabelInterpretation is null)
                return string.Empty;

            return translationContext.TranslateLabel(
                ActiveLanguage,
                page.LabelInterpretation
            );
        }

        var match = FindContextById(contextId);
        if (match is FormControlContext control)
            return translationContext.TranslateLabel(ActiveLanguage, control.Interpretation);
        if (match is FormListContext list)
            return translationContext.TranslateLabel(ActiveLanguage, list.Interpretation);
        if (match is IFormElementContext context && context.Interpretation.Label is not null)
            return translationContext.TranslateLabel(ActiveLanguage, context.Interpretation.Label);

        throw new ArgumentException($"Could not get the translated label for context '{match.Id}'");
    }

    public string? GetHelperIconText(Guid controlContextId)
    {
        var match = FindContextById(controlContextId);
        if (match is not FormControlContext control)
            return null;

        var optionValue = control.Interpretation.GetOption(FormUiSchemaOptionKeys.HelperIconTextLabel);
        if (optionValue is null)
            return null;

        var literalValue = $"{optionValue}";
        var labelInterpretation = new UiSchemaLabelInterpretation(Label: literalValue, I18n: literalValue);
        return translationContext.TranslateLabel(ActiveLanguage, labelInterpretation) ?? literalValue;
    }

    public string? GetHelperText(Guid controlContextId)
    {
        var match = FindContextById(controlContextId);
        if (match is not FormControlContext control)
            return null;

        var optionValue = control.Interpretation.GetOption(FormUiSchemaOptionKeys.HelperTextLabel);
        if (optionValue is null)
            return null;

        var literalValue = $"{optionValue}";
        var labelInterpretation = new UiSchemaLabelInterpretation(Label: literalValue, I18n: literalValue);
        return translationContext.TranslateLabel(ActiveLanguage, labelInterpretation) ?? literalValue;
    }

    public string? GetPrefixText(Guid controlContextId)
    {
        var match = FindContextById(controlContextId);
        if (match is not FormControlContext control)
            return null;

        var optionValue = control.Interpretation.GetOption(FormUiSchemaOptionKeys.PrefixLabel);
        if (optionValue is null)
            return null;

        var literalValue = $"{optionValue}";
        var labelInterpretation = new UiSchemaLabelInterpretation(Label: literalValue, I18n: literalValue);
        return translationContext.TranslateLabel(ActiveLanguage, labelInterpretation) ?? literalValue;
    }

    public string? GetSuffixText(Guid controlContextId)
    {
        var match = FindContextById(controlContextId);
        if (match is not FormControlContext control)
            return null;

        var optionValue = control.Interpretation.GetOption(FormUiSchemaOptionKeys.SuffixLabel);
        if (optionValue is null)
            return null;

        var literalValue = $"{optionValue}";
        var labelInterpretation = new UiSchemaLabelInterpretation(Label: literalValue, I18n: literalValue);
        return translationContext.TranslateLabel(ActiveLanguage, labelInterpretation) ?? literalValue;
    }

    public string? GetArrayAddLabel(Guid arrayContextId)
    {
        var match = FindContextById(arrayContextId);
        if (match is not FormArrayContext array) return null;

        var rawKey = array.Interpretation.AddLabel;
        if (string.IsNullOrWhiteSpace(rawKey)) return null;

        var labelInterp = new UiSchemaLabelInterpretation(Label: rawKey, I18n: rawKey);
        return translationContext.TranslateLabel(ActiveLanguage, labelInterp) ?? rawKey;
    }

    public string? GetTranslatedLabel(string translationKey)
    {
        if (string.IsNullOrWhiteSpace(translationKey))
            return null;

        var labelInterpretation = new UiSchemaLabelInterpretation(translationKey, translationKey);
        return translationContext.TranslateLabel(ActiveLanguage, labelInterpretation);
    }

    public JsonFormOptions CreateArrayItemFormOptions(Guid arrayContextId, JToken? itemData = null)
    {
        if (initOptions is null)
            throw new InvalidOperationException("The form context has not been instantiated.");

        var match = FindContextById(arrayContextId);
        var arrayContext = CastArray(match);
        var itemSchemaToken = JToken.Parse($"{initOptions.DataSchema}")
            .SelectToken(arrayContext.Interpretation.AbsoluteItemsSchemaJsonPath, false)
            ?? throw new InvalidOperationException(
                $"Could not find the array item schema at '{arrayContext.Interpretation.AbsoluteItemsSchemaJsonPath}'.");

        return new JsonFormOptions(
            Newtonsoft.Json.Schema.JSchema.Parse($"{itemSchemaToken}"),
            ToRootUiSchema(arrayContext.Interpretation.ItemUiSchema),
            initOptions.TranslationSchema)
        {
            ConfigureFactories = initOptions.ConfigureFactories,
            Data = itemData?.DeepClone() ?? new JObject(),
            DefaultTranslations = new DefaultTranslationResourcesDictionary(defaultTranslations),
            Disabled = disabled,
            Language = activeLanguage,
            ReadOnly = readOnly
        };
    }

    public string? GetCssClass(Guid elementContextId)
    {
        var match = FindContextById(elementContextId);

        var optionValue = match switch
        {
            FormControlContext control => control.Interpretation.GetOption(FormUiSchemaOptionKeys.CssClass),
            FormActionButtonContext btn => btn.Interpretation.GetOption(FormUiSchemaOptionKeys.CssClass),
            _ => null
        };

        return optionValue is null ? null : $"{optionValue}";
    }

    public IEnumerable<TranslatedEnumItem> GetTranslatedEnumItems(Guid controlContextId)
    {
        var match = FindContextById(controlContextId);
        var controlContext = CastControl(match);

        var items = translationContext.TranslateEnum(ActiveLanguage, controlContext.Interpretation) ?? [];

        var enumItemOptionsToken = controlContext.Interpretation.GetOption(FormUiSchemaOptionKeys.EnumItemOptions);
        if (enumItemOptionsToken is null)
            return items;

        var optionsObject = JObject.Parse($"{enumItemOptionsToken}");

        return items.Select(item =>
        {
            if (optionsObject.TryGetValue(item.Value, StringComparison.OrdinalIgnoreCase, out var itemConfig)
                && itemConfig is JObject itemConfigObj
                && itemConfigObj.TryGetValue("helperText", StringComparison.OrdinalIgnoreCase, out var helperTextToken))
            {
                return new TranslatedEnumItem(item.Label, item.Value, $"{helperTextToken}");
            }
            return item;
        });
    }

    public FormPageContext GetPage(int index)
    {
        return pages[index];
    }

    public void InstantiateList(Guid listContextId)
    {
        var listMatch = FindContextById(listContextId);
        var listContext = CastList(listMatch);
        dataContext.InstantiateList(listContext);
    }

    public void AddListItem(Guid listContextId)
    {
        var match = FindContextById(listContextId);
        var listContext = CastList(match);
        dataContext.AddListItem(listContext);
        notificationHandler.Notify(JsonFormNotificationType.OnDataChanged);
    }

    public void RemoveListItem(Guid listContextId, Guid listItemContextId)
    {
        var listMatch = FindContextById(listContextId);
        var listItemMatch = FindContextById(listItemContextId);

        var listContext = CastList(listMatch);
        dataContext.RemoveListItem(listContext, listItemMatch);
        notificationHandler.Notify(JsonFormNotificationType.OnDataChanged);
    }

    public void InstantiateArray(Guid arrayContextId)
    {
        var match = FindContextById(arrayContextId);
        var arrayContext = CastArray(match);
        dataContext.InstantiateArray(arrayContext);
    }

    public void AddArrayItem(Guid arrayContextId)
    {
        var match = FindContextById(arrayContextId);
        var arrayContext = CastArray(match);
        dataContext.AddArrayItem(arrayContext);
        var addedIndex = arrayContext.Items.Length - 1;
        EnforceRules();
        notificationHandler.Notify(JsonFormNotificationType.OnDataChanged);
        _ = initOptions?.InvokeArrayItemAdded(arrayContext, addedIndex, this);
    }

    public void AddArrayItem(Guid arrayContextId, JToken itemData)
    {
        var match = FindContextById(arrayContextId);
        var arrayContext = CastArray(match);
        dataContext.AddArrayItem(arrayContext, itemData);
        var addedIndex = arrayContext.Items.Length - 1;
        EnforceRules();
        notificationHandler.Notify(JsonFormNotificationType.OnDataChanged);
        _ = initOptions?.InvokeArrayItemAdded(arrayContext, addedIndex, this);
    }

    public void UpdateArrayItem(Guid arrayContextId, Guid arrayItemId, JToken itemData)
    {
        var match = FindContextById(arrayContextId);
        var arrayContext = CastArray(match);
        // Capture the index before the rebuild regenerates item ids.
        var updatedIndex = arrayContext.Items
            .Select((item, i) => (item, i))
            .FirstOrDefault(x => x.item.Id == arrayItemId).i;
        dataContext.UpdateArrayItem(arrayContext, arrayItemId, itemData);
        EnforceRules();
        notificationHandler.Notify(JsonFormNotificationType.OnDataChanged);
        _ = initOptions?.InvokeArrayItemUpdated(arrayContext, updatedIndex, this);
    }

    public JToken? GetArrayItemData(Guid arrayContextId, Guid arrayItemId)
    {
        var match = FindContextById(arrayContextId);
        var arrayContext = CastArray(match);
        return dataContext.GetArrayItemData(arrayContext, arrayItemId);
    }

    public void RemoveArrayItem(Guid arrayContextId, Guid arrayItemId)
    {
        var match = FindContextById(arrayContextId);
        var arrayContext = CastArray(match);
        // Capture the index before removal so we can pass it to the event
        var removedIndex = arrayContext.Items
            .Select((item, i) => (item, i))
            .FirstOrDefault(x => x.item.Id == arrayItemId).i;
        dataContext.RemoveArrayItem(arrayContext, arrayItemId);
        EnforceRules();
        notificationHandler.Notify(JsonFormNotificationType.OnDataChanged);
        _ = initOptions?.InvokeArrayItemRemoved(arrayContext, removedIndex, this);
    }

    public void MoveArrayItem(Guid arrayContextId, int fromIndex, int toIndex)
    {
        var match = FindContextById(arrayContextId);
        var arrayContext = CastArray(match);
        dataContext.MoveArrayItem(arrayContext, fromIndex, toIndex);
        EnforceRules();
        notificationHandler.Notify(JsonFormNotificationType.OnDataChanged);
        _ = initOptions?.InvokeArrayItemMoved(arrayContext, fromIndex, toIndex, this);
    }

    public Task NotifyArrayItemAdded(Guid arrayContextId, int addedIndex)
    {
        var match = FindContextById(arrayContextId);
        return initOptions?.InvokeArrayItemAdded(CastArray(match), addedIndex, this) ?? Task.CompletedTask;
    }

    public Task NotifyArrayItemRemoved(Guid arrayContextId, int removedIndex)
    {
        var match = FindContextById(arrayContextId);
        return initOptions?.InvokeArrayItemRemoved(CastArray(match), removedIndex, this) ?? Task.CompletedTask;
    }

    public Task NotifyArrayItemMoved(Guid arrayContextId, int fromIndex, int toIndex)
    {
        var match = FindContextById(arrayContextId);
        return initOptions?.InvokeArrayItemMoved(CastArray(match), fromIndex, toIndex, this) ?? Task.CompletedTask;
    }

    public Task NotifyArrayItemUpdated(Guid arrayContextId, int updatedIndex)
    {
        var match = FindContextById(arrayContextId);
        return initOptions?.InvokeArrayItemUpdated(CastArray(match), updatedIndex, this) ?? Task.CompletedTask;
    }

    public void ChangeLanguage(string language)
    {
        Validate();
        activeLanguage = language;
        notificationHandler.Notify(JsonFormNotificationType.OnLanguageChanged);
    }

    public void ChangeDisabled(bool disabled)
    {
        this.disabled = disabled;
        notificationHandler.Notify(JsonFormNotificationType.OnDisabledChanged);
    }

    public void ChangeReadOnly(bool readOnly)
    {
        this.readOnly = readOnly;
        notificationHandler.Notify(JsonFormNotificationType.OnReadOnlyChanged);
    }

    private static FormControlContext CastControl(IFormElementContext context)
    {
        return context as FormControlContext
            ?? throw new InvalidCastException($"Context of type '{context.GetType()}' could not be cast to type '{typeof(FormControlContext)}'");
    }

    private static FormListContext CastList(IFormElementContext context)
    {
        return context as FormListContext
            ?? throw new InvalidCastException($"Context of type '{context.GetType()}' could not be cast to type '{typeof(FormListContext)}'");
    }

    private static FormArrayContext CastArray(IFormElementContext context)
    {
        return context as FormArrayContext
            ?? throw new InvalidCastException($"Context of type '{context.GetType()}' could not be cast to type '{typeof(FormArrayContext)}'");
    }

    private void EnforceRules()
    {
        var rootContexts = GetAllRootElementContexts();
        for (var j = 0; j < rootContexts.Length; j++)
        {
            var context = rootContexts[j];
            ruleEnforcer.EnforceRule(dataContext, context, rootContexts);
        }

        ruleEnforcer.EnforceRulesForPages(dataContext, pages, rootContexts);
    }

    private IFormElementContext[] GetAllRootElementContexts()
    {
        var result = new List<IFormElementContext>();

        for (var i = 0; i < pages.Length; i++)
        {
            var page = pages[i];
            result.AddRange(page.ElementContexts);
        }

        return [.. result];
    }

    public FormControlContext? FindControl(Func<FormControlContext, bool> predicate)
    {
        return pages
            .SelectMany(p => p.FindContexts(_ => true))
            .OfType<FormControlContext>()
            .FirstOrDefault(predicate);
    }

    public IEnumerable<FormControlContext> FindControls(Func<FormControlContext, bool> predicate)
    {
        return pages
            .SelectMany(p => p.FindContexts(_ => true))
            .OfType<FormControlContext>()
            .Where(predicate);
    }

    public async Task NotifyControlValueChanged(Guid controlContextId)
    {
        if (initOptions is null) return;
        var match = FindContextById(controlContextId);
        if (match is FormControlContext control)
            await initOptions.InvokeControlValueChanged(control, this);
    }

    public async Task NotifyControlInputChanged(Guid controlContextId)
    {
        if (initOptions is null) return;
        var match = FindContextById(controlContextId);
        if (match is FormControlContext control)
            await initOptions.InvokeControlInputChanged(control, this);
    }

    public Task InvokeAction(string actionKey)
        => initOptions?.InvokeAction(actionKey, this) ?? Task.CompletedTask;

    private IFormElementContext FindContextById(Guid id)
    {
        foreach (var page in pages)
        {
            var result = page.FindContextById(id);
            if (result is not null)
                return result;
        }

        throw new InvalidOperationException($"Could not find context by id '{id}'");
    }

    private static Core.UiSchema.FormUiSchema ToRootUiSchema(Core.UiSchema.FormUiSchemaElement element)
    {
        return new Core.UiSchema.FormUiSchema(
            element.Type,
            element.Scope,
            element.Label,
            element.Elements,
            element.Options);
    }

    private static DefaultTranslationResourcesDictionary MergeDefaultTranslations(
        DefaultTranslationResourcesDictionary optionDefaults,
        IEnumerable<IJsonFormDefaultTranslationProvider>? providers)
    {
        var result = new DefaultTranslationResourcesDictionary();

        MergeInto(result, optionDefaults);

        if (providers is not null)
        {
            foreach (var provider in providers)
            {
                MergeInto(result, provider.GetDefaultTranslations());
            }
        }

        return result;
    }

    private static void MergeInto(
        DefaultTranslationResourcesDictionary target,
        DefaultTranslationResourcesDictionary source)
    {
        foreach (var (language, sourceSections) in source)
        {
            if (!target.TryGetValue(language, out var targetSections))
            {
                targetSections = new Dictionary<string, TranslationSection>(StringComparer.OrdinalIgnoreCase);
                target[language] = targetSections;
            }

            foreach (var (key, section) in sourceSections)
            {
                targetSections.TryAdd(key, section);
            }
        }
    }

    private void RemoveHiddenElements()
    {        
        var displayedContexts = pages
            .SelectMany(x => x.FindContexts(ctx => !ctx.Hidden && ctx is FormControlContext))
            .Cast<FormControlContext>();

        var hiddenContexts = pages
            .SelectMany(x =>
            {
                return x.FindContexts(
                    ctx => ctx.Hidden && ctx is FormControlContext controlContext && IsTrulyHidden(controlContext, displayedContexts)
                );
            })
            .Cast<FormControlContext>();

        foreach (var ctx in hiddenContexts)
            dataContext.UpdateValue(ctx, null);
    }

    private static bool IsTrulyHidden(FormControlContext ctx, IEnumerable<FormControlContext> displayedContexts)
    {
        return !displayedContexts.Any(displayedCtx => displayedCtx.Interpretation.AbsoluteSchemaJsonPath == ctx.Interpretation.AbsoluteSchemaJsonPath);
    }
}
