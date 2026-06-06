using Orbyss.Blazor.JsonForms.Constants;
using System.Linq.Expressions;
using System.Reflection;

namespace Orbyss.Blazor.JsonForms.ComponentFactory;

/// <summary>
/// A single parameter assignment registered on a <see cref="FormComponentFactory"/>.
/// Stores the resolved parameter name and the static value to assign.
/// </summary>
public abstract class ComponentParameterEntry
{
    public abstract string ParameterName { get; }
    public abstract object? Value { get; }
}

/// <summary>
/// Typed parameter entry created from a property-selector expression and a value.
/// The parameter name is resolved from the expression at construction time, so mistakes
/// (misspelled property names) surface immediately — not at render time.
/// </summary>
public sealed class ComponentParameterEntry<TComponent, TValue>(
    Expression<Func<TComponent, TValue>> selector,
    TValue value)
    : ComponentParameterEntry
{
    public override string ParameterName { get; } = ResolveParameterName(selector);
    public override object? Value { get; } = value;

    private static string ResolveParameterName(Expression<Func<TComponent, TValue>> expr)
    {
        // Support direct property access: x => x.FloatLabelType
        if (expr.Body is MemberExpression member && member.Member is PropertyInfo)
            return member.Member.Name;

        // Support cast expressions: x => (object)x.SomeProperty
        if (expr.Body is UnaryExpression unary && unary.Operand is MemberExpression castMember && castMember.Member is PropertyInfo)
            return castMember.Member.Name;

        throw new ArgumentException(
            $"Expression must be a direct property selector (e.g. x => x.SomeProperty). Got: {expr.Body}");
    }
}
