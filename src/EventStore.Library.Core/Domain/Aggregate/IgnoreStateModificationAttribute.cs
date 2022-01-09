namespace EventStore.Library.Core.Domain.Aggregate;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class IgnoreStateModificationAttribute : Attribute
{
}