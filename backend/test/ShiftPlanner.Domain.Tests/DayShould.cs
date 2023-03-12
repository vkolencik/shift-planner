using Xunit;

namespace ShiftPlanner.Domain.Tests;

public class DayShould
{
    [Fact]
    public void ReturnScheduledMonth()
    {
        var day = new Day(2023, 4, 23);
        
        var scheduledMonth = day.ScheduledMonth;
        
        Assert.Equal(new ScheduledMonth(2023, 4), scheduledMonth);
    }
}
