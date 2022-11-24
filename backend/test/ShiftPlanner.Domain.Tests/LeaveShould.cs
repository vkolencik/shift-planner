using System;
using System.Collections.Generic;
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
        var leave = new Leave(new Day(2022, 2, 15), new Day(2022, 2, dayOfMonth));

        var lengthDays = leave.LengthDays;

        Assert.Equal(expectedLength, lengthDays);
    }

    [Fact]
    public void Throw_GivenInvalidDateRange()
    {
        Assert.Throws<ArgumentException>(() => new Leave(new Day(new DateOnly(2022, 2, 15)), new Day(new DateOnly(2022, 2, 14))));
    }

    [Fact]
    public void ReturnIndividualDays()
    {
        var firstDay = new Day(2022, 2, 27);
        var secondDay = new Day(2022, 2, 28);

        var leave = new Leave(firstDay, secondDay);

        Assert.Equal(new HashSet<Day> {firstDay, secondDay}, leave.Days);
    }
}
