namespace ShiftPlanner.Domain;

public record ScheduledMonth(int Year, int Month) : IComparable<ScheduledMonth>
{
    public IReadOnlyCollection<Day> Days => Enumerable.Range(1, DateTime.DaysInMonth(Year, Month))
        .Select(d => new Day(Year, Month, d))
        .ToArray();

    public ScheduledMonth Previous
    {
        get
        {
            var previousMonthDate = new DateOnly(Year, Month, 1).AddMonths(-1);
            return new ScheduledMonth(previousMonthDate.Year, previousMonthDate.Month);
        }
    }

    public override string ToString() => $"{Year}-{Month:D2}";

    public int CompareTo(ScheduledMonth? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        var yearComparison = Year.CompareTo(other.Year);
        if (yearComparison != 0) return yearComparison;
        return Month.CompareTo(other.Month);
    }

    public static bool operator <(ScheduledMonth? left, ScheduledMonth? right)
    {
        return Comparer<ScheduledMonth>.Default.Compare(left, right) < 0;
    }

    public static bool operator >(ScheduledMonth? left, ScheduledMonth? right)
    {
        return Comparer<ScheduledMonth>.Default.Compare(left, right) > 0;
    }

    public static bool operator <=(ScheduledMonth? left, ScheduledMonth? right)
    {
        return Comparer<ScheduledMonth>.Default.Compare(left, right) <= 0;
    }

    public static bool operator >=(ScheduledMonth? left, ScheduledMonth? right)
    {
        return Comparer<ScheduledMonth>.Default.Compare(left, right) >= 0;
    }
}
