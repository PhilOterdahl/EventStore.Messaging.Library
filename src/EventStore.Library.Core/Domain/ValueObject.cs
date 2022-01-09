namespace EventStore.Library.Core.Domain;

/// <summary>
/// Base class for Value Objects.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// To be overridden in inheriting classes for providing a collection of atomic values of
    /// this Value Object.
    /// </summary>
    /// <returns>Collection of atomic values.</returns>
    protected abstract IEnumerable<object> GetAtomicValues();

    /// <summary>
    /// Compares two Value Objects according to atomic values returned by <see cref="GetAtomicValues"/>.
    /// </summary>
    /// <param name="other">Object to compare to.</param>
    /// <returns>True if objects are considered equal.</returns>
    public override bool Equals(object other)
    {
        if (other == null || other.GetType() != GetType())
            return false;

        var values = GetAtomicValues().ToArray();
        var otherValues = ((ValueObject)other).GetAtomicValues().ToArray();

        if (values.Length != otherValues.Length)
            return false;

        for (var i = 0; i < values.Length; i++)
        {
            if (values[i] is null ^ otherValues[i] is null)
                return false;

            if (values[i] != null && !values[i].Equals(otherValues[i]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Returns hashcode value calculated according to a collection of atomic values
    /// returned by <see cref="GetAtomicValues"/>.
    /// </summary>
    /// <returns>Hashcode value.</returns>
    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }

    public static bool operator ==(ValueObject x, ValueObject y)
    {
        if (x is null && y is null)
            return true;

        if (x is null || y is null)
            return false;

        return x.Equals(y);
    }

    public static bool operator !=(ValueObject x, ValueObject y)
    {
        return !(x == y);
    }
}