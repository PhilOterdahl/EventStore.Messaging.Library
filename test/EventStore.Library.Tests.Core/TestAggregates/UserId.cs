using System;
using EventStore.Library.Core;

namespace EventStore.Library.Tests.Core.TestAggregates;

public class UserId : StreamId
{
    public UserId(string value) : base(value)
    {
    }

    public static UserId Create() => new(Guid.NewGuid().ToString());

    public override string Category => "User";
}