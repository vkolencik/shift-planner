using System.Linq;
using Xunit;

namespace ShiftPlanner.Domain.Tests;

public class ScheduledMonthShould
{
    [Fact]
    public void ReturnDaysOfMonth()
    {
        var days = new ScheduledMonth(2022, 2).Days;

        Assert.Equal(28, days.Select(d => d.Date.Day).ToHashSet().Count);
        Assert.All(days, d => Assert.Equal(2022, d.Date.Year));
        Assert.All(days, d => Assert.Equal(2, d.Date.Month));
    }

    [Theory]
    [InlineData(2022, 12, 2022, 11)]
    [InlineData(2023, 1, 2022, 12)]
    public void ReturnPreviousMonth(int year, int month, int expectedPreviousYear, int expectedPreviousMonth)
    {
        var scheduledMonth = new ScheduledMonth(year, month);
        var previousScheduledMonth = scheduledMonth.Previous;
        Assert.Equal(new ScheduledMonth(expectedPreviousYear, expectedPreviousMonth), previousScheduledMonth);
    }
}
