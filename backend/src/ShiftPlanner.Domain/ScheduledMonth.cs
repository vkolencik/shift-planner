namespace ShiftPlanner.Domain;

public record ScheduledMonth(int Year, int Month)
{
    static IReadOnlyCollection<Day> GetDaysOfMonth(ScheduledMonth month)
    {
        return Enumerable
            .Range(1, DateTime.DaysInMonth(month.Year, month.Month))
            .Select(d => new Day(new DateOnly(2000,1,1)))
            .ToArray();
    }
}
    
