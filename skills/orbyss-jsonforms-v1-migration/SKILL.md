---
name: orbyss-jsonforms-v1-migration
description: >
  Migrate a codebase from Orbyss.Blazor.JsonForms major version 1 to version 2.
  Use this whenever a user is upgrading Orbyss JSON Forms, hits build errors after
  bumping the package, or mentions v1→v2, "IFormComponentInstanceProvider",
  "JsonFormContextInitOptions", "GetInputField/GetGrid", missing
  "Orbyss.Components.Json.Models", "Orbyss.Blazor.JsonForms.Constants/UiSchema/
  ComponentBases does not exist", "helperIconLabel", or "PageTitleClass/
  GroupTitleClass not found" on Orbyss forms. Point an AI session at this skill and
  it will detect the consumer type, apply the required code/schema changes, and
  stop to ask the user wherever a choice changes the look or behaviour of their
  form. Trigger even if the user only says "upgrade my json forms" without naming
  the version.
---

# Migrating Orbyss.Blazor.JsonForms v1 → v2

v2 is a structural release. The two biggest changes: the UI layer moved from a
single `IFormComponentInstanceProvider` to a set of **component factories**, and
core contracts moved into a new **`Orbyss.Blazor.JsonForms.Core`** package /
namespace. This skill walks a real codebase through the upgrade.

Your job: **detect what the codebase actually uses, apply the mechanical changes
automatically, and pause to ask the user only where a change alters the form's
look or behaviour** (those decision points are flagged 🟠 below).

## Step 0 — figure out the consumer type

Grep the repo to classify the work:

- **App consumer** (light migration): uses a UI integration package
  (`Orbyss.Blazor.*.JsonForms`) + `<JsonForm>` + schemas, but does **not**
  implement the renderer. Signal: no `IFormComponentInstanceProvider`, no
  `*FormComponentInstance` classes.
- **UI-layer author** (heavy migration): implements the renderer. Signal:
  `IFormComponentInstanceProvider`, `GetInputField`, `GetGrid`,
  `*FormComponentInstanceBase`, `FormComponentInstance<…>`.

A repo can be both (an app that also has a few bespoke components). Do the app
steps (Part A) for everyone; add Part B if the renderer is implemented.

Confirm the package versions first:

```bash
grep -rn "Orbyss.Blazor.JsonForms" --include=*.csproj .
```

## Step 1 — packages

- Bump `Orbyss.Blazor.JsonForms` (and any `Orbyss.Blazor.*.JsonForms` UI package)
  to their v2 versions.
- **UI-layer libraries**: add a reference to **`Orbyss.Blazor.JsonForms.Core`**
  (contracts moved there) and keep referencing the engine only if you need it.
- **Remove** any direct `Orbyss.Components.Json.Models` package reference — those
  models now ship inside `Orbyss.Blazor.JsonForms.Core`.

```bash
dotnet remove package Orbyss.Components.Json.Models   # if present
```

## Step 2 — namespaces (everyone)

Core types moved under `Orbyss.Blazor.JsonForms.Core.*`. The engine
(`JsonFormContext`, interpreters, `AddJsonForms`, `<JsonForm>`) stays under
`Orbyss.Blazor.JsonForms.*`. Update `using`s:

| v1 namespace | v2 namespace |
|---|---|
| `Orbyss.Blazor.JsonForms.Constants` | `Orbyss.Blazor.JsonForms.Core.Constants` |
| `Orbyss.Blazor.JsonForms.ComponentBases` | `Orbyss.Blazor.JsonForms.Core.ComponentBases` |
| `Orbyss.Blazor.JsonForms.UiSchema` | `Orbyss.Blazor.JsonForms.Core.UiSchema` |
| `Orbyss.Blazor.JsonForms.Interpretation` (models) | `Orbyss.Blazor.JsonForms.Core.Interpretation` |
| `Orbyss.Blazor.JsonForms.Context.Models` | `Orbyss.Blazor.JsonForms.Core.Context.Models` |
| `Orbyss.Blazor.JsonForms.Context.Interfaces` (`IJsonFormContext`, `IFormElementContext`, `IJsonFormNotificationHandler`) | `Orbyss.Blazor.JsonForms.Core.Context.Interfaces` |
| `Orbyss.Blazor.JsonForms.Context.Notifications` (`JsonFormNotificationType`) | `Orbyss.Blazor.JsonForms.Core.Context.Notifications` |
| `Orbyss.Blazor.JsonForms.Utils` (`CssClassHelper`, converters) | `Orbyss.Blazor.JsonForms.Core.Utils` |
| `Orbyss.Components.Json.Models` (`TranslatedEnumItem`, `TranslationSchema`, `TranslationSection`, `DateTimeUtcTicks`, `DateUtcTicks`) | `Orbyss.Blazor.JsonForms.Core.Models` |

Caution: some leaf namespaces exist in **both** projects (e.g. `…Interpretation`
holds interpreters in the engine and models in core; `…Context.Interfaces` and
`…Context.Notifications` likewise). When a file uses both an interpreter/handler
*and* a model/enum, keep the engine `using` **and** add the `…Core.*` one. Let the
compiler guide you: build, read each "type or namespace does not exist" error, add
the matching `…Core.*` import.

## Step 3 — renames (everyone)

