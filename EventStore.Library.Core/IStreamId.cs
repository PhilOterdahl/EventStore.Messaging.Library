namespace EventStore.Library.Core;

public interface IStreamId
{
    public string Value { get; }
    public string Category { get; }
}