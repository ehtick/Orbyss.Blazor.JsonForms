---
name: orbyss-jsonforms-custom-components
description: >
  Build custom Blazor components and a UI layer for the Orbyss.Blazor.JsonForms
  schema-driven form engine — with no third-party UI dependency (plain HTML/CSS,
  or your own component set). Use this whenever the user is working with
  Orbyss.Blazor.JsonForms / Orbyss.Blazor.JsonForms.Core and wants to render form
  fields with their own components, implement IFormComponentFactory or a
  *ComponentFactory, inherit FormInputComponentBase, wire a control/button/list/
  array/action-button/navigation component, fix "no component registered for
  control type" errors, apply their own theme to the form, or replace
  Syncfusion/MudBlazor with bespoke components. Trigger even if they don't name
  the package but mention a "json forms" custom input, control factory, or
  ValueChanged/ConvertFromJToken not firing.
---

# Building custom components for Orbyss.Blazor.JsonForms

This skill helps you build a **UI layer** for the Orbyss JSON Forms engine using
**your own Blazor components** — no MudBlazor, no Syncfusion, no third-party
dependency required. The engine decides *what* each field should be (a text box, a
single-select, a date picker, an inline array…) and *with what values*; you supply
the components that draw it and apply your own theme.

If the task is instead about writing the JSON/UI/translation **schemas**, use the
`orbyss-jsonforms-schema-authoring` skill.

## Mental model (read first)

- The form is produced from three schemas (data / UI / translation). The engine
  resolves each field to a **`ControlType`** and looks up the component **type**
  you registered for that type's **factory slot**.
- Your UI layer is a set of **factories**. You almost never implement them from
  scratch — you subclass the provided defaults and assign component types to slots.
- Every data-bound input component **must implement `IFormComponent`**. Inheriting
  `FormInputComponentBase<TValue>` does this for you and pre-declares every
  engine-wired parameter.
- Binding flows back to the form when your component invokes **`OnValueChanged`**.

Everything you reference lives under `Orbyss.Blazor.JsonForms.Core.*` (contracts)
and the engine is in `Orbyss.Blazor.JsonForms` (`AddJsonForms`, `<JsonForm>`).

## Setup

```bash
dotnet add package Orbyss.Blazor.JsonForms        # full engine (app host)
# or, for a redistributable UI-layer library:
dotnet add package Orbyss.Blazor.JsonForms.Core   # contracts only
```

