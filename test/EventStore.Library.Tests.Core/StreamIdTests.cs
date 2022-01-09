using System;
using EventStore.Library.Tests.Core.TestAggregates;
using FluentAssertions;
using Xunit;

namespace EventStore.Library.Tests.Core;

public class StreamIdTests
{
    [Fact]
    public void Stream_id_can_not_be_null()
    {
        Assert.Throws<ArgumentNullException>(() => new UserId(null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Stream_id_can_not_be_whitespace_or_empty(string id)
    {
        Assert.Throws<ArgumentException>(() => new UserId(id));
    }

    [Fact]
    public void Stream_id_writes_category_dash_id_when_calling_to_string()
    {
        var id = UserId.Create();
        id.ToString().Should().StartWith(id.Category);
    }
}