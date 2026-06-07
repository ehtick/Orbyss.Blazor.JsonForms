# UI / JSON / translation schema — full reference

Exhaustive companion to the schema-authoring SKILL.md. Read for edge cases,
label-resolution order, enum helper text, and a complete worked form.

## Table of contents
- [Label & key resolution order](#label-resolution)
- [Control-type resolution (exact rules)](#control-type)
- [Full options catalogue](#options)
- [enumItemOptions (per-value helper text)](#enumitemoptions)
- [Rules — effects and conditions](#rules)
- [Translation schema — full structure](#translation)
- [Complete worked example](#worked-example)

## Label resolution

Every element's display text (and every label-like option value) resolves in this
order — first hit wins:

1. **`i18n`** key on the element → that translation section's `label`.
2. **`label`** value (if it differs from the property name) → that section's `label`.
3. **Schema-path section** — a section addressed by the field's data path
   (property `firstName` → section `firstName`; nested data → nested sections).
4. **Humanised property name** — e.g. `firstName` → `First Name`.

This is why `label` doubles as a translation key, and why a form with no
translation schema still shows readable labels. Options resolved this way:
`label`, `i18n`, `helperIconTextLabel`, `helperTextLabel`, `prefixLabel`,
`suffixLabel`, `addLabel`.

## Control type

`ControlTypeInterpreter` reads the JSON Schema at the control's `scope`:

- `string` + `enum` → `Enum` (single-select). If the enum lives in an array's
  `items`, → `EnumList` (multi-select).
- `string` + `format: "datetime"` → `DateTime`; `format: "date"` → `DateOnly`;
  otherwise → `String`.
- `number` + `format: "datetime"` → `DateTimeUtcTicks`; `format: "date"` →
  `DateOnlyUtcTicks`; otherwise → `Number`.
- `integer` → `Integer`; `boolean` → `Boolean`.
- `array` → only supported as a control when `items` is `type: string` with an
  `enum` (→ `EnumList`). Any other `items` type throws; arrays of objects must use
  `ArrayLayout`/`ListWithDetail`.
- No `type` → throws (`SchemaTypeNotSpecifiedException`).

Numeric `minimum`/`maximum` are exposed to the renderer (e.g. slider range).

## Options

| Key | Element(s) | JSON type | Notes |
|---|---|---|---|
| `readonly` | control-like | bool | Read-only display. |
| `disabled` | control-like | bool | Disabled. |
| `hidden` | control-like | bool | Hidden initially; reveal with a `Show` rule. Truly-hidden values are stripped from `GetFormData()`. |
| `cssClass` | Control, ActionButton, List | string | Appended to the component's default class; prefix `!` to replace it entirely (the `!` is stripped). |
| `helperIconTextLabel` | Control | string | Helper-icon tooltip. Resolved as a label. |
| `helperTextLabel` | Control | string | Helper text below the field. Resolved as a label. Overrides a programmatic helper text. |
| `prefixLabel` | numeric Control | string | Prefix (e.g. `€`). Resolved as a label. |
| `suffixLabel` | numeric Control | string | Suffix (e.g. `kg`). Resolved as a label. |
| `enumItemOptions` | enum Control | object | Per-value metadata; see below. |
| `detail` | ListWithDetail | object | Required item UI schema element. |
| `actionKey` | ActionButton | string | Matches `RegisterAction(key, …)`. |
| `addLabel` | ArrayLayout | string | Add-button label (resolved as a label; default `+`). |
| `component` | Control, ActionButton | string | UI-layer alias selecting a specific component. |
| `parameters` | Control, ActionButton | object | UI-layer Blazor parameter overrides. Engine-owned keys (`Value`, `ValueChanged`, `Checked`, `CheckedChanged`, `Values`, `ValuesChanged`) are rejected. |

## enumItemOptions

Attach per-enum-value helper text (keyed by the enum **value**):

```json
{
  "type": "Control",
  "scope": "#/properties/plan",
  "options": {
    "enumItemOptions": {
      "pro":  { "helperText": "Best for teams" },
      "free": { "helperText": "Up to 3 users" }
    }
  }
}
```

The text surfaces on each `TranslatedEnumItem.HelperText` for the renderer to show.

## Rules

`effect` ∈ `Show`, `Hide`, `Enable`, `Disable`.
`condition` = `{ "scope": "#/properties/x", "schema": <fragment> }`; true when the
value at `scope` validates against the fragment. Common fragments:

- `{ "const": "admin" }` — equals a value
- `{ "enum": ["a", "b"] }` — one of
- `{ "minLength": 1 }` — non-empty string
- `{ "minimum": 18 }` — numeric threshold
- `{ "type": "string" }` — present/typed

Pair `"hidden": true` with `"effect": "Show"` for fields that appear on condition.
Rules apply to controls, lists, arrays, action buttons, and pages, and
re-evaluate after every value change and array mutation.

## Translation

```
resources
└── <language>            (e.g. "en", "nl")
    └── translation
        └── <sectionKey>          property name | schema path | label/i18n key | free key
            ├── label             display text
            ├── error             { <errorKey>: "message", … }
            ├── <enumValue>       enum display label, e.g. "admin": "Administrator"
            └── <nestedSection>   mirrors nested object data
```

Error keys: `required`, `minimum`, `maximum`, `maximumLength`, `minimumLength`,
`maximumItems`, `minimumItems`, `pattern`, `contains`, `const`. A missing key
falls back to a default; if none translate, the section default is used.

Nested example (data `{ "address": { "street": "…" } }`):

```json
{
  "resources": { "en": { "translation": {
    "address": {
      "label": "Address",
      "street": { "label": "Street" },
      "city":   { "label": "City" }
    }
  } } }
}
```

## Worked example

**Goal:** a 2-page user form. Page 1: first name (required, ≥2), role (dropdown).
Page 2: addresses (inline array). A "company name" field shows only for admins.

JSON Schema:

```json
{
  "type": "object",
  "properties": {
    "firstName":   { "type": "string", "minLength": 2 },
    "role":        { "type": "string", "enum": ["admin", "user"] },
    "companyName": { "type": "string" },
    "addresses": {
      "type": "array",
      "minItems": 1,
      "items": {
        "type": "object",
        "properties": {
          "street": { "type": "string" },
          "city":   { "type": "string" }
        }
      }
    }
  },
  "required": ["firstName"]
}
```

UI Schema:

```json
{
  "type": "Categorization",
  "elements": [
    {
      "type": "Category",
      "label": "account",
      "elements": [
        { "type": "Control", "scope": "#/properties/firstName",
          "options": { "helperTextLabel": "firstName.help" } },
        { "type": "Control", "scope": "#/properties/role" },
        { "type": "Control", "scope": "#/properties/companyName",
          "options": { "hidden": true },
          "rule": { "effect": "Show",
                    "condition": { "scope": "#/properties/role", "schema": { "const": "admin" } } } }
      ]
    },
    {
      "type": "Category",
      "label": "addresses",
      "elements": [
        { "type": "ArrayLayout", "scope": "#/properties/addresses",
          "options": { "addLabel": "addAddress" } }
      ]
    }
  ]
}
```

Translation Schema:

```json
{
  "resources": {
    "en": { "translation": {
      "account":     { "label": "Account" },
      "addresses":   { "label": "Addresses" },
      "firstName":   { "label": "First name", "error": { "required": "Required", "minLength": "Min 2 characters" } },
      "firstName.help": { "label": "Your legal first name." },
      "role": {
        "label": "Role",
        "admin": "Administrator",
        "user": "User"
      },
      "companyName": { "label": "Company name" },
      "addAddress":  { "label": "Add address" },
      "street":      { "label": "Street" },
      "city":        { "label": "City" }
    } }
  }
}
```

Notes: page titles come from each `Category`'s `label` (`account`, `addresses`);
the array auto-generates a street+city row from `items.properties`; `companyName`
is hidden until `role == "admin"`; the array enforces `minItems: 1` on submit.
