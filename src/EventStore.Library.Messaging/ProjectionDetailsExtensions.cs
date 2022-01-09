using EventStore.Client;

namespace EventStore.Library.Messaging;

internal static class ProjectionDetailsExtensions
{
    public static IEnumerable<ProjectionDetails> WhereProjection(this IEnumerable<ProjectionDetails> projectionDetails, string name) =>
        projectionDetails.Where(projection => projection.Name == name);

    public static IEnumerable<ProjectionDetails> WhereRunning(this IEnumerable<ProjectionDetails> projectionDetails) =>
        projectionDetails.Where(projection => projection.Status == "Running");
}