# 📦 Orbyss.Blazor.JsonForms

**A fully .NET-native implementation of the [JsonForms.io](https://jsonforms.io) standard for schema-driven forms in Blazor.**
No Angular, no web components — just C#, JSON Schema, and a flexible, UI-agnostic Blazor architecture.

![NuGet](https://img.shields.io/nuget/v/Orbyss.Blazor.JsonForms)
![NuGet Downloads](https://img.shields.io/nuget/dt/Orbyss.Blazor.JsonForms)

---

## 🎯 What is this?

Render rich, validated, localised forms from **three JSON documents** — a data
schema, a layout schema, and a translation schema — and plug in **any** Blazor
component library (or your own components) to draw the actual inputs.

The engine resolves *what* to render (a text box, a single-select dropdown, a date
picker, an inline array repeater…) and *with what values*; you supply the
components. It carries no dependency on any UI toolkit.

- ✅ Form generation from three schemas (data, layout, translations)
- ✅ Automatic control-type resolution from JSON Schema
- ✅ Localisation, validation, and conditional rules (Show / Hide / Enable / Disable)
- ✅ Multi-page (wizard) forms, inline array repeaters, list-with-detail, action buttons
- ✅ Inline **or dialog** array editing — driven entirely by your UI layer
- ✅ Theming through CSS custom properties — no component edits
- ✅ A clean, three-layer parameter-binding contract for custom components

---

## 🧠 The two things you'll do

Almost everything users do with this library is one of two tasks — and each has a
ready-to-import **skill** that teaches an AI assistant to do it well:

| You want to… | Skill to import |
|---|---|
| **Generate the schemas** for a form (fields, layout, pages, rules, dropdowns, dates, helper text, action buttons, arrays, lists, translations) | [`skills/orbyss-jsonforms-schema-authoring`](./skills/orbyss-jsonforms-schema-authoring) |
| **Build your own components** (no MudBlazor/Syncfusion needed) and apply your own theme the standard way | [`skills/orbyss-jsonforms-custom-components`](./skills/orbyss-jsonforms-custom-components) |
| **Upgrade from v1 to v2** | [`skills/orbyss-jsonforms-v1-migration`](./skills/orbyss-jsonforms-v1-migration) |

Drop a skill folder into your assistant's `.claude/skills/` and it gains the full,
exact capability set of this library (not the generic jsonforms.io spec).

---

## 📦 The two packages

| Package | Use it to… | Depends on |
|---|---|---|
| **[`Orbyss.Blazor.JsonForms`](https://www.nuget.org/packages/Orbyss.Blazor.JsonForms)** | Host a form in your app (the full engine: Razor components, DI, interpreter, CSS). | `Orbyss.Blazor.JsonForms.Core` |
| **[`Orbyss.Blazor.JsonForms.Core`](https://www.nuget.org/packages/Orbyss.Blazor.JsonForms.Core)** | Build a **UI layer** against contracts only (interfaces, context & interpretation models, the component factory, base classes). | — |

Both target **net8.0** and **net10.0**.

### Ready-made UI integration

- 🧩 [Orbyss.Blazor.Syncfusion.JsonForms](https://www.nuget.org/packages/Orbyss.Blazor.Syncfusion.JsonForms) — full Syncfusion Blazor renderer, one `AddSyncfusionJsonForms()` call.

Or **roll your own** for Radzen, Telerik, Fluent UI, plain HTML/CSS, or anything else — it takes one factory class per slot and a DI registration. See [Bring your own UI](#%EF%B8%8F-bring-your-own-ui) below, or use the Syncfusion package as a reference implementation.

---

## 🚀 Quick start

```bash
dotnet add package Orbyss.Blazor.JsonForms
```

Include the base stylesheet (optional — it provides the default theme):

```html
<link rel="stylesheet" href="_content/Orbyss.Blazor.JsonForms/orbyss-forms.css" />
```

Register services (a UI integration package does the factory wiring for you):

```csharp
builder.Services.AddJsonForms(configureFactories: o =>
{
    o.ConfigureControls = controls =>
    {
        controls.TextInputComponentType    = typeof(MyTextBox);
        controls.NumberInputComponentType  = typeof(MyNumberInput);
        controls.BooleanInputComponentType = typeof(MyCheckbox);
        controls.DropdownComponentType     = typeof(MyDropdown);
        // …assign the slots you use
    };
    o.ConfigureButtons = b => b.SubmitButtonComponentType = typeof(MyButton);
});
```

Render:

```razor
<JsonForm InitOptions="@options" OnSubmit="HandleSubmit" />

@code {
    private JsonFormOptions options = new(jsonSchemaJson, uiSchemaJson, translationSchemaJson);
    private Task HandleSubmit(JToken formData) => Save(formData);
}
```

- `JsonFormOptions` takes the three schemas as **strings** or parsed objects
  (`JSchema`, `FormUiSchema`, `TranslationSchema`), plus optional seed `Data`,
  initial `Language`, and `Disabled` / `ReadOnly`.
- The form context and component factory resolve from `[Parameter]`s or DI.
- Cascading values `Language`, `Disabled`, and `ReadOnly` are honoured.

---

## 🔄 How it works

Forms are driven by three schemas:

| Schema | Purpose |
|---|---|
| **JSON Schema** | Data structure — types, `enum`, `format`, constraints, `required`. Drives control-type resolution. |
| **UI Schema** | Layout, grouping, per-field options, and conditional rules. |
| **Translation Schema** | Localised labels, helper text, enum display values, and error messages. |

The data schema decides the **control type** automatically:

| JSON Schema | Control |
|---|---|
| `string` | text input |
| `string` + `enum` | single-select dropdown |
| `string` + `format: date` / `datetime` | date / datetime picker |
| `number` / `integer` | numeric input |
| `boolean` | checkbox / switch |
| `array` of `string` + `enum` | multi-select |

The **schema-authoring skill** knows every option and generates all three schemas
from a plain description of the form you want.

---

## 🧱 What you can put in a form (UI schema)

| Capability | `type` / option |
|---|---|
| Vertical / horizontal layouts | `VerticalLayout`, `HorizontalLayout`, `Group` |
| Multi-page (wizard) forms | `Categorization` + `Category` |
| Data-bound fields | `Control` (`scope`) |
| Conditional visibility / state | `rule` → `Show` / `Hide` / `Enable` / `Disable` |
| Read-only / disabled / hidden | `options.readonly` / `disabled` / `hidden` |
| Helper text & tooltips | `options.helperTextLabel` / `helperIconTextLabel` |
| Numeric prefix / suffix | `options.prefixLabel` / `suffixLabel` |
| Enum item helper text | `options.enumItemOptions` |
| Inline action buttons | `ActionButton` + `options.actionKey` (+ `RegisterAction`) |
| Inline array repeaters (add/remove/reorder) | `ArrayLayout` |
| List-with-detail editor | `ListWithDetail` + `options.detail` |
| Per-field custom component | `options.component` (alias) |
| Per-field Blazor parameters | `options.parameters` |
| Custom CSS class | `options.cssClass` (append, or `!x` to replace) |
| Value-change side-effects | `OnControlValueChanged` / `OnControlInputChanged` |
| Array lifecycle hooks | `OnArrayItemAdded` / `Removed` / `Moved` / `Updated` |

Full authoring rules, the exact options catalogue, and worked examples are in the
[schema-authoring skill](./skills/orbyss-jsonforms-schema-authoring).

---

## 🎨 Theming

Theme the whole form by overriding CSS custom properties — no component code:

```css
:root {
  --orbyss-form-primary:       #e91e63;
  --orbyss-form-error:         #c0392b;
  --orbyss-form-border-radius: 8px;
  --orbyss-form-spacing:       1.25rem;   /* all gaps follow unless individually set */
}
```

All variables and class names are also exposed as C# constants (`FormCssVariables`,
`FormCssClasses`). Page/group titles render as neutral `<div>`s
(`orbyss-form-page-title` / `orbyss-form-group-title`) you style freely. The
custom-components skill covers applying your own theme the standard way.

---

## 🛠 Bring your own UI — in three rules

1. **Input components implement `IFormComponent`** — inherit
   `FormInputComponentBase<TValue>` to get it (and all standard parameters) free.
2. **Declare `Value` and invoke `OnValueChanged`** on commit — that's how state
   flows back into the form.
3. **Don't touch engine-owned parameters** (`Value`/`ValueChanged`/…) — the engine
   wires binding automatically.

```razor
@inherits FormInputComponentBase<string?>
<input value="@Value" class="@Class" disabled="@(Disabled || ReadOnly)"
       @onchange="e => OnValueChanged.InvokeAsync(e.Value?.ToString())" />
```

Component registration is **per form**: factories are transient, and a single form
can override a slot, add parameters, or register an alias via
`<JsonForm ConfigureFactories="…">` — layered on top of the application defaults,
without affecting other forms.

The [custom-components skill](./skills/orbyss-jsonforms-custom-components) has full,
dependency-free recipes for every slot (inputs, dropdowns, dates, sliders, buttons,
stepper navigation, list, action button, and an array layout with inline **and**
dialog editing), plus the factory model, the three-layer parameter precedence,
per-form configuration, and theming.

---

## 🏗 Building & testing

```bash
# from the repository root
dotnet build Orbyss.Blazor.JsonForms.slnx
dotnet test  tests/Orbyss.Blazor.JsonForms.Tests.csproj
```

The solution contains the `forms/` engine, the `core/` contracts library, and the
`tests/` project. The engine references `core/` by project reference locally and by
package reference when `UseNuGetForCore=true`.

---

## 📚 Documentation map

| Where | For |
|---|---|
| **This README** | Overview, install, quick start, capability map. |
| **[skills/orbyss-jsonforms-schema-authoring](./skills/orbyss-jsonforms-schema-authoring)** | Generating JSON / UI / translation schemas — every option, with examples. |
| **[skills/orbyss-jsonforms-custom-components](./skills/orbyss-jsonforms-custom-components)** | Building your own components + UI layer, theming, the binding contract. |
| **[skills/orbyss-jsonforms-v1-migration](./skills/orbyss-jsonforms-v1-migration)** | Upgrading a v1 codebase to v2. |
| **[forms/README.md](./forms/README.md)** · **[core/README.md](./core/README.md)** | The NuGet package readmes. |

---

## 📄 License

MIT License — © Orbyss. See [LICENSE](./forms/LICENSE).

## 🔗 Links

- 🌍 Website: [https://orbyss.io](https://orbyss.io)
- 📦 NuGet: [Orbyss.Blazor.JsonForms](https://www.nuget.org/packages/Orbyss.Blazor.JsonForms) · [Orbyss.Blazor.JsonForms.Core](https://www.nuget.org/packages/Orbyss.Blazor.JsonForms.Core)
- 🧑‍💻 GitHub: [orbyss-io/Orbyss.Blazor.JsonForms](https://github.com/orbyss-io/Orbyss.Blazor.JsonForms)
- 🔌 UI integrations: [Syncfusion](https://www.nuget.org/packages/Orbyss.Blazor.Syncfusion.JsonForms) · [MudBlazor](https://www.nuget.org/packages/Orbyss.Blazor.MudBlazor.JsonForms)
- 📐 Standard: [JsonForms.io](https://jsonforms.io/)

## 🤝 Contributing

Contributions are welcome — bug fixes, improvements, documentation, or ideas.
Fork the repo, create a branch, and open a pull request.

- Write clean, readable code
- Keep PRs focused and descriptive
- Update the relevant **skill** in `skills/` in the same PR when engine behaviour changes
- Open issues for larger features or discussions

---

⭐️ If you find this useful, [give us a star](https://github.com/orbyss-io/Orbyss.Blazor.JsonForms/stargazers) and help spread the word!
