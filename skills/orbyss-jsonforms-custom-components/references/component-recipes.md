# Component recipes (dependency-free)

Copy-pasteable examples for every Orbyss JSON Forms slot, using plain HTML +
the `--orbyss-form-*` theme contract. All `@using` lines assume
`Orbyss.Blazor.JsonForms.Core.*`. Register each component on the matching
sub-factory (see SKILL.md).

## Table of contents
- [Dropdown (Enum) and multi-select (EnumList)](#dropdown)
- [Checkbox (Boolean)](#checkbox)
- [Number / integer with prefix-suffix](#numeric)
- [Date pickers (DateOnly / DateTime / *UtcTicks)](#date)
- [Slider via the `component` alias](#slider)
- [Buttons (Submit / Next / Previous)](#buttons)
- [Navigation / stepper (multi-page)](#navigation)
- [List + list item (ListWithDetail)](#list)
- [Action button](#actionbutton)
- [Array layout — inline and dialog](#array)
- [Wiring a full control factory](#factory)

---

## Dropdown

`Enum` binds `string`; `EnumList` binds `IEnumerable<string>`. Both receive
`Items` (`IEnumerable<TranslatedEnumItem>` — each has `Value`, `Label`,
`HelperText?`) and `MultiSelect`. You can ship one component that switches on
`MultiSelect`, or two components on the `Dropdown`/`MultiSelect` slots. Single:

```razor
@* MyDropdown.razor *@
@using Orbyss.Blazor.JsonForms.Core.ComponentBases
@using Orbyss.Blazor.JsonForms.Core.Models
@inherits FormInputComponentBase<string>

@if (!string.IsNullOrWhiteSpace(Label)) { <span class="my-field__label">@Label</span> }

<select class="@Class" disabled="@Disabled"
        @onchange="e => OnValueChanged.InvokeAsync(e.Value?.ToString())">
    <option value="" selected="@string.IsNullOrEmpty(Value)"></option>
    @foreach (var item in Items)
    {
        <option value="@item.Value" selected="@(item.Value == Value)">@item.Label</option>
    }
</select>

@if (!string.IsNullOrWhiteSpace(ErrorHelperText)) { <span class="my-field__error">@ErrorHelperText</span> }
```

Multi-select (`EnumList`) — emit `IEnumerable<string>`:

```razor
@* MyMultiSelect.razor *@
@using Orbyss.Blazor.JsonForms.Core.ComponentBases
@using Orbyss.Blazor.JsonForms.Core.Models
@inherits FormInputComponentBase<IEnumerable<string>>

@foreach (var item in Items)
{
    var selected = Value?.Contains(item.Value) == true;
    <label class="my-chip">
        <input type="checkbox" checked="@selected" disabled="@Disabled"
               @onchange="() => Toggle(item.Value)" />
        @item.Label
    </label>
}

@code {
    private void Toggle(string value)
    {
        var set = new HashSet<string>(Value ?? []);
        if (!set.Add(value)) set.Remove(value);
        OnValueChanged.InvokeAsync(set);
    }
}
```

## Checkbox

`Boolean` binds `bool` (non-nullable):

```razor
@* MyCheckbox.razor *@
@using Orbyss.Blazor.JsonForms.Core.ComponentBases
@inherits FormInputComponentBase<bool>

<label class="my-check">
    <input type="checkbox" class="@Class" checked="@Value" disabled="@Disabled"
           @onchange="e => OnValueChanged.InvokeAsync((bool)(e.Value ?? false))" />
    <span>@Label</span>
</label>
```

## Numeric

`Integer` binds `int?`, `Number` binds `double?`. Declare `PrefixText`/`SuffixText`
to render the schema's `prefixLabel`/`suffixLabel`. Use `Culture` for formatting.

```razor
@* MyNumberInput.razor *@
@using Orbyss.Blazor.JsonForms.Core.ComponentBases
@inherits FormInputComponentBase<double?>

@if (!string.IsNullOrWhiteSpace(Label)) { <span class="my-field__label">@Label</span> }
<div class="my-numeric">
    @if (!string.IsNullOrWhiteSpace(PrefixText)) { <span class="my-numeric__prefix">@PrefixText</span> }
    <input type="number" class="@Class" value="@Value" disabled="@Disabled" readonly="@ReadOnly"
           @onchange="OnChange" />
    @if (!string.IsNullOrWhiteSpace(SuffixText)) { <span class="my-numeric__suffix">@SuffixText</span> }
</div>

@code {
    private Task OnChange(ChangeEventArgs e)
    {
        var raw = e.Value?.ToString();
        double? value = double.TryParse(raw, System.Globalization.NumberStyles.Any, Culture, out var d) ? d : null;
        return OnValueChanged.InvokeAsync(value);
    }
}
```

For integers, inherit `FormInputComponentBase<int?>` and parse with `int.TryParse`.
`Minimum`/`Maximum` from the JSON Schema are on `control.Interpretation` — read
them in the factory and pass via `parameters` or `SetParameter` if your input needs
range attributes.

## Date

`DateOnly` stores `"yyyy-MM-dd"` (string), `DateTime` stores an ISO datetime,
`DateOnlyUtcTicks`/`DateTimeUtcTicks` store a UTC-ticks `long`. Override
`ConvertFromJToken` for the string/ticks cases (the engine handles the write-back).

```razor
@* MyDatePicker.razor — DateOnly *@
@using Orbyss.Blazor.JsonForms.Core.ComponentBases
@inherits FormInputComponentBase<DateOnly?>

<input type="date" class="@Class" disabled="@Disabled"
       value="@(Value?.ToString("yyyy-MM-dd"))"
       @onchange="OnChange" />

@code {
    public override object? ConvertFromJToken(Newtonsoft.Json.Linq.JToken? token)
        => token is null || token.Type == Newtonsoft.Json.Linq.JTokenType.Null
            ? null : DateOnly.ParseExact(token.ToString(), "yyyy-MM-dd");

    private Task OnChange(ChangeEventArgs e)
        => OnValueChanged.InvokeAsync(DateOnly.TryParse(e.Value?.ToString(), out var d) ? d : null);
}
```

`DateTimeUtcTicks` / `DateUtcTicks` (from `Orbyss.Blazor.JsonForms.Core.Models`)
expose `.UtcTicks` and `.DateTime`; bind their nullable forms and construct
`new DateTimeUtcTicks(ticks)` on change.

## Slider

A slider is best exposed as a `component` alias so the schema opts in per field.
Make it generic over the numeric type and register an alias that resolves to the
right closed type:

```csharp
public sealed class MySlider<TValue> : FormInputComponentBase<TValue> { /* … */ }

public sealed class MyControlFactory : ControlComponentFactory
{
    protected override Type? ResolveComponentType(string? alias, ControlType controlType)
    {
        if (string.Equals(alias, "slider", StringComparison.OrdinalIgnoreCase))
            return controlType == ControlType.Integer
                ? typeof(MySlider<int?>)
                : typeof(MySlider<double?>);
        return base.ResolveComponentType(alias, controlType);
    }
}
```

Schema: `{ "type": "Control", "scope": "#/properties/volume", "options": { "component": "slider", "parameters": { "Step": 5 } } }`.

## Buttons

```razor
@* MyButton.razor — used for Submit / Next / Previous slots *@
@using Orbyss.Blazor.JsonForms.Core.ComponentBases
@inherits FormButtonComponentBase

<button type="button" class="@Class orbyss-form-action-button"
        disabled="@Disabled" @onclick="() => OnClicked.InvokeAsync()">@Text</button>
```

`Text` is the engine-resolved label; `OnClicked` runs the form's submit/navigation
logic (validation included).

## Navigation

Multi-page (Categorization) forms render your navigation component. It owns the
prev/next/submit chrome and page progress. Use the cascaded `IJsonFormContext` to
read `GetPages()` / `PageCount` and to render each page's `FormSinglePage`
equivalent, or build your own stepper. Minimal shape:

```razor
@* MyStepper.razor *@
@using Orbyss.Blazor.JsonForms.Core.ComponentBases
@inherits FormNavigationComponentBase

@* render the current page's elements (via the engine's page contexts),
   plus Prev/Next, and a Submit button that calls OnSubmitClicked unless
   HideSubmitButton is true *@
```

For a working multi-page navigation, model it on the engine's single-page flow:
iterate `FormContext.GetPages()`, render each element with `<FormElementSelector>`,
and call `OnSubmitClicked` on the last page. (Most apps use single-page forms and
never need this slot.)

## List

`ListWithDetail` renders a container + one item wrapper per entry; the engine
supplies the item fields as `ChildContent`.

```razor
@* MyList.razor *@
@using Orbyss.Blazor.JsonForms.Core.ComponentBases
@inherits FormListComponentBase

<div class="my-list">
    @if (!string.IsNullOrWhiteSpace(Title)) { <div class="orbyss-form-group-title">@Title</div> }
    @ChildContent
    @if (!string.IsNullOrWhiteSpace(Error)) { <div class="my-field__error">@Error</div> }
    <button type="button" disabled="@(Disabled || ReadOnly)"
            @onclick="() => OnAddItemClicked.InvokeAsync()">+ Add</button>
</div>
```

```razor
@* MyListItem.razor *@
@using Orbyss.Blazor.JsonForms.Core.ComponentBases
@inherits FormListItemComponentBase

<div class="my-list__item">
    <div class="my-list__item-body">@ChildContent</div>
    <button type="button" disabled="@(Disabled || ReadOnly)"
            @onclick="() => OnRemoveItemClicked.InvokeAsync()">✕</button>
</div>
```

## ActionButton

Fires a handler registered with `options.RegisterAction(key, handler)`:

```razor
@* MyActionButton.razor *@
@using Orbyss.Blazor.JsonForms.Core.ComponentBases
@inherits FormActionButtonComponentBase

<button type="button" class="@Class orbyss-form-action-button"
        disabled="@Disabled" @onclick="() => OnClick.InvokeAsync()">@Label</button>
```

## Array

The array component is fully yours to render — there is no engine "add" hook to
intercept; you own the button and decide inline vs. dialog. Read
`ArrayContext.Items` fresh each render (don't cache).

**Inline** (edit fields directly in each row):

```razor
@* MyArrayInline.razor *@
@using Orbyss.Blazor.JsonForms.Core.ComponentBases
@using Orbyss.Blazor.JsonForms.Core.Context.Interfaces
@inherits FormArrayLayoutComponentBase

@if (ArrayContext is not null)
{
    <div class="orbyss-form-array">
        @foreach (var item in ArrayContext.Items)
        {
            var id = item.Id;
            <div class="orbyss-form-array-item">
                <div class="orbyss-form-array-item-controls">
                    <FormElementSelector Context="@item.ElementContext" />
                </div>
                <button type="button" @onclick="() => FormContext!.RemoveArrayItem(ArrayContext!.Id, id)">✕</button>
            </div>
        }
        <div class="orbyss-form-array-add-row">
            <button type="button" @onclick="() => FormContext!.AddArrayItem(ArrayContext!.Id)">@(AddLabel ?? "+")</button>
        </div>
    </div>
}

@code {
    [CascadingParameter] public IJsonFormContext? FormContext { get; set; }
}
```

`<FormElementSelector>` is provided by the engine package; it renders an item's
controls through the same factories. `FormArrayItemContext` exposes `Id`, `Index`,
and `ElementContext`.

**Dialog** (rows show a summary; add/edit happen in a modal hosting a scoped form).
Use the data API so item contexts stay in sync — never push array items via
`UpdateFormData`:

```razor
@* sketch — your dialog hosts a separate <JsonForm> built from the item sub-schema *@
private async Task OnAddClicked()
{
    var result = await ShowItemDialogAsync(itemJsonSchema, itemUiSchema, itemTranslations);
    if (result.Confirmed)
        FormContext!.AddArrayItem(ArrayContext!.Id, result.Data); // result.Data = scoped form's GetFormData()
}

private async Task OnEditClicked(FormArrayItemContext item)
{
    var seed   = FormContext!.GetArrayItemData(ArrayContext!.Id, item.Id);   // pre-fill
    var result = await ShowItemDialogAsync(itemJsonSchema, itemUiSchema, itemTranslations, seed);
    if (result.Confirmed)
        FormContext!.UpdateArrayItem(ArrayContext!.Id, item.Id, result.Data); // commit
}
```

`AddArrayItem(id, data)`, `UpdateArrayItem(id, itemId, data)`, and
`RemoveArrayItem` each re-run rules, raise data-changed, and fire the matching
event (`OnArrayItemAdded` / `OnArrayItemUpdated` / `OnArrayItemRemoved`). Seed and
replacement data are deep-cloned.

A simpler dialog approach that needs no new scoped form: `AddArrayItem(id)` to
create the empty row, render its `ElementContext` inside your modal, and call
`RemoveArrayItem` if the user cancels.

## Factory

A complete control factory tying it together:

```csharp
using Orbyss.Blazor.JsonForms.Core.ComponentFactory;
using Orbyss.Blazor.JsonForms.Core.Constants;
using Orbyss.Blazor.JsonForms.Core.Interpretation;

public sealed class MyControlFactory : ControlComponentFactory
{
    public MyControlFactory()
    {
        TextInputComponentType    = typeof(MyTextInput);
        NumberInputComponentType  = typeof(MyNumberInput);
        IntegerInputComponentType = typeof(MyNumberInput);
        BooleanInputComponentType = typeof(MyCheckbox);
        DropdownComponentType     = typeof(MyDropdown);
        MultiSelectComponentType  = typeof(MyMultiSelect);
        DateOnlyInputComponentType = typeof(MyDatePicker);

        SetParameter<MyTextInput,   string?>(x => x.Class, FormCssClasses.TextInput);
        SetParameter<MyNumberInput, string?>(x => x.Class, FormCssClasses.NumberInput);
        SetParameter<MyDropdown,    string?>(x => x.Class, FormCssClasses.Dropdown);
        SetParameter<MyCheckbox,    string?>(x => x.Class, FormCssClasses.BooleanInput);
        SetParameter<MyDatePicker,  string?>(x => x.Class, FormCssClasses.DateInput);

        RegisterAlias("slider", typeof(MySlider<double?>));
    }
}
```
