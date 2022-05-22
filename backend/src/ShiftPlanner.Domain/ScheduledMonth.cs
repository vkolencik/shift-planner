namespace ShiftPlanner.Domain;

public record ScheduledMonth(int Year, int Month)
{
    public IReadOnlyCollection<Day> Days => Enumerable.Range(1, DateTime.DaysInMonth(Year, Month))
        .Select(d => new Day(Year, Month, d))
        .ToArray();
}
