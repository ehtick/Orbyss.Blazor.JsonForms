# 📦 Orbyss.Blazor.JsonForms

**A fully .NET-native implementation of the [JsonForms.io](https://jsonforms.io) standard for schema-driven forms in Blazor.**
No Angular, no web components — just C#, JSON Schema, and a UI-agnostic Blazor architecture.

Render rich, validated, localised forms from three JSON documents — a data
schema, a layout schema, and a translation schema — and plug in **any** Blazor
component library to draw the inputs.

> 📖 **Importable skills** teach an AI assistant the exact, full capability set:
> [schema authoring](https://github.com/orbyss-io/Orbyss.Blazor.JsonForms/tree/main/skills/orbyss-jsonforms-schema-authoring)
> (generate JSON/UI/translation schemas) and
> [custom components](https://github.com/orbyss-io/Orbyss.Blazor.JsonForms/tree/main/skills/orbyss-jsonforms-custom-components)
> (build your own renderer + theming). Upgrading? See the
> [v1→v2 migration skill](https://github.com/orbyss-io/Orbyss.Blazor.JsonForms/tree/main/skills/orbyss-jsonforms-v1-migration).

---

## What you get

- ✅ Form generation from three schemas (data, layout, translations)
- ✅ Automatic control-type resolution from JSON Schema
- ✅ Localisation, validation, and conditional rules (Show / Hide / Enable / Disable)
- ✅ Multi-page (wizard) forms, inline array repeaters, list-with-detail, action buttons
- ✅ Inline **or dialog** array editing — driven entirely by your UI layer
- ✅ Theming through CSS custom properties — no component edits
- ✅ A clean, three-layer parameter-binding contract for custom components

This package is the **engine**. For building a UI layer against contracts only,
reference [`Orbyss.Blazor.JsonForms.Core`](https://www.nuget.org/packages/Orbyss.Blazor.JsonForms.Core).
Targets **net8.0** and **net10.0**.

### Ready-made UI integration

- 🧩 [Orbyss.Blazor.Syncfusion.JsonForms](https://www.nuget.org/packages/Orbyss.Blazor.Syncfusion.JsonForms) — full Syncfusion Blazor renderer, one `AddSyncfusionJsonForms()` call.

Or **roll your own** — see [Bring your own UI](#bring-your-own-ui).

---

## Installation

```bash
dotnet add package Orbyss.Blazor.JsonForms
```

Include the base stylesheet (provides the default theme):

```html
<link rel="stylesheet" href="_content/Orbyss.Blazor.JsonForms/orbyss-forms.css" />
```

Register services. With a UI integration package this is usually one call; with
your own components, assign the factory slots:

```csharp
builder.Services.AddJsonForms(configureFactories: o =>
{
    o.ConfigureControls = controls =>
    {
        controls.TextInputComponentType    = typeof(MyTextBox);
        controls.NumberInputComponentType  = typeof(MyNumberInput);
        controls.BooleanInputComponentType = typeof(MyCheckbox);
        controls.DropdownComponentType     = typeof(MyDropdown);
        controls.MultiSelectComponentType  = typeof(MyMultiSelect);
        controls.DateOnlyInputComponentType= typeof(MyDatePicker);
        // …assign the slots you use
    };
    o.ConfigureButtons      = b => b.SubmitButtonComponentType = typeof(MyButton);
    o.ConfigureActionButtons= a => a.ActionButtonComponentType = typeof(MyActionButton);
    o.ConfigureArrayLayout  = a => a.ArrayLayoutComponentType  = typeof(MyArrayRepeater);
});
```

---

## Quick start

```razor
<JsonForm InitOptions="@options" OnSubmit="HandleSubmit" />

@code {
    private JsonFormOptions options = new(
        jsonSchemaJson,
        uiSchemaJson,
        translationSchemaJson);

    private Task HandleSubmit(JToken formData) => Save(formData);
}
```

- `JsonFormOptions` accepts the three schemas as **strings** or parsed
  objects (`JSchema`, `FormUiSchema`, `TranslationSchema`), plus optional seed
  `Data`, initial `Language`, and `Disabled` / `ReadOnly`.
- The form context resolves from a `FormContext` `[Parameter]`, a **transient**
  DI registration, or the built-in builder.
- Cascading values `Language`, `Disabled`, `ReadOnly` are honoured.

---

## The three schemas

| Schema | Purpose |
|---|---|
| **JSON Schema** | Data structure — types, `enum`, `format`, constraints, `required`. Drives control-type resolution. |
| **UI Schema** | Layout, grouping, per-field options, conditional rules. |
| **Translation Schema** | Localised labels, helper text, enum display values, error messages. |

```json
// JSON Schema
{
  "type": "object",
  "properties": {
    "firstName": { "type": "string", "minLength": 2 },
    "role":      { "type": "string", "enum": ["admin", "user", "guest"] }
  },
  "required": ["firstName"]
}
```

```json
// UI Schema
{
  "type": "VerticalLayout",
  "elements": [
    { "type": "Control", "scope": "#/properties/firstName",
      "options": { "helperIconTextLabel": "firstName.helper" } },
    { "type": "Control", "scope": "#/properties/role" }
  ]
}
```

```json
// Translation Schema
{
  "resources": {
    "en": { "translation": {
      "firstName": { "label": "First Name", "error": { "minLength": "Min 2 characters" } },
      "role":      { "label": "Role" },
      "firstName.helper": { "label": "Enter the legal first name." }
    }}
  }
}
```

The data schema decides the control type automatically — `string` → text,
`string`+`enum` → dropdown, `number`/`integer` → numeric, `boolean` → checkbox,
`string`+`format:date` → date picker, array of string-enum → multi-select. Full
rules: [schema-authoring skill](https://github.com/orbyss-io/Orbyss.Blazor.JsonForms/tree/main/skills/orbyss-jsonforms-schema-authoring).

---

## UI-schema capabilities

| Feature | `type` / option |
|---|---|
| Layouts | `VerticalLayout`, `HorizontalLayout`, `Group` |
| Multi-page (wizard) | `Categorization` + `Category` |
| Conditional state | `rule` → `Show` / `Hide` / `Enable` / `Disable` |
| Inline action button | `ActionButton` + `options.actionKey` (+ `RegisterAction`) |
| Inline array repeater | `ArrayLayout` |
| List with detail editor | `ListWithDetail` + `options.detail` |
| Read-only / disabled / hidden | `options.readonly` / `disabled` / `hidden` |
| Helper text / tooltip | `options.helperTextLabel` / `helperIconTextLabel` |
| Numeric prefix / suffix | `options.prefixLabel` / `suffixLabel` |
| Enum item helper text | `options.enumItemOptions` |
| Per-field component override | `options.component` (alias) |
| Per-field Blazor parameters | `options.parameters` |
| CSS class | `options.cssClass` (append, or `!x` to replace) |

Each label-like option (`helperTextLabel`, `helperIconTextLabel`, `prefixLabel`,
`suffixLabel`, `addLabel`, plus `label`/`i18n`) is resolved as a translation key
first, then falls back to its literal string.

---

## Reacting to changes

Subscribe on the options object before passing it to `<JsonForm/>`:

```csharp
var options = new JsonFormOptions(jsonSchema, uiSchema, translationSchema);

options.OnControlValueChanged += async (control, form) =>
{
    if (control.AbsoluteDataJsonPath != "$.postcode") return;
    var address = await lookup.ByPostcodeAsync($"{form.GetValue(control.Id)}");
    var cityCtx = form.FindControl(c => c.AbsoluteDataJsonPath == "$.city");
    if (cityCtx is not null) form.UpdateValue(cityCtx.Id, JToken.FromObject(address.City));
};

options.RegisterAction("calculate", async form =>
{
    var ctx = form.FindControl(c => c.AbsoluteDataJsonPath == "$.total");
    if (ctx is not null) form.UpdateValue(ctx.Id, JToken.FromObject(Compute(form.GetFormData())));
});
```

Events: `OnControlValueChanged` (committed change), `OnControlInputChanged` (per
keystroke), and `OnArrayItemAdded` / `OnArrayItemRemoved` / `OnArrayItemMoved` /
`OnArrayItemUpdated`. All are multi-subscriber (`+=`). `UpdateValue` re-runs rules
and refreshes automatically.

**Array dialog editing** is the UI provider's job — the `ArrayLayout` component
owns its add/edit affordances. Land a dialog's result with
`form.AddArrayItem(id, itemData)` (create), `form.GetArrayItemData(id, itemId)` +
`form.UpdateArrayItem(id, itemId, itemData)` (edit), or `form.RemoveArrayItem(id,
itemId)`. See the
[custom-components skill](https://github.com/orbyss-io/Orbyss.Blazor.JsonForms/tree/main/skills/orbyss-jsonforms-custom-components).

---

## Theming

Override CSS custom properties — no component code:

```css
:root {
  --orbyss-form-primary:       #e91e63;
  --orbyss-form-error:         #c0392b;
  --orbyss-form-border-radius: 8px;
  --orbyss-form-spacing:       1.25rem;
}
```

Variable and class names are also C# constants (`FormCssVariables`,
`FormCssClasses`). Append a class with `"cssClass": "x"`, replace the default
with `"cssClass": "!x"`. Full catalogue:
[custom-components skill](https://github.com/orbyss-io/Orbyss.Blazor.JsonForms/tree/main/skills/orbyss-jsonforms-custom-components).

---

## Bring your own UI

The engine is completely UI-agnostic. Plugging in your own component library takes
two things: **components** and **factories**.

### Components — three rules

1. **Inherit `FormInputComponentBase<TValue>`** — you get `IFormComponent` and all
   standard engine parameters (`Value`, `Disabled`, `ReadOnly`, `Class`,
   `ErrorHelperText`, `Label`, …) for free.
2. **Invoke `OnValueChanged`** on commit — that's how state flows back into the form.
3. **Don't declare engine-owned parameters** (`Value`, `ValueChanged`, `Checked`, …)
   — the engine wires them automatically.

```razor
@* MyTextBox.razor *@
@inherits FormInputComponentBase<string?>

<input value="@Value" class="@Class" disabled="@(Disabled || ReadOnly)"
       @onchange="e => OnValueChanged.InvokeAsync(e.Value?.ToString())" />

@if (!string.IsNullOrWhiteSpace(ErrorHelperText)) { <div class="err">@ErrorHelperText</div> }
```

### Factories — assign your components to slots

Extend the factory base classes and assign component types in the constructor:

```csharp
public class MyControlFactory : ControlComponentFactory
{
    public MyControlFactory()
    {
        TextInputComponentType    = typeof(MyTextBox);
        NumberInputComponentType  = typeof(MyNumberInput);
        IntegerInputComponentType = typeof(MyIntegerInput);
        BooleanInputComponentType = typeof(MyCheckbox);
        DropdownComponentType     = typeof(MyDropdown);
        MultiSelectComponentType  = typeof(MyMultiSelect);
        DateOnlyInputComponentType = typeof(MyDatePicker);

        RegisterAlias("toggle", typeof(MyToggle));   // custom variant via options.component
    }
}
```

Register all factories as **transient**, then call `AddJsonForms()`:

```csharp
services.AddTransient<IControlComponentFactory>(_ => new MyControlFactory());
services.AddTransient<IButtonComponentFactory>(_ => new MyButtonFactory());
services.AddTransient<INavigationComponentFactory>(_ => new MyNavigationFactory());
services.AddTransient<IListComponentFactory>(_ => new MyListFactory());
services.AddTransient<IActionButtonComponentFactory>(_ => new MyActionButtonFactory());
services.AddTransient<IArrayLayoutComponentFactory>(_ => new MyArrayLayoutFactory());
services.AddJsonForms();
```

Wrap it all in an `AddMyJsonForms()` extension method and you have a complete,
self-contained UI integration package.

### Per-form factory configuration

Factories are transient — one set per `<JsonForm>` — so you can override any slot
or parameter for a single form without touching the application defaults:

```razor
<JsonForm InitOptions="@options" OnSubmit="HandleSubmit"
          ConfigureFactories="@(f => f.ConfigureControls = c =>
              c.BooleanInputComponentType = typeof(MySwitch))" />
```

### Reference implementation

[`Orbyss.Blazor.Syncfusion.JsonForms`](https://github.com/orbyss-io/Orbyss.Blazor.Syncfusion.JsonForms)
is a complete, production-ready example of exactly this pattern — six factory
classes, a `AddSyncfusionJsonForms()` extension, default translations, and a
stylesheet. Use it as a blueprint.

The complete contract — auto-wired parameters per slot, `JToken` conversion,
aliases, multi-page navigation, lists, arrays, and theming — is in the
[custom-components skill](https://github.com/orbyss-io/Orbyss.Blazor.JsonForms/tree/main/skills/orbyss-jsonforms-custom-components).

---

## License

MIT License — © Orbyss.

## Links

- 🌍 [orbyss.io](https://orbyss.io)
- 🧑‍💻 [GitHub](https://github.com/orbyss-io/Orbyss.Blazor.JsonForms) · Skills: [schema authoring](https://github.com/orbyss-io/Orbyss.Blazor.JsonForms/tree/main/skills/orbyss-jsonforms-schema-authoring) · [custom components](https://github.com/orbyss-io/Orbyss.Blazor.JsonForms/tree/main/skills/orbyss-jsonforms-custom-components) · [v1→v2 migration](https://github.com/orbyss-io/Orbyss.Blazor.JsonForms/tree/main/skills/orbyss-jsonforms-v1-migration)
- 📦 [Core package](https://www.nuget.org/packages/Orbyss.Blazor.JsonForms.Core)
- 🔌 [Syncfusion UI](https://www.nuget.org/packages/Orbyss.Blazor.Syncfusion.JsonForms) · [Syncfusion source](https://github.com/orbyss-io/Orbyss.Blazor.Syncfusion.JsonForms)
- 📐 [JsonForms.io](https://jsonforms.io/)