| v1 | v2 |
|---|---|
| `JsonFormContextInitOptions` | `JsonFormContextOptions` |
| UI-schema option `helperIconLabel` | `helperIconTextLabel` |
| `FormUiSchemaOptionKeys.HelperIconLabel` | `FormUiSchemaOptionKeys.HelperIconTextLabel` |

Update **both** code constants and any **schema JSON / strings** containing
`"helperIconLabel"`.

```bash
grep -rn "helperIconLabel\|JsonFormContextInitOptions" .
```

## Step 4 — 🟠 `<JsonForm>` title styling (decision point)

`PageTitleClass` and `GroupTitleClass` parameters on `<JsonForm>` were **removed**.
Page and group titles now render as neutral `<div>`s with fixed classes
(`orbyss-form-page-title`, `orbyss-form-group-title`) that you restyle via CSS
(class override or the `--orbyss-form-*-title-*` variables).

If the codebase passes `PageTitleClass`/`GroupTitleClass`, this changes how titles
look unless their styling is moved. **Ask the user** (see [decision points](#decision-points))
whether to port their old class's rules onto the new classes, then remove the
parameters from the `<JsonForm>` usage.

## Step 5 — UI-layer authors only (Part B)

If the codebase implements `IFormComponentInstanceProvider`, this is the bulk of
the work: convert the provider + component-instance classes into **component
factories + components that inherit the engine base classes**. The full transform,
the per-method mapping table, and a before/after worked example are in
[`references/provider-to-factories.md`](references/provider-to-factories.md).
Read it and apply it before continuing.

Headlines:
- `IFormComponentInstanceProvider` → six sub-factories
  (`IControlComponentFactory`, `IButtonComponentFactory`,
  `INavigationComponentFactory`, `IListComponentFactory`,
  `IActionButtonComponentFactory`, `IArrayLayoutComponentFactory`); subclass the
  provided defaults (`ControlComponentFactory`, …) and assign component **types**
  to slots.
- 🟠 `GetGrid` / `GetGridRow` / `GetGridColumn` are **gone** — layout containers
  are now plain `<div>`s emitted by the engine (`orbyss-form-row` /
  `orbyss-form-column`). Custom layout components can no longer be injected; layout
  is CSS now. This is a visual change → decision point.
- Component-instance classes (`InputFormComponentInstanceBase`,
  `FormComponentInstance<T>`, `GetFormInputParameters()`, …) are **gone**. Each
  component now inherits `FormInputComponentBase<TValue>` (or the matching base for
  buttons/list/array/etc.), and per-component static parameters move to
  `factory.SetParameter<TComponent, TValue>(x => x.Prop, value)`.
- The JToken→value converter you used to pass into an instance constructor becomes
  an override of `ConvertFromJToken` on the component (default uses
  `JToken.ToObject<TValue>()`).
- DI: register each sub-factory **transient** before `AddJsonForms`, or pass
  `configureFactories` to `AddJsonForms`. (Transient — not singleton — so each form
  gets its own instances; this is what enables the new per-form
  `<JsonForm ConfigureFactories="…">` override and prevents cross-form state
  bleed. If you register your own composite/sub-factories, use `AddTransient`.)

## Step 6 — verify

```bash
dotnet build
dotnet test   # if the consumer has tests
```

Then run the app and confirm: fields render, values round-trip (type into a field,
submit, inspect data), conditional rules fire, and titles/layout look right after
Step 4 / the Step 5 layout decision.

## Decision points

Use these whenever the codebase hits them — present options and context, and let
the **user** choose, because they change the user-facing form:

**🟠 Title styling (Step 4).** If they passed `PageTitleClass="x"` /
`GroupTitleClass="y"`:
- *Option A (recommended):* port the rules from `.x` / `.y` onto
  `.orbyss-form-page-title` / `.orbyss-form-group-title` in their stylesheet, and
  drop the parameters. Titles look the same.
- *Option B:* leave the new default styling (medium, bold) and delete the old
  classes. Simpler, but the look changes.

**🟠 Custom layout components removed (Step 5).** If they injected custom grid/row/
column components via `GetGrid*`:
- *Option A (recommended):* accept the engine's plain-`<div>` layout
  (`orbyss-form-row` / `orbyss-form-column`) and reproduce the old look with CSS
  (e.g. fl/grid rules on those classes). Less code, standard theming.
- *Option B:* if a specific layout behaviour can't be expressed in CSS, wrap the
  whole `<JsonForm>` or build the layout outside the engine. Flag the specific case
  to the user for a bespoke decision.

Frame each as: "v1 did X via Y, which v2 removed. Here's why and the two ways
forward — which do you want?" Don't silently pick the option that changes their UX.

## Migration checklist

- [ ] Packages bumped; `Orbyss.Components.Json.Models` package removed; Core added for UI-layer libs.
- [ ] All `using`s repointed (Step 2); build resolves all namespaces.
- [ ] `JsonFormContextInitOptions` → `JsonFormContextOptions`; `helperIconLabel` → `helperIconTextLabel` in code **and** schemas.
- [ ] `PageTitleClass`/`GroupTitleClass` removed; title styling decision applied.
- [ ] (UI-layer) provider → sub-factories; instance classes → component base inheritance; converters → `ConvertFromJToken`; DI updated; layout decision applied.
- [ ] `dotnet build` clean; app runs; values round-trip; rules and titles verified.
