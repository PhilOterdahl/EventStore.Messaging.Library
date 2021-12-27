namespace EventStore.Library.Core;

public static class SystemProjections
{
    public const string ByEventType = "$by_event_type";
    public const string ByCategory = "$by_category";
    public const string ByCorrelation = "$by_correlation_id";
}