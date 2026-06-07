using System.Collections;

namespace NUnit.Framework;

public static class Assert
{
    public static void That(object? actual, IConstraint constraint, string? message = null)
    {
        if (!constraint.Matches(actual))
            throw new Xunit.Sdk.XunitException(message ?? constraint.FailureMessage(actual));
    }

    public static T Throws<T>(Action action)
        where T : Exception
        => Xunit.Assert.Throws<T>(action);

    public static void DoesNotThrow(Action action)
    {
        var exception = Xunit.Record.Exception(action);
        if (exception is not null)
            throw new Xunit.Sdk.XunitException($"Expected no exception, but got {exception.GetType().Name}: {exception.Message}");
    }

    public static void DoesNotThrowAsync(Func<Task> action)
    {
        var exception = Xunit.Record.ExceptionAsync(action).GetAwaiter().GetResult();
        if (exception is not null)
            throw new Xunit.Sdk.XunitException($"Expected no exception, but got {exception.GetType().Name}: {exception.Message}");
    }
}

public static class Is
{
    public static NullConstraint Null => new();
    public static IConstraint True => new EqualConstraint(true);
    public static IConstraint False => new EqualConstraint(false);
    public static IConstraint Empty => new EmptyConstraint();
    public static NotOperator Not { get; } = new();

    public static IConstraint EqualTo(object? expected) => new EqualConstraint(expected);
    public static IConstraint EquivalentTo(IEnumerable expected) => new EquivalentConstraint(expected);
    public static IConstraint SameAs(object? expected) => new SameAsConstraint(expected);
    public static IConstraint InstanceOf<T>() => new InstanceOfConstraint(typeof(T));
}

public static class Has
{
    public static PropertyConstraintBuilder Length { get; } = new("Length");
    public static PropertyConstraintBuilder Count { get; } = new("Count");
}

public static class Does
{
    public static IConstraint Contain(object? expected) => new ContainsConstraint(expected);
}

public sealed class NotOperator
{
    public IConstraint Null => new NotConstraint(Is.Null);
    public IConstraint Empty => new NotConstraint(Is.Empty);
    public IConstraint EqualTo(object? expected) => new NotConstraint(Is.EqualTo(expected));
    public IConstraint InstanceOf<T>() => new NotConstraint(Is.InstanceOf<T>());
    public IConstraint SameAs(object? expected) => new NotConstraint(Is.SameAs(expected));
}

public sealed class OrOperator(IConstraint left)
{
    public IConstraint Empty => new OrConstraint(left, Is.Empty);
}

public sealed class PropertyConstraintBuilder(string propertyName)
{
    public IConstraint EqualTo(object? expected) => new PropertyConstraint(propertyName, new EqualConstraint(expected));
    public IConstraint GreaterThanOrEqualTo(IComparable expected) => new PropertyConstraint(propertyName, new GreaterThanOrEqualConstraint(expected));
}

public interface IConstraint
{
    bool Matches(object? actual);

    string FailureMessage(object? actual);
}

public abstract class ConstraintBase : IConstraint
{
    public abstract bool Matches(object? actual);

    public virtual string FailureMessage(object? actual)
        => $"Constraint failed for value '{actual ?? "<null>"}'.";
}

public sealed class NullConstraint : ConstraintBase
{
    public OrOperator Or => new(this);

    public override bool Matches(object? actual) => actual is null;

    public override string FailureMessage(object? actual) => $"Expected null, but got '{actual}'.";
}

public sealed class EqualConstraint(object? expected) : ConstraintBase
{
    public override bool Matches(object? actual)
    {
        if (expected is null || actual is null)
            return expected is null && actual is null;

        if (IsNumeric(expected) && IsNumeric(actual))
            return Convert.ToDecimal(expected) == Convert.ToDecimal(actual);

        return object.Equals(expected, actual);
    }

    public override string FailureMessage(object? actual) => $"Expected '{expected}', but got '{actual}'.";

    private static bool IsNumeric(object value)
    {
        return value is byte or sbyte or short or ushort or int or uint or long or ulong
            or float or double or decimal;
    }
}

public sealed class GreaterThanOrEqualConstraint(IComparable expected) : ConstraintBase
{
    public override bool Matches(object? actual)
        => actual is IComparable comparable && comparable.CompareTo(expected) >= 0;
}

public sealed class SameAsConstraint(object? expected) : ConstraintBase
{
    public override bool Matches(object? actual) => ReferenceEquals(expected, actual);
}

public sealed class InstanceOfConstraint(Type expectedType) : ConstraintBase
{
    public override bool Matches(object? actual) => actual is not null && expectedType.IsInstanceOfType(actual);
}

public sealed class EmptyConstraint : ConstraintBase
{
    public override bool Matches(object? actual)
    {
        return actual switch
        {
            null => false,
            string text => text.Length == 0,
            IEnumerable enumerable => !enumerable.Cast<object?>().Any(),
            _ => false
        };
    }
}

public sealed class NotConstraint(IConstraint inner) : ConstraintBase
{
    public override bool Matches(object? actual) => !inner.Matches(actual);
}

public sealed class OrConstraint(IConstraint left, IConstraint right) : ConstraintBase
{
    public override bool Matches(object? actual) => left.Matches(actual) || right.Matches(actual);
}

public sealed class PropertyConstraint(string propertyName, IConstraint inner) : ConstraintBase
{
    public override bool Matches(object? actual)
    {
        if (actual is null)
            return false;

        var property = actual.GetType().GetProperty(propertyName);
        if (property is null)
            return false;

        return inner.Matches(property.GetValue(actual));
    }
}

public sealed class ContainsConstraint(object? expected) : ConstraintBase
{
    public override bool Matches(object? actual)
        => actual is IEnumerable enumerable && enumerable.Cast<object?>().Any(item => object.Equals(item, expected));
}

public sealed class EquivalentConstraint(IEnumerable expected) : ConstraintBase
{
    public override bool Matches(object? actual)
    {
        if (actual is not IEnumerable actualEnumerable)
            return false;

        var expectedItems = expected.Cast<object?>().ToList();
        var actualItems = actualEnumerable.Cast<object?>().ToList();

        return expectedItems.Count == actualItems.Count
            && expectedItems.All(expectedItem => actualItems.Count(actualItem => object.Equals(actualItem, expectedItem))
                == expectedItems.Count(item => object.Equals(item, expectedItem)));
    }
}
