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

    public int Year => Date.Year;
    public int Month => Date.Month;
    public int DayOfMonth => Date.Day;
    public ScheduledMonth ScheduledMonth => new ScheduledMonth(Year, Month);

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

    public virtual bool Equals(Day? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Date.Equals(other.Date);
    }

    public override int GetHashCode()
    {
        return Date.GetHashCode();
    }

    public override string ToString() => Date.ToString();
}
