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
}
