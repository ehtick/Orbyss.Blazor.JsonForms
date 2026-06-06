# 📦 Orbyss.Blazor.JsonForms

**A fully .NET-native implementation of the [JsonForms.io](https://jsonforms.io) standard for schema-driven forms in Blazor.**  
No Angular, no web components — just C#, JSON Schema, and flexible Blazor architecture.

---

## 🎯 What is this?

This is the **UI-agnostic core framework** for rendering dynamic forms from JSON Schema in .NET. It handles:

- Form generation from three schemas (data, layout, translations)
- Schema interpretation and control type resolution
- Localisation via a translation schema
- Layout, validation, rules, and data management

You plug in a **UI layer** (a `IFormComponentInstanceProvider` implementation). This library is the form engine — you bring the renderer.

---

## 🚀 Available UI Integrations

Use one of our ready-to-go UI packages:

- 🧩 [Orbyss.Blazor.Syncfusion.JsonForms](https://www.nuget.org/packages/Orbyss.Blazor.Syncfusion.JsonForms)
- 🎨 [Orbyss.Blazor.MudBlazor.JsonForms](https://www.nuget.org/packages/Orbyss.Blazor.MudBlazor.JsonForms)

Or build your own — for example when you have your own Blazor component system or use **Radzen**, **Telerik**, or **Fluent UI**.

---

## 📦 Installation

```bash
dotnet add package Orbyss.Blazor.JsonForms
```

Then reference a UI implementation package or build your own.

---

## ⚡ Quick Start

```razor
<JsonForm InitOptions="@options" />

@code {
    JsonFormContextInitOptions options = new(
        jsonSchema,
        uiSchema,
        translationSchema
    );
}
```

> 💡 `JsonFormContext` can be provided as a `[Parameter]` or registered as a **Transient** DI service.  
> 💡 `IFormComponentInstanceProvider` can be provided as a `[Parameter]` or as a DI service.  
> 💡 The following cascading values are supported: `Language`, `Disabled`, `ReadOnly`.

---

## 🔄 How the Framework Works

Forms are driven by three schemas:

| Schema | Purpose |
|---|---|
| **JSON Schema** | Defines the data structure — types, constraints, required fields |
| **UI Schema** | Controls layout, grouping, options, and rules |
| **Translation Schema** | Provides localised labels, error messages, and enum display values |

### Full Example

```json
// JSON Schema
{
    "type": "object",
    "properties": {
        "firstName": { "type": "string", "minLength": 2, "maxLength": 50 },
        "surname":   { "type": "string" },
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
        {
            "type": "Control",
            "scope": "#/properties/firstName",
            "options": {
                "helperIconLabel": "firstName.helper",
                "cssClass": "highlighted-field"
            }
        },
        {
            "type": "Control",
            "scope": "#/properties/role",
            "options": {
                "helperIconLabel": "Select the role that matches the user's access level."
            }
        },
        {
            "type": "Control",
            "scope": "#/properties/surname",
            "options": { "hidden": true },
            "rule": {
                "effect": "Show",
                "condition": {
                    "scope": "#/properties/firstName",
                    "schema": { "minLength": 2 }
                }
            }
        }
    ]
}
```

```json
// Translation Schema
{
    "resources": {
        "en": {
            "translation": {
                "firstName": {
                    "label": "First Name",
                    "error": { "minLength": "Must be at least 2 characters" }
                },
                "surname":  { "label": "Surname" },
                "role":     { "label": "Role" },
                "firstName.helper": { "label": "Enter the user's legal first name." }
            }
        },
        "nl": {
            "translation": {
                "firstName": {
                    "label": "Voornaam",
                    "error": { "minLength": "Moet minimaal 2 tekens bevatten" }
                },
                "surname":  { "label": "Achternaam" },
                "role":     { "label": "Rol" },
                "firstName.helper": { "label": "Voer de wettelijke voornaam in." }
            }
        }
    }
}
```

---

## 🗂 Known UI Schema Options

These option keys are defined in `FormUiSchemaOptionKeys` and are understood by the core engine:

| Option key | Applies to | Type | Description |
|---|---|---|---|
| `readonly` | Control | `bool` | Makes the control read-only |
| `disabled` | Control | `bool` | Disables the control |
| `hidden` | Control | `bool` | Hides the control initially (can be revealed by a rule) |
| `cssClass` | Control | `string` | One or more CSS classes applied to the component. Merged with any programmatically assigned class — schema class is appended last |
| `helperIconLabel` | Control | `string` | Text shown in a helper icon tooltip. Resolved through the translation context: treated as an i18n key first, falls back to the literal string |
| `helperTextLabel` | Control | `string` | Helper text shown below the control. Resolved via translation context. Schema value overwrites any programmatically set helper text. |
| `enumItemOptions` | Control | `object` | Per-enum metadata keyed by enum value. Each entry may contain a `helperText` property. |
| `prefixLabel` | Control | `string` | Prefix prepended to numeric display values (e.g. `€`). Resolved via translation context. Schema wins. |
| `suffixLabel` | Control | `string` | Suffix appended to numeric display values (e.g. `m`, `kg`). Resolved via translation context. Schema wins. |
| `detail` | ListWithDetail | `object` | The UI schema for a list item's detail view |

> 💡 Custom options beyond these can still be read at runtime via `control.Interpretation.GetOption("myKey")` in your `IFormComponentInstanceProvider` implementation.

### `helperIconLabel` — translation resolution

The value is resolved just like a label. Given `"helperIconLabel": "myKey"`:

1. The engine looks for a translation section named `"myKey"` and returns its `label` property.
2. If no match is found the literal string `"myKey"` is used as-is.

This means you can freely mix i18n keys and plain text in the same form.

### `cssClass` — merging behaviour

If a control has both a schema-defined class and a programmatically assigned class (set on the component instance by your `IFormComponentInstanceProvider`), the two are merged:

```
final class = "{programmaticClass} {schemaClass}"
```

---

## 🛠 Implementing Your Own UI Layer

### 1. Implement `IFormComponentInstanceProvider`

```csharp
public interface IFormComponentInstanceProvider
{
    InputFormComponentInstanceBase GetInputField(IJsonFormContext context, FormControlContext control);
    IFormComponentInstance GetGridRow(IFormElementContext? row);
    IFormComponentInstance GetGridColumn(IFormElementContext? column);
    IFormComponentInstance GetGrid(IJsonFormContext? form, FormPageContext? page);
    ButtonFormComponentInstanceBase GetButton(FormButtonType type, IJsonFormContext? form);
    NavigationFormComponentInstanceBase GetNavigation(IJsonFormContext formContext);
    ListFormComponentInstanceBase GetList(FormListContext? list = null);
    ListItemFormComponentInstance GetListItem(IFormElementContext? listItem = null);
}
```

`GetInputField` is the most important method — it maps a control context to a component instance:

```csharp
public virtual InputFormComponentInstanceBase GetInputField(IJsonFormContext context, FormControlContext control)
{
    return control.Interpretation.ControlType switch
    {
        ControlType.Boolean         => GetBooleanField(control),
        ControlType.String          => GetTextField(control),
        ControlType.Enum            => GetDropDownField(control),
        ControlType.EnumList        => GetMultiDropDownField(control),
        ControlType.DateTime        => GetDateTimeField(control),
        ControlType.DateOnly        => GetDateOnlyField(control),
        ControlType.DateOnlyUtcTicks  => GetDateUtcTicksField(control),
        ControlType.DateTimeUtcTicks  => GetDateTimeUtcTicksField(control),
        ControlType.Integer         => GetIntegerField(control),
        ControlType.Number          => GetNumberField(control),
        _ => throw new NotSupportedException($"Cannot create an input field for type '{control.Interpretation.ControlType}'")
    };
}
```

### 2. Create your Razor component

Your component receives a standard set of parameters automatically populated by the engine:

```razor
@* MyTextInput.razor *@

<input value="@Value"
       placeholder="@Label"
       disabled="@(Disabled || ReadOnly)"
       class="@Class"
       @oninput="e => OnValueChanged.InvokeAsync(e.Value?.ToString())" />

@if (!string.IsNullOrWhiteSpace(HelperIconText))
{
    <span class="helper-icon" title="@HelperIconText">ℹ️</span>
}

@if (!string.IsNullOrWhiteSpace(ErrorHelperText))
{
    <div class="error">@ErrorHelperText</div>
}
else if (!string.IsNullOrWhiteSpace(HelperText))
{
    <div class="helper"><i>@HelperText</i></div>
}

@code {
    [Parameter] public string?  Label          { get; set; }
    [Parameter] public string?  Class          { get; set; }
    [Parameter] public bool     Disabled        { get; set; }
    [Parameter] public bool     ReadOnly        { get; set; }
    [Parameter] public string?  ErrorHelperText { get; set; }
    [Parameter] public string?  HelperText      { get; set; }
    [Parameter] public string?  HelperIconText  { get; set; }
    [Parameter] public string?  Value           { get; set; }
    [Parameter] public EventCallback<string?> OnValueChanged { get; set; }
}
```

#### Standard parameters on `InputFormComponentInstanceBase`

| Parameter | Type | Set by |
|---|---|---|
| `Label` | `string?` | Engine (translated) |
| `Disabled` | `bool` | Engine / UI schema |
| `ReadOnly` | `bool` | Engine / UI schema |
| `ErrorHelperText` | `string?` | Engine (validation) |
| `HelperText` | `string?` | Your component instance (overwritten by `helperTextLabel` schema option if set) |
| `HelperIconText` | `string?` | Engine (from `helperIconLabel` option, translated) |
| `Class` | `string?` | Engine (merged from `cssClass` option + programmatic) |
| `Style` | `string?` | Your component instance |
| `Value` | `object?` | Engine (typed per control type) |

#### Value / callback types per control type

The `Value` and `OnValueChanged` types must match exactly:

| Control type | Value type | EventCallback type |
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

> ⚠️ If `OnValueChanged` is never invoked, the form state will not update.

### 3. Create a component instance class

The component instance is the contract between your provider and your Razor component. When the built-in parameters are enough, use a built-in instance directly:

```csharp
public virtual ListItemFormComponentInstance GetListItem(IFormElementContext? listItem = null)
{
    return new ListItemFormComponentInstance<MyListItem>();
}
```

#### `UiSchemaControlInterpretation` — numeric constraints

`Minimum` (`double?`) and `Maximum` (`double?`) are automatically parsed from the JSON Schema `minimum`/`maximum` keywords and exposed on `control.Interpretation.Minimum` / `control.Interpretation.Maximum`. Use these in your provider to configure range-based components such as sliders.

#### Component instance hierarchy

- `FormComponentInstanceBase` — base for all instances
  - `InputFormComponentInstanceBase` — standard input fields (label, value, disabled, read-only, errors, helper text)
    - `NumericInputFormComponentInstanceBase` — adds `PrefixText` and `SuffixText`; schema options `prefixLabel`/`suffixLabel` overwrite these at render time
    - `DropdownFormComponentInstanceBase` — enum fields (items, multi-select, clearable, searchable)

For components with additional parameters, derive from the appropriate base and override `GetFormInputParameters`:

```csharp
// Component
@* MySwitch.razor *@
<MySwitch OnLabel="@OnLabel" OffLabel="@OffLabel" ... />

[Parameter] public string? OnLabel  { get; set; }
[Parameter] public string? OffLabel { get; set; }

// Component instance
public class MySwitchInstance : InputFormComponentInstance<MySwitch>
{
    public MySwitchInstance() : base(token => (bool?)token) { }

    public string? OnLabel  { get; set; }
    public string? OffLabel { get; set; }

    protected override IDictionary<string, object?> GetFormInputParameters()
    {
        return new Dictionary<string, object?>
        {
            [nameof(MySwitch.OnLabel)]  = OnLabel,
            [nameof(MySwitch.OffLabel)] = OffLabel
        };
    }
}
```

### 4. Return your instance from the provider

```csharp
protected virtual InputFormComponentInstanceBase GetBooleanField(FormControlContext control)
{
    // Custom options can drive which component is rendered
    var boolType = $"{control.Interpretation.GetOption("custom-bool-type")}";

    return boolType == "switch"
        ? new MySwitchInstance { OnLabel = "Yes", OffLabel = "No" }
        : new MyCheckboxInstance();
}
```

---

## 📄 License
MIT License — © Orbyss

## 🔗 Links
- 🌍 **Website**: [https://orbyss.io](https://orbyss.io)
- 📦 **NuGet**: [Orbyss.Blazor.JsonForms](https://www.nuget.org/packages/Orbyss.Blazor.JsonForms)
- 🧑‍💻 **GitHub**: [https://github.com/Orbyss-io](https://github.com/orbyss-io)
- 📝 **License**: [MIT](./LICENSE)
- [JsonForms.io](https://jsonforms.io/)
- [Syncfusion UI integration](https://www.nuget.org/packages/Orbyss.Blazor.Syncfusion.JsonForms)
- [MudBlazor UI integration](https://www.nuget.org/packages/Orbyss.Blazor.MudBlazor.JsonForms)

## 🤝 Contributing

Contributions are welcome — bug fixes, improvements, documentation, or ideas.  
Fork the repo, create a branch, and open a pull request.

- Write clean, readable code
- Keep PRs focused and descriptive
- Open issues for larger features or discussions

---

⭐️ If you find this useful, [give us a star](https://github.com/orbyss-io/Orbyss.Blazor.JsonForms/stargazers) and help spread the word!
