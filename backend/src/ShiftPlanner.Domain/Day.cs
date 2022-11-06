namespace ShiftPlanner.Domain;

public record Day(DateOnly Date) : IComparable<Day>
{
    public DayOfWeek DayOfWeek => Date.DayOfWeek;

    public Day(int year, int month, int day) : this(new DateOnly(year, month, day))
    {
    }

    public Day(ScheduledMonth month, int day) : this(month.Year, month.Month, day)
    {
    }

    public int Index() => Date.Day;

    public static bool operator <(Day? left, Day? right)
    {
        return Comparer<Day>.Default.Compare(left, right) < 0;
    }

    public static bool operator >(Day? left, Day? right)
    {
        return Comparer<Day>.Default.Compare(left, right) > 0;
    }

    public static bool operator <=(Day? left, Day? right)
    {
        return Comparer<Day>.Default.Compare(left, right) <= 0;
    }

    public static bool operator >=(Day? left, Day? right)
    {
        return Comparer<Day>.Default.Compare(left, right) >= 0;
    }

    public int CompareTo(Day? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return Date.CompareTo(other.Date);
    }
}
