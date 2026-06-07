---
name: orbyss-jsonforms-schema-authoring
description: >
  Author the three schemas that drive an Orbyss.Blazor.JsonForms form — the JSON
  (data) schema, the UI schema, and the translation schema — from a description of
  the form the user wants. Use this whenever someone wants to create, edit, or
  debug Orbyss JSON Forms schemas: lay out fields, add sections/pages/wizards,
  add conditional show/hide/enable/disable rules, configure dropdowns/multi-select/
  dates/numbers, add helper text/tooltips/prefixes, build inline array repeaters or
  list-with-detail, add action buttons, localise labels and validation messages, or
  pick custom components via options.component/options.parameters/options.cssClass.
  Trigger on "json forms schema", "uischema", "form schema", "scope is null",
  "make a form with…", "add a wizard/stepper", or "conditional field" in an Orbyss
  forms context — even if the package isn't named explicitly.
---

# Authoring Orbyss.Blazor.JsonForms schemas

This skill generates and edits the **three schemas** that produce an Orbyss form.
You do not need a running app — you produce correct JSON. If the task is instead
about building Blazor **components** for the renderer, use the
`orbyss-jsonforms-custom-components` skill.

> ⚠️ This is **not** the generic jsonforms.io spec. The capabilities below are
> exactly what `Orbyss.Blazor.JsonForms` supports — no more, no less.

## The three schemas

| Schema | Decides |
|---|---|
| **JSON Schema** | The data shape — property types, `enum`, `format`, constraints, `required`. **This determines each field's control type.** |
| **UI Schema** | Layout, grouping, pages, per-field `options`, and conditional `rule`s. |
| **Translation Schema** | Labels, helper text, enum display values, and validation messages, per language. |

## Workflow

1. **Understand the data.** Turn the user's description into a JSON Schema
   (properties, types, `enum`s, `format`s, `required`, numeric/array constraints).
   The data schema is the source of truth for field types.
