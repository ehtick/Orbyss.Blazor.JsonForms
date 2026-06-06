using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace Orbyss.Blazor.JsonForms.Utils;

/// <summary>
/// Strips entries from a parameter dictionary whose keys do not correspond to a
/// <c>[Parameter]</c>-decorated property on the target component type.
///
/// <para>
/// Call this as the final step in every factory <c>Create*</c> method to ensure the
/// dictionary passed to <c>DynamicComponent.Parameters</c> contains only values the
/// component can actually receive. Blazor throws if an unrecognised parameter key is
/// present when <c>RendererInfo.IsInteractive</c> is <c>true</c>.
/// </para>
/// </summary>
public static class RemoveUndeclaredParameters
{
    /// <summary>Removes all entries in <paramref name="parameters"/> not declared on <paramref name="componentType"/>.</summary>
    public static void Remove(Type componentType, IDictionary<string, object?> parameters)
    {
        var declared = componentType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<ParameterAttribute>() is not null)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var toRemove = parameters.Keys.Where(k => !declared.Contains(k)).ToList();
        foreach (var key in toRemove)
            parameters.Remove(key);
    }

    /// <summary>Generic overload for compile-time-known component types.</summary>
    public static void Remove<TComponent>(IDictionary<string, object?> parameters)
        => Remove(typeof(TComponent), parameters);
}
