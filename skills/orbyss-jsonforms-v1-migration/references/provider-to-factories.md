# v1 `IFormComponentInstanceProvider` → v2 component factories

The heavy part of the v1→v2 migration for **UI-layer authors**. v1 had one provider
returning *component-instance* objects; v2 has six *factories* that map each element
to a component **type** (+ static parameters), and components inherit engine base
classes directly. This removes a whole layer of instance classes.

## Method mapping

| v1 `IFormComponentInstanceProvider` method | v2 |
|---|---|
| `GetInputField(context, control)` | `IControlComponentFactory.CreateControl` — provided by `ControlComponentFactory`; you just **assign the input slots** and let the default create the instance. |
| `GetButton(type, form)` | `IButtonComponentFactory` — assign `SubmitButtonComponentType` / `NextButtonComponentType` / `PreviousButtonComponentType`. |
| `GetNavigation(form)` | `INavigationComponentFactory` — assign `NavigationComponentType`. |
| `GetList()` / `GetListItem()` | `IListComponentFactory` — assign `ListComponentType` / `ListItemComponentType`. |
| `GetActionButton()` | `IActionButtonComponentFactory` — assign `ActionButtonComponentType`. |
| `GetArrayLayout()` | `IArrayLayoutComponentFactory` — assign `ArrayLayoutComponentType`. |
| `GetGrid()` / `GetGridRow()` / `GetGridColumn()` | **Removed.** Layout containers are plain `<div>`s (`orbyss-form-row` / `orbyss-form-column`) emitted by the engine. Style via CSS. (🟠 visual change — confirm with the user.) |

## Instance-class → component-base mapping

v1 component-instance classes are deleted. Each component now inherits an engine
base class and is referenced by type:

| v1 instance base | v2 component base (inherit) |
|---|---|
| `InputFormComponentInstanceBase` / `InputFormComponentInstance<T>` | `FormInputComponentBase<TValue>` |
| `NumericInputFormComponentInstanceBase` | `FormInputComponentBase<int?>` / `<double?>` (declare `PrefixText`/`SuffixText`) |
| `DropdownFormComponentInstanceBase` | `FormInputComponentBase<string>` / `<IEnumerable<string>>` (declare `Items`/`MultiSelect`) |
| `ButtonFormComponentInstanceBase` | `FormButtonComponentBase` |
| `NavigationFormComponentInstanceBase` | `FormNavigationComponentBase` |
| `ListFormComponentInstanceBase` | `FormListComponentBase` |
| `ListItemFormComponentInstance` | `FormListItemComponentBase` |
| `ActionButtonFormComponentInstanceBase` | `FormActionButtonComponentBase` |
| `ArrayLayoutFormComponentInstanceBase` | `FormArrayLayoutComponentBase` |

Other moves:
- **Per-component parameters**: v1 overrode `GetFormInputParameters()` on the
  instance. v2 → `factory.SetParameter<TComponent, TValue>(x => x.Prop, value)`
  (static defaults) or the schema's `options.parameters` (per field).
- **JToken→value converter**: v1 passed a converter delegate to the instance
  constructor (e.g. `base(token => (bool?)token)`). v2 → override
  `ConvertFromJToken` on the component (default `JToken.ToObject<TValue>()`).
- **Choosing a component by control type or option**: v1 did this with a `switch`
  inside `GetInputField`. v2 → assign the per-type slots; for one alias mapping to
  several components, override `ControlComponentFactory.ResolveComponentType`.
- **Engine-owned parameters** (`Value`, `ValueChanged`, `Checked`,
  `CheckedChanged`, `Values`, `ValuesChanged`) are now restricted — remove any
  code that set them; the engine wires binding.

## Before / after — a boolean "switch"

### v1

```csharp
// Component
public partial class MySwitch : ComponentBase
{
    [Parameter] public bool? Value { get; set; }
    [Parameter] public EventCallback<bool?> OnValueChanged { get; set; }
    [Parameter] public string? OnLabel { get; set; }
    // …other parameters…
}

// Instance
public class MySwitchInstance : InputFormComponentInstance<MySwitch>
{
    public MySwitchInstance() : base(token => (bool?)token) { }   // converter
    public string? OnLabel { get; set; }
    protected override IDictionary<string, object?> GetFormInputParameters()
        => new Dictionary<string, object?> { [nameof(MySwitch.OnLabel)] = OnLabel };
}

// Provider
public class MyProvider : IFormComponentInstanceProvider
{
    public InputFormComponentInstanceBase GetInputField(IJsonFormContext ctx, FormControlContext control)
        => control.Interpretation.ControlType switch
        {
            ControlType.Boolean => new MySwitchInstance { OnLabel = "Yes" },
            ControlType.String  => new MyTextInstance(),
            // …
        };
    public IFormComponentInstance GetGrid(...) => new MyGridInstance();
    // …GetButton, GetNavigation, GetList, GetListItem, GetActionButton, GetArrayLayout…
}

// DI
services.AddSingleton<IFormComponentInstanceProvider, MyProvider>();
services.AddJsonForms(/* … */);
```

