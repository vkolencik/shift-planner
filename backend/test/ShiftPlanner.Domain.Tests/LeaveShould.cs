using System;
using Xunit;

namespace ShiftPlanner.Domain.Tests;

public class LeaveShould
{
    [Theory]
    [InlineData(15, 1)]
    [InlineData(16, 2)]
    [InlineData(28, 14)]
    public void HaveProperLength(int dayOfMonth, int expectedLength)
    {
        var leave = new Leave(new DateOnly(2022, 2, 15), new DateOnly(2022, 2, dayOfMonth));

        var lengthDays = leave.LengthDays;
        
        Assert.Equal(expectedLength, lengthDays);
    }

    [Fact]
    public void Throw_GivenInvalidDateRange()
    {
        Assert.Throws<ArgumentException>(() => new Leave(new DateOnly(2022, 2, 15), new DateOnly(2022, 2, 14)));
    }
}
