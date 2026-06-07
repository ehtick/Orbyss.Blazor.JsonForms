using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using Orbyss.Blazor.JsonForms.Core.Constants;
using Orbyss.Blazor.JsonForms.Core.Extensions;
using Orbyss.Blazor.JsonForms.Core.Utils;
using System.Linq.Expressions;
using System.Reflection;

namespace Orbyss.Blazor.JsonForms.Core.ComponentFactory;

/// <summary>
/// Shared infrastructure for all concrete sub-factory classes.
/// Provides parameter registration (<see cref="SetParameter{TComponent,TValue}"/>),
/// alias registration (<see cref="RegisterAlias"/>), and helpers used during
/// <c>Create*</c> implementations.
/// </summary>
public abstract class ComponentFactoryBase
{
    // ── Parameter registration ────────────────────────────────────────────────

    private readonly Dictionary<Type, List<ComponentParameterEntry>> _parameters = new();

    /// <summary>
    /// Registers a static parameter value for <typeparamref name="TComponent"/>.
    /// The parameter name is resolved from the lambda at call time — typos fail at compile time.
    /// Throws <see cref="InvalidOperationException"/> when the name is engine-restricted.
    /// </summary>
    protected ComponentFactoryBase SetParameter<TComponent, TValue>(
        Expression<Func<TComponent, TValue>> selector,
        TValue value)
    {
        var entry = new ComponentParameterEntry<TComponent, TValue>(selector, value);
        GuardRestricted(entry.ParameterName);
        AddEntry(typeof(TComponent), entry);
        return this;
    }

    /// <summary>
    /// Returns all static parameter entries registered for <paramref name="componentType"/>.
    /// The engine merges these on top of its auto-wired parameters (layer 2 of 3).
    /// </summary>
    public IReadOnlyList<ComponentParameterEntry> GetAssignedParameters(Type componentType)
        => _parameters.TryGetValue(componentType, out var list) ? list : [];

    private void AddEntry(Type componentType, ComponentParameterEntry entry)
    {
        if (!_parameters.TryGetValue(componentType, out var list))
            _parameters[componentType] = list = [];
        list.Add(entry);
    }

    // ── Alias registry (UI schema "component" option) ─────────────────────────

    private readonly Dictionary<string, Type> _aliases = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Maps a UI schema <c>"component"</c> key to a concrete <see cref="Type"/>.
    /// The key is matched case-insensitively. Registering the same key twice replaces the entry.
    /// </summary>
    public void RegisterAlias(string key, Type componentType)
        => _aliases[key] = componentType;

    /// <summary>Returns the component type registered for the given alias key, or <c>null</c>.</summary>
    public Type? ResolveAlias(string key)
        => _aliases.TryGetValue(key, out var type) ? type : null;

    // ── Parameter-merging helpers used by Create* implementations ────────────

    /// <summary>
    /// Writes all registered static parameters for <paramref name="componentType"/> into
    /// <paramref name="instance"/>. Overwrites any engine-auto-wired values (layer 2).
    /// </summary>
    protected void ApplyAssignedParameters(IComponentInstance instance, Type componentType)
    {
        foreach (var entry in GetAssignedParameters(componentType))
            instance.Parameters[entry.ParameterName] = entry.Value;
    }

    /// <summary>
    /// Merges the CSS class from the UI schema into any factory-registered class, separating
    /// them with a space. When neither is set nothing is written.
    /// </summary>
    protected static void MergeCssClass(IComponentInstance instance, string? schemaClass)
    {
        if (instance.Parameters.TryGetValue(FormComponentParameterKeys.Class, out var rawClass)
            && rawClass is string factoryClass)
        {
            instance.Parameters[FormComponentParameterKeys.Class] =
                CssClassHelper.Merge(factoryClass, schemaClass);
        }
        else if (!string.IsNullOrEmpty(schemaClass))
        {
            instance.Parameters[FormComponentParameterKeys.Class] = schemaClass;
        }
    }

    /// <summary>
    /// Applies the UI schema <c>"parameters"</c> option object into the instance's parameter
    /// dictionary (layer 3 — highest precedence, last write wins).
    /// Throws <see cref="InvalidOperationException"/> if any key is engine-restricted.
    /// </summary>
    protected static void ApplyUiSchemaParameters(IComponentInstance instance, JToken? parametersToken)
    {
        if (parametersToken is not JObject parametersObj) return;

        foreach (var (key, value) in parametersObj)
        {
            var parameterProperty = instance.ComponentType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(x => x.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Parameter '{key}' does not exist on component '{instance.ComponentType.Name}'");

            if (parameterProperty.GetCustomAttribute<ParameterAttribute>() is null)
                throw new InvalidOperationException($"Component '{instance.ComponentType.Name}' does not have a parameter attributed defined for '{key}'");

            if (FormComponentParameterKeys.Restricted.Contains(key))
                throw new InvalidOperationException(
                    $"Parameter '{key}' is engine-owned and cannot be overridden via the UI schema " +
                    $"'parameters' option. The engine wires this automatically from the form context.");

            if (value is null)
            {
                continue;
            }

            if (value is not JValue jsonValue)
            {
                instance.Parameters[key] = DefaultJsonConverter.Deserialize(value.ToString(), parameterProperty.PropertyType);
                continue;
            }

            var dotnetValue = Convert.ChangeType(
                jsonValue.ToDotnetValue(),
                parameterProperty.PropertyType
            );
            instance.Parameters[key] = dotnetValue;
        }
    }

    // ── Guard ─────────────────────────────────────────────────────────────────

    private static void GuardRestricted(string parameterName)
    {
        if (FormComponentParameterKeys.Restricted.Contains(parameterName))
            throw new InvalidOperationException(
                $"Parameter '{parameterName}' is engine-owned (value binding / change notification) " +
                $"and cannot be set via SetParameter or UI schema options. " +
                $"The engine wires these automatically from the form context.");
    }
}
