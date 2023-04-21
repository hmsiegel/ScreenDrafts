namespace ScreenDrafts.Domain.Common.Contracts;

public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Gets the atomic value of the value object.
    /// </summary>
    /// <returns>The collection of objects representing the value object values.</returns>
    public abstract IEnumerable<object?> GetAtomicValues();

    /// <inheritdoc />
    public bool Equals(ValueObject? other) => Equals(obj: other);

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;

        var valueObject = (ValueObject)obj;

        return GetAtomicValues().SequenceEqual(valueObject.GetAtomicValues());
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !Equals(left, right);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Aggregate(
            default(int),
            HashCode.Combine);
    }
}