Register your UI layer, then the engine. Register sub-factories as **transient**
(see [why](#per-form-configuration)) and *before* `AddJsonForms` — the engine only
supplies defaults via `TryAdd`, so yours win:

```csharp
services.AddTransient<IControlComponentFactory>(_ => new MyControlFactory());
services.AddTransient<IButtonComponentFactory>(_ => new MyButtonFactory());
services.AddTransient<INavigationComponentFactory>(_ => new MyNavigationFactory());
services.AddTransient<IListComponentFactory>(_ => new MyListFactory());
services.AddTransient<IActionButtonComponentFactory>(_ => new MyActionButtonFactory());
services.AddTransient<IArrayLayoutComponentFactory>(_ => new MyArrayLayoutFactory());
services.AddJsonForms();
```

You can also skip custom factory classes and configure the bundled defaults inline
via `AddJsonForms(configureFactories: o => o.ConfigureControls = c => { … })` —
these become the **application defaults** every form starts from.

Only assign the slots you actually use; an unregistered slot throws a clear
"No component registered for …" error the first time that element renders.

## The factory slots

| Sub-factory (subclass the default) | Slots (assign `…ComponentType`) | Component must inherit |
|---|---|---|
| `ControlComponentFactory` | `TextInput`, `NumberInput`, `IntegerInput`, `BooleanInput`, `Dropdown`, `MultiSelect`, `DateTimeInput`, `DateTimeUtcTicksInput`, `DateOnlyInput`, `DateOnlyUtcTicksInput` | `FormInputComponentBase<TValue>` (implements `IFormComponent`) |
| `ButtonComponentFactory` | `SubmitButtonComponentType`, `NextButtonComponentType`, `PreviousButtonComponentType` | `FormButtonComponentBase` |
| `NavigationComponentFactory` | `NavigationComponentType` | `FormNavigationComponentBase` |
| `ListComponentFactory` | `ListComponentType`, `ListItemComponentType` | `FormListComponentBase` / `FormListItemComponentBase` |
| `ActionButtonComponentFactory` | `ActionButtonComponentType` | `FormActionButtonComponentBase` |
| `ArrayLayoutComponentFactory` | `ArrayLayoutComponentType` | `FormArrayLayoutComponentBase` |

`ControlType` → CLR value type your input must bind:

| `ControlType` | `Value` type | callback |
|---|---|---|
| `String` | `string?` | `EventCallback<string?>` |
| `Boolean` | `bool` | `EventCallback<bool>` |
| `Integer` | `int?` | `EventCallback<int?>` |
| `Number` | `double?` | `EventCallback<double?>` |
| `Enum` | `string` | `EventCallback<string>` |
| `EnumList` | `IEnumerable<string>` | `EventCallback<IEnumerable<string>>` |
| `DateTime` | `DateTime?` | `EventCallback<DateTime?>` |
| `DateOnly` | `DateOnly?` | `EventCallback<DateOnly?>` |
| `DateTimeUtcTicks` | `DateTimeUtcTicks?` | `EventCallback<DateTimeUtcTicks?>` |
| `DateOnlyUtcTicks` | `DateUtcTicks?` | `EventCallback<DateUtcTicks?>` |

## Rules you must not violate

These either throw at registration/render time or silently break binding. Each
exists for a concrete reason — keep them in mind rather than memorising them rotely:

1. **Input components implement `IFormComponent`.** The factory validates this when
   you assign a slot, so a wrong type fails immediately, not at render. Inheriting
   `FormInputComponentBase<TValue>` satisfies it for free.
2. **Declare `Value` of the right CLR type and invoke `OnValueChanged` on commit.**
   `OnValueChanged` is how a committed value flows back into the form, re-runs
   rules, and clears the error. If you never invoke it, the form never updates.
3. **Never set the engine-owned parameters** `Value`, `ValueChanged`, `Checked`,
   `CheckedChanged`, `Values`, `ValuesChanged` via `SetParameter` or the UI-schema
   `parameters` option — the engine wires value binding itself, and setting them
   throws.
4. **Only parameters you declare with `[Parameter]` reach your component.** The
   factory strips unknown keys before rendering (Blazor rejects unknown parameters
   on a `DynamicComponent`). So if you want `PrefixText`, `HelperText`,
   `OnInputChanged`, etc., declare them — otherwise they're silently dropped.

## Building an input component (full, no dependencies)

Inherit `FormInputComponentBase<TValue>` and bind `Value` + `OnValueChanged`.
This is a complete, dependency-free text input that applies your theme via the
`--orbyss-form-*` CSS variables and the standard CSS class hooks:

```razor
@* MyTextInput.razor *@
@using Orbyss.Blazor.JsonForms.Core.ComponentBases
@inherits FormInputComponentBase<string?>

<label class="my-field">
    @if (!string.IsNullOrWhiteSpace(Label))
    {
        <span class="my-field__label">
            @Label
            @if (!string.IsNullOrWhiteSpace(HelperIconText))
            {
                <span class="my-field__icon" title="@HelperIconText">&#9432;</span>
            }
        </span>
    }

    <input class="@Class"
           value="@Value"
           disabled="@Disabled"
           readonly="@ReadOnly"
           @oninput="e => OnInputChanged.InvokeAsync(e.Value?.ToString())"
           @onchange="e => OnValueChanged.InvokeAsync(e.Value?.ToString())" />

    @if (!string.IsNullOrWhiteSpace(ErrorHelperText))
    {
        <span class="my-field__error">@ErrorHelperText</span>
    }
    else if (!string.IsNullOrWhiteSpace(HelperText))
    {
        <span class="my-field__help">@HelperText</span>
    }
</label>
```

Register it on your control factory and give it the default class hook:

```csharp
public sealed class MyControlFactory : ControlComponentFactory
{
    public MyControlFactory()
    {
        TextInputComponentType = typeof(MyTextInput);
        SetParameter<MyTextInput, string?>(x => x.Class, FormCssClasses.TextInput); // "orbyss-form-text-input"
        // …assign the other slots…
    }
}
```

### Parameters the engine auto-wires onto an input

Declared for you on `FormInputComponentBase<TValue>` — use the ones you render,
ignore the rest:

`Value`, `ValueChanged`, `OnValueChanged` (commit), `OnInputChanged` (per
keystroke), `Disabled`, `ReadOnly`, `Label`, `Class`, `Style`, `Culture`,
`HelperText`, `HelperIconText`, `ErrorHelperText`, and — for `Enum`/`EnumList`
controls — `Items` (`IEnumerable<TranslatedEnumItem>`) and `MultiSelect`. Numeric
controls also receive `PrefixText`/`SuffixText` when declared.

### Custom value conversion

`FormInputComponentBase<TValue>.ConvertFromJToken` defaults to
`JToken.ToObject<TValue>()`. Override it for types that need custom parsing — e.g.
a date-only picker stored as `"yyyy-MM-dd"`:

```csharp
public sealed class MyDatePicker : FormInputComponentBase<DateOnly?>
{
    public override object? ConvertFromJToken(JToken? token)
        => token is null || token.Type == JTokenType.Null
            ? null
            : DateOnly.ParseExact(token.ToString(), "yyyy-MM-dd");
}
```

(The engine converts your committed value back to JSON automatically, handling
`DateOnly`, `DateTime`, `DateTimeUtcTicks`, `DateUtcTicks` specially.)

## How parameters are applied (precedence)

When the factory creates a control, parameters are written in three layers,
**last write wins**, then unknown keys are stripped:

1. **Engine auto-wire** — `Value`, `Label`, `Disabled`, … (above).
2. **Factory `SetParameter`** — your static defaults, e.g. a CSS class or width.
3. **UI-schema `parameters` option** — per-field overrides authored in the schema.

So `SetParameter<MyTextInput,string?>(x => x.Class, FormCssClasses.TextInput)` sets
the default class; a field's `"options": { "parameters": { "Class": "!special" } }`
overrides it for that field.

## Theming the standard way

Apply your brand through the shared contract — no per-component hardcoding:

- **CSS variables** (`FormCssVariables`): style your components with
  `var(--orbyss-form-primary)`, `var(--orbyss-form-error)`,
  `var(--orbyss-form-border-radius)`, `var(--orbyss-form-spacing)`,
  `var(--orbyss-form-input-height)`, the title variables, etc. Consumers retheme by
  overriding these variables — your components inherit the change for free.
- **Class hooks** (`FormCssClasses`): assign the matching default class to each
  slot via `SetParameter` (`TextInput`, `NumberInput`, `Dropdown`, `BooleanInput`,
  `DateInput`, `Slider`, `EnumBlocks`, `ActionButton`). The engine merges a
  field's UI-schema `cssClass` on top (append, or `!x` to replace).
- **Layout & titles** are emitted by the engine as plain `<div>`s with
  `orbyss-form-row` / `orbyss-form-column` / `orbyss-form-page-title` /
  `orbyss-form-group-title` — style those in your stylesheet too.

Ship a stylesheet that defines the variables and styles the class hooks; document
the variables so consumers can override them. (See the engine's
`orbyss-forms.css` for the variable set and defaults.)

## Selecting components from the schema

- **Aliases** — register `RegisterAlias("slider", typeof(MySlider))` so a field
  with `"options": { "component": "slider" }` uses it. When one alias must map to
  different components per control type (e.g. integer vs number slider), override
  `ResolveComponentType(string? alias, ControlType controlType)` instead of a flat
  alias.
- **Per-field parameters** — `"options": { "parameters": { "Step": 5 } }` sets any
  declared `[Parameter]` (case-insensitive); engine-owned keys are rejected.

## Per-form configuration

Component registration is **per form**, not global. Sub-factories are registered
**transient**, so every `<JsonForm>` resolves its own instances. That lets one
form override the defaults without touching any other form — pass a
`ConfigureFactories` delegate that runs the same `FormComponentFactoryOptions`
pipeline (slot types, `SetParameter`, `RegisterAlias`) on *that form's* factories,
layered on top of the application defaults (per-form wins):

```razor
<JsonForm InitOptions="@options"
          ConfigureFactories="@(f =>
          {
              f.ConfigureControls = c =>
              {
                  c.TextInputComponentType = typeof(MySpecialTextInput);       // override a slot for THIS form
                  c.SetParameter<MySpecialTextInput, string?>(x => x.Variant, "compact");
                  c.RegisterAlias("rating", typeof(StarRating));               // form-local alias
              };
              f.ConfigureButtons = b => b.SubmitButtonComponentType = typeof(BigSubmitButton);
          })" />
```

Use this for screen-specific tweaks (a compact variant on one page, a one-off
custom control, a different submit button) without creating a separate UI layer.
Notes:

- The application defaults set in `AddJsonForms(configureFactories: …)` apply
  first; `ConfigureFactories` on `<JsonForm>` layers on top and overwrites.
- This requires the registered factory to derive from the configurable base
  (`ControlComponentFactory`, etc.) — the bundled defaults and any subclass do. A
  factory implementing the interface directly can't be configured this way; build
  it fully and pass `ComponentFactory` to `<JsonForm>` instead.
- For *full* manual control of one form, pass a ready
  `IFormComponentFactory` as `ComponentFactory` — it bypasses DI and
  `ConfigureFactories` entirely.

## Non-input slots and the array dialog

Buttons, navigation (stepper), list/list-item, action button, and the array
layout each have a base class and are wired the same way. The array layout is
**fully provider-rendered** — you decide inline vs. dialog editing and call the
form's data API to land changes:

```csharp
form.AddArrayItem(arrayId);                          // inline add (empty row)
form.AddArrayItem(arrayId, itemJToken);              // dialog add (seeded)
form.UpdateArrayItem(arrayId, itemId, itemJToken);   // dialog edit (replace)
JToken? data = form.GetArrayItemData(arrayId, itemId); // pre-fill an edit dialog
form.RemoveArrayItem(arrayId, itemId);
form.MoveArrayItem(arrayId, fromIndex, toIndex);     // drag reorder
```

Full, copy-pasteable component recipes for **every** slot (dropdown, checkbox,
date, slider via alias, submit button, stepper navigation, list + list-item,
action button, and an array layout with both inline and dialog modes) are in
[`references/component-recipes.md`](references/component-recipes.md). Read it when
you need a slot beyond the text input above.

## Build checklist

- [ ] Each input inherits `FormInputComponentBase<TValue>` with the correct `TValue`.
- [ ] Each input binds `Value` and invokes `OnValueChanged` on commit (and `OnInputChanged` if you want live input).
- [ ] No engine-owned parameter is set anywhere.
- [ ] Every parameter you render is declared `[Parameter]`.
- [ ] Each slot has a default CSS class via `SetParameter` and styles use `--orbyss-form-*` variables.
- [ ] Enum components declare `Items` + `MultiSelect`; numeric declare `PrefixText`/`SuffixText` if shown.
- [ ] Sub-factories registered **transient**, before `AddJsonForms` (enables per-form overrides).
- [ ] `dotnet build` is clean and a sample form renders + round-trips values.
