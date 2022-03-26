namespace ShiftPlanner.Domain;

public record Day(DateOnly Date)
{
    public int Index() => Date.Day;

    public static IReadOnlyCollection<Day> CreateDaysForMonth(ScheduledMonth month)
    {
        return Enumerable.Range(1, DateTime.DaysInMonth(month.Year, month.Month))
            .Select(d => new Day(new DateOnly(month.Year, month.Month, d)))
            .ToArray();
    }
}