2. **Check for a UI layer / custom components.** The available components, the
   `component` aliases (e.g. `"slider"`, `"blocks"`), valid `parameters`, and CSS
   classes depend on the user's UI layer (Syncfusion, MudBlazor, or their own). If
   the user wants anything beyond a plain control — a slider, an enum-block
   selector, a dialog-mode array, a specific CSS class — **ask which UI layer they
   use and what aliases/parameters/classes it exposes** before emitting
   `options.component` / `options.parameters` / `options.cssClass`. See
   [When to ask the user](#when-to-ask-the-user).
3. **Lay out the UI Schema** — layouts, optional pages, controls, rules,
   action buttons, arrays, lists.
4. **Write the Translation Schema** for each language — labels, errors, enum
   labels, and any helper/prefix keys referenced from the UI schema.
5. **Sanity-check** against the [gotchas](#gotchas).

## JSON Schema → control type

Each `Control` resolves its component from the JSON Schema at its `scope`:

| JSON Schema at the scope | Control |
|---|---|
| `string` | text input |
| `string` + `enum` | single-select dropdown (`Enum`) |
| `string` + `format: "date"` | date picker (`DateOnly`) |
| `string` + `format: "datetime"` | datetime picker (`DateTime`) |
| `integer` | integer input |
| `number` | number input |
| `number` + `format: "date"` / `"datetime"` | date/datetime stored as UTC ticks |
| `boolean` | checkbox / switch |
| `array` of `string` **with `enum`** | multi-select (`EnumList`) |
| `object` / `array` of objects | not a single control — use `Group`/`ArrayLayout`/`ListWithDetail` |

`minimum`/`maximum` (numbers) and `minItems`/`maxItems` (arrays) are read for
validation and component config. `required` lives on the **parent object**.

```json
{
  "type": "object",
  "properties": {
    "firstName": { "type": "string", "minLength": 2, "maxLength": 50 },
    "role":      { "type": "string", "enum": ["admin", "user", "guest"] },
    "birthDate": { "type": "string", "format": "date" },
    "premium":   { "type": "number", "minimum": 0 },
    "tags":      { "type": "array", "items": { "type": "string", "enum": ["a","b","c"] } }
  },
  "required": ["firstName"]
}
```

## UI Schema element types

| `type` | Use |
|---|---|
| `VerticalLayout` | Stack children top-to-bottom. Needs `elements`. |
| `HorizontalLayout` | Place children side-by-side. Needs `elements`. |
| `Group` | Like `VerticalLayout`, with an optional title `label`. |
| `Categorization` | **Multi-page / wizard.** Direct children must all be `Category`. |
| `Category` | One page; its `label` is the page title. |
| `Control` | One data-bound field. Needs `scope`. No children. |
| `ListWithDetail` | Repeating list, each item edited via a detail layout (`options.detail`). |
| `ActionButton` | Inline button firing a registered action (`options.actionKey`). |
| `ArrayLayout` | Inline array repeater (add/remove/reorder rows). |

A `Control` `scope` is a JSON pointer into the JSON Schema, e.g.
`#/properties/firstName`. Layouts render as plain `<div>`s (no component needed).

```json
{
  "type": "VerticalLayout",
  "elements": [
    { "type": "Control", "scope": "#/properties/firstName" },
    {
      "type": "HorizontalLayout",
      "elements": [
        { "type": "Control", "scope": "#/properties/role" },
        { "type": "Control", "scope": "#/properties/birthDate" }
      ]
    }
  ]
}
```

Wizard (pages):

```json
{
  "type": "Categorization",
  "elements": [
    { "type": "Category", "label": "personal", "elements": [ { "type": "Control", "scope": "#/properties/firstName" } ] },
    { "type": "Category", "label": "account",  "elements": [ { "type": "Control", "scope": "#/properties/role" } ] }
  ]
}
```

## Options (the `options` object on an element)

| Option | On | Type | Effect |
|---|---|---|---|
| `readonly` | control-like | bool | Display-only. |
| `disabled` | control-like | bool | Greyed out. |
| `hidden` | control-like | bool | Hidden initially; a `Show` rule can reveal it. Hidden values are dropped from the submitted data. |
| `cssClass` | Control / ActionButton / List | string | CSS class(es). Appended to the component default, or `"!x"` to replace it. |
| `helperIconTextLabel` | Control | string | Tooltip on a helper icon. Translation key first, else literal. |
| `helperTextLabel` | Control | string | Helper text under the field. Translation key first, else literal. |
| `prefixLabel` / `suffixLabel` | numeric Control | string | Static prefix/suffix (e.g. `€`, `kg`). Translation key first. |
| `enumItemOptions` | enum Control | object | Per-enum-value metadata keyed by value; each may carry `helperText`. |
| `detail` | ListWithDetail | object | **Required** — the item's UI schema element. |
| `actionKey` | ActionButton | string | Key matching `RegisterAction(key, …)` in code. |
| `addLabel` | ArrayLayout | string | "Add item" button label (translation key; default `+`). |
| `component` | Control / ActionButton | string | **Alias** selecting a custom component (UI-layer-specific). |
| `parameters` | Control / ActionButton | object | Per-field Blazor parameter overrides (UI-layer-specific; engine-owned keys rejected). |

> Any label-like option (`helperIconTextLabel`, `helperTextLabel`,
> `prefixLabel`, `suffixLabel`, `addLabel`, plus `label`/`i18n`) is resolved as a
> translation key first, falling back to the literal string. So you can mix i18n
> keys and plain text freely.

## Rules (conditional show / hide / enable / disable)

```json
{
  "type": "Control",
  "scope": "#/properties/companyName",
  "options": { "hidden": true },
  "rule": {
    "effect": "Show",
    "condition": { "scope": "#/properties/role", "schema": { "const": "admin" } }
  }
}
```

- `effect`: `Show` | `Hide` | `Enable` | `Disable`.
- `condition.scope`: the field whose value is tested.
- `condition.schema`: a JSON Schema fragment; the condition is true when the
  referenced value validates against it (e.g. `{ "const": "admin" }`,
  `{ "minLength": 2 }`, `{ "enum": ["a","b"] }`).

Rules re-evaluate after every change. Combine `"hidden": true` with a `Show` rule
for fields that appear conditionally.

## ActionButton

```json
{ "type": "ActionButton", "label": "calculate", "options": { "actionKey": "calc-premium" } }
```

The button is wired in code: `options.RegisterAction("calc-premium", async form => { … })`.
Tell the user they must register a handler with the matching key.

## ArrayLayout

Inline repeater for an array of objects. With no `items`, the engine
auto-generates a row from the array's `items.properties`:

```json
{ "type": "ArrayLayout", "scope": "#/properties/addresses", "options": { "addLabel": "addAddress" } }
```

Custom per-row layout via `items`:

```json
{
  "type": "ArrayLayout",
  "scope": "#/properties/addresses",
  "items": {
    "type": "HorizontalLayout",
    "elements": [
      { "type": "Control", "scope": "#/properties/street" },
      { "type": "Control", "scope": "#/properties/city" }
    ]
  }
}
```

`minItems`/`maxItems` from the JSON Schema are validated on submit. **Inline vs.
dialog editing is a UI-layer feature** — if the user wants a dialog/popup editor,
that depends on their UI layer (e.g. a `"mode": "dialog"` option). Ask them which
layer they use and which array options it supports before adding such options.

## ListWithDetail

```json
{
  "type": "ListWithDetail",
  "scope": "#/properties/contacts",
  "label": "contacts",
  "options": {
    "detail": {
      "type": "VerticalLayout",
      "elements": [
        { "type": "Control", "scope": "#/properties/name" },
        { "type": "Control", "scope": "#/properties/email" }
      ]
    }
  }
}
```

The `detail` scopes are relative to the array item. `detail` is required.

## Translation Schema

```json
{
  "resources": {
    "en": {
      "translation": {
        "firstName": { "label": "First Name", "error": { "minLength": "At least 2 characters" } },
        "role": {
          "label": "Role",
          "enums": [
            { "value": "admin", "label": "Administrator" },
            { "value": "user",  "label": "Standard user" }
          ]
        },
        "addAddress": { "label": "Add address" },
        "firstName.helper": { "label": "Enter the legal first name." }
      }
    },
    "nl": { "translation": { "firstName": { "label": "Voornaam" } } }
  }
}
```

- **Section key** = the field's property name (or schema path), **or** an explicit
  `label`/`i18n` key from the UI schema, **or** a free key referenced by an option
  (e.g. `firstName.helper`, `addAddress`).
- `label` — display text. `error` — per-error-type messages. `enums` — display
  labels for enum values (`{ value, label }`); omit to show raw values.
- Nested objects mirror nested data via nested sections.
- With no matching language/section, labels fall back to a humanised property name
  and errors to defaults — a form without translations still renders sensibly.

**Error-type keys** (for the `error` object): `required`, `minimum`, `maximum`,
`maximumLength`, `minimumLength`, `maximumItems`, `minimumItems`, `pattern`,
`contains`, `const`.

A full, exhaustive reference (every option, every error key, label-resolution
order, enum helper text, and worked multi-section examples) is in
[`references/ui-schema-reference.md`](references/ui-schema-reference.md) — read it
for edge cases or when generating a large/complex form.

## When to ask the user

The data/UI/translation schemas above are fully portable. But three options are
**UI-layer-specific** and you cannot guess their valid values — ask for them when
the user's request implies them:

- **`options.component`** (aliases like `"slider"`, `"blocks"`, `"dialog"`): which
  components/aliases does their UI layer register?
- **`options.parameters`**: which `[Parameter]` names does the target component
  expose, and their types?
- **`options.cssClass`**: which CSS classes exist in their stylesheet/theme?

Ask concisely, e.g.: *"You want a slider for `volume` — which UI layer are you
using, and does it expose a `slider` component alias and a `Step` parameter?"*
If they don't know, generate the rest and leave a clearly-marked TODO for the
layer-specific option.

## Gotchas

- **"scope is null" / control not rendering** — a `Control` needs a valid `scope`
  pointing at an existing JSON Schema property (`#/properties/...`). Layouts use
  `elements`, not `scope`.
- **Empty layouts throw** — `VerticalLayout`/`HorizontalLayout`/`Group` must have
  `elements`.
- **Arrays as a single control are multi-select only** — `array` renders as a
  control **only** when its `items` is `type: string` **with an `enum`**
  (→ multi-select). Arrays of objects must use `ArrayLayout` or `ListWithDetail`.
- **`ListWithDetail` requires `options.detail`** — omitting it throws.
- **`required` goes on the parent object**, not the property.
- **`helperIconTextLabel`** is the current option name (it was `helperIconLabel`
  in v1).
- **Hidden fields are stripped from submitted data** — use a `Show` rule if a
  hidden field's value must still be submitted when revealed.

## Output checklist

- [ ] Every UI `Control` `scope` resolves to a JSON Schema property.
- [ ] Field types match intent (enum→dropdown, array-of-string-enum→multi-select, etc.).
- [ ] Conditional fields use `"hidden": true` + a `Show`/`Enable` rule.
- [ ] `ListWithDetail` has `options.detail`; `ArrayLayout` has a scope.
- [ ] Every referenced translation key (labels, errors, enums, helper/prefix/addLabel) exists for each language.
- [ ] Any `component`/`parameters`/`cssClass` values were confirmed with the user's UI layer.
- [ ] Action buttons' `actionKey`s are noted for the developer to register in code.