### v2

```csharp
using Orbyss.Blazor.JsonForms.Core.ComponentBases;
using Orbyss.Blazor.JsonForms.Core.ComponentFactory;
using Orbyss.Blazor.JsonForms.Core.ComponentFactory.SubFactories;
using Orbyss.Blazor.JsonForms.Core.Constants;
using Newtonsoft.Json.Linq;

// Component — inherits the engine base, declares its extra parameter, overrides the converter
public partial class MySwitch : FormInputComponentBase<bool?>
{
    [Parameter] public string? OnLabel { get; set; }
    public override object? ConvertFromJToken(JToken? token)
        => token is null || token.Type == JTokenType.Null ? null : (bool?)token;
}

// No instance class. The factory assigns the type + static params.
public sealed class MyControlFactory : ControlComponentFactory
{
    public MyControlFactory()
    {
        BooleanInputComponentType = typeof(MySwitch);
        TextInputComponentType    = typeof(MyText);
        // …assign every input slot you support…

        SetParameter<MySwitch, string?>(x => x.OnLabel, "Yes");
        SetParameter<MySwitch, string?>(x => x.Class, FormCssClasses.BooleanInput);
    }
}

// DI — register the sub-factories (transient) before AddJsonForms
services.AddTransient<IControlComponentFactory>(_ => new MyControlFactory());
services.AddTransient<IButtonComponentFactory>(_ => new MyButtonFactory());
services.AddTransient<INavigationComponentFactory>(_ => new MyNavigationFactory());
services.AddTransient<IListComponentFactory>(_ => new MyListFactory());
services.AddTransient<IActionButtonComponentFactory>(_ => new MyActionButtonFactory());
services.AddTransient<IArrayLayoutComponentFactory>(_ => new MyArrayLayoutFactory());
services.AddJsonForms();
```

`GetGrid*` and `MyGridInstance` are simply deleted — layout is now CSS on
`orbyss-form-row` / `orbyss-form-column`.

## Variant selection (was a `switch` in `GetInputField`)

If v1 chose between components by control type for the same "slot" (e.g. an enum
rendered as a dropdown or as blocks depending on an option), express it in
`ResolveComponentType`:

```csharp
protected override Type? ResolveComponentType(string? alias, ControlType controlType)
{
    if (string.Equals(alias, "blocks", StringComparison.OrdinalIgnoreCase))
        return controlType == ControlType.EnumList ? typeof(MyEnumMultiBlocks) : typeof(MyEnumBlocks);
    return base.ResolveComponentType(alias, controlType);  // alias registry, then per-type slot
}
```

The schema opts in with `"options": { "component": "blocks" }`.

## Bundling it as a UI package (the Syncfusion shape)

A clean v2 UI library exposes one `Add…JsonForms` extension that registers all six
sub-factories then calls `AddJsonForms`:

```csharp
public static IServiceCollection AddMyJsonForms(
    this IServiceCollection services,
    Func<IServiceProvider, IJsonFormContext>? jsonFormContextFactory = null)
{
    services.AddTransient<IControlComponentFactory>(_ => new MyControlFactory());
    services.AddTransient<IButtonComponentFactory>(_ => new MyButtonFactory());
    services.AddTransient<INavigationComponentFactory>(_ => new MyNavigationFactory());
    services.AddTransient<IListComponentFactory>(_ => new MyListFactory());
    services.AddTransient<IActionButtonComponentFactory>(_ => new MyActionButtonFactory());
    services.AddTransient<IArrayLayoutComponentFactory>(_ => new MyArrayLayoutFactory());
    return services.AddJsonForms(jsonFormContextFactory);
}
```

Transient registration is what lets a single form override slots/parameters/aliases
via `<JsonForm ConfigureFactories="…">` without affecting other forms. Register
**transient**, not singleton.

Consumers then call `AddMyJsonForms()` instead of registering a provider.

## Per-file procedure

1. For each v1 component-instance class, find its component and the converter +
   `GetFormInputParameters()` it carried.
2. Make the component inherit the right engine base; move extra parameters onto the
   component as `[Parameter]`; move the converter into `ConvertFromJToken`.
3. Delete the instance class.
4. In the factory subclass, assign the component to its slot and re-express the
   instance's parameters as `SetParameter(...)` calls.
5. Delete `GetGrid*` and any layout-container components (🟠 confirm the CSS layout
   plan with the user).
6. Replace the provider registration with the six sub-factory registrations (or a
   single `Add…JsonForms` extension).
7. Build; resolve namespace errors per the Step 2 table in SKILL.md; run a form and
   confirm values round-trip.
