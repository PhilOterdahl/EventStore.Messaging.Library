using EventStore.Library.Core.Domain;

namespace EventStore.Library.Core;

public abstract class StreamId : ValueObject, IStreamId
{
    protected StreamId(string value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Id can not be empty or whitespace", nameof(value));

        Value = value;
    }

    public string Value { get; }
    public abstract string Category { get; }

    public override string ToString() => $"{Category}-{Value}";
    public static implicit operator string?(StreamId id) => id?.Value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Category;
        yield return Value;
    }
}