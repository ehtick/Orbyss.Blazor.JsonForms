# 📦 Orbyss.Blazor.JsonForms.Core

**Contracts-only core for [`Orbyss.Blazor.JsonForms`](https://www.nuget.org/packages/Orbyss.Blazor.JsonForms) — the .NET-native [JsonForms.io](https://jsonforms.io) engine for schema-driven Blazor forms.**

This package contains **no engine and no Razor renderer** — only the contracts a
**UI layer** is built against: interfaces, context & interpretation models, the
component-factory abstractions, the abstract component base classes, constants,
and the shared models. Reference it from a UI integration package so your code
depends on stable contracts instead of the full engine.

> Building a form in an app? You want the full engine package
> [`Orbyss.Blazor.JsonForms`](https://www.nuget.org/packages/Orbyss.Blazor.JsonForms)
> (it depends on this one).

---

## When to reference this package

Reference `Orbyss.Blazor.JsonForms.Core` when you are **writing a UI layer** — a
set of Blazor components plus the factories that map each resolved form element to
your components. Examples that do exactly this:

- [`Orbyss.Blazor.Syncfusion.JsonForms`](https://www.nuget.org/packages/Orbyss.Blazor.Syncfusion.JsonForms)
- [`Orbyss.Blazor.MudBlazor.JsonForms`](https://www.nuget.org/packages/Orbyss.Blazor.MudBlazor.JsonForms)
- Your own component set (Radzen, Telerik, Fluent UI, or plain HTML/CSS — no third-party dependency required).

```bash
dotnet add package Orbyss.Blazor.JsonForms.Core
```

Targets **net8.0** and **net10.0**.

---

## What's inside

Everything lives under the `Orbyss.Blazor.JsonForms.Core.*` namespace:

| Namespace | Contents |
|---|---|
| `…Core.ComponentFactory` | `IFormComponentFactory` + the six per-slot sub-factories (`IControlComponentFactory`, `IButtonComponentFactory`, `INavigationComponentFactory`, `IListComponentFactory`, `IActionButtonComponentFactory`, `IArrayLayoutComponentFactory`), `IComponentInstance`, `IFormComponent`, default factory implementations. |
| `…Core.ComponentBases` | Abstract Blazor base classes to inherit from: `FormInputComponentBase<TValue>`, `FormButtonComponentBase`, `FormActionButtonComponentBase`, `FormNavigationComponentBase`, `FormListComponentBase`, `FormListItemComponentBase`, `FormArrayLayoutComponentBase`. |
| `…Core.Context.Interfaces` | `IJsonFormContext` (the live form) and friends. |
| `…Core.Context.Models` | Element contexts (`FormControlContext`, `FormArrayContext`, …) and `JsonFormContextOptions`. |
| `…Core.Interpretation` | `ControlType` and the interpretation models. |
| `…Core.UiSchema` | `FormUiSchema` records and the rule/element enums. |
| `…Core.Constants` | `FormComponentParameterKeys`, `FormUiSchemaOptionKeys`, `FormCssClasses`, `FormCssVariables`, `FormCulture`, `ControlTypeLookup`. |
| `…Core.Models` | `TranslatedEnumItem`, `TranslationSchema`, `TranslationSection`, `DateTimeUtcTicks`, `DateUtcTicks`. |
| `…Core.Utils` | `CssClassHelper`, `RemoveUndeclaredParameters`, converters. |

---

## The shape of a UI layer

A UI layer assigns a Blazor component type to each factory slot, then registers
the sub-factories (as **transient**) before calling `AddJsonForms`:

```csharp
public sealed class MyControlFactory : ControlComponentFactory
{
    public MyControlFactory()
    {
        TextInputComponentType    = typeof(MyTextBox);     // must implement IFormComponent
        NumberInputComponentType  = typeof(MyNumberInput);
        BooleanInputComponentType = typeof(MyCheckbox);
        DropdownComponentType     = typeof(MyDropdown);
        // … assign the slots you support, register defaults via SetParameter, aliases via RegisterAlias
    }
}

services.AddTransient<IControlComponentFactory>(_ => new MyControlFactory());
// … register the other five sub-factories …
services.AddJsonForms();
```

Input components inherit `FormInputComponentBase<TValue>` (which implements
`IFormComponent`), declare a `Value` of the matching CLR type, and invoke
`OnValueChanged` on commit — the engine wires the rest.

**Register sub-factories as transient.** Each `<JsonForm>` resolves its own
instances, which is what makes **per-form** configuration possible and safe: a
single form can override a slot, add parameters, or register an alias via
`<JsonForm ConfigureFactories="…">` (running `FormComponentFactoryOptions` on that
form's instances, on top of the application defaults) without affecting other
forms.

---

## Learn how to build with it

The how-to lives in two importable skills (drop them into your AI assistant's
`.claude/skills/`):

- **Building custom components** — [`skills/orbyss-jsonforms-custom-components`](https://github.com/orbyss-io/Orbyss.Blazor.JsonForms/tree/main/skills/orbyss-jsonforms-custom-components)
- **Authoring schemas** (UI / JSON / translation) — [`skills/orbyss-jsonforms-schema-authoring`](https://github.com/orbyss-io/Orbyss.Blazor.JsonForms/tree/main/skills/orbyss-jsonforms-schema-authoring)

---

## License

MIT License — © Orbyss. · [GitHub](https://github.com/orbyss-io/Orbyss.Blazor.JsonForms) · [Full engine package](https://www.nuget.org/packages/Orbyss.Blazor.JsonForms)
