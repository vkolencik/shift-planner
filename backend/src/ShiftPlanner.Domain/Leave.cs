namespace ShiftPlanner.Domain;

public record Leave
{
    public Leave(Day from, Day to)
    {
        if (to.Date < from.Date)
        {
            throw new ArgumentException("End date cannot be before start date", nameof(to));
        }

        this.From = from;
        this.To = to;
    }

    public int LengthDays => (To.Date.ToDateTime(TimeOnly.MinValue) - From.Date.ToDateTime(TimeOnly.MinValue)).Days + 1;

    public IEnumerable<Day> Days
    {
        get
        {
            for (var date = From.Date; date <= To.Date; date = date.AddDays(1))
            {
                yield return new Day(date);
            }
        }
    }
    
    public Day From { get; init; }
    public Day To { get; init; }

    public void Deconstruct(out Day from, out Day to)
    {
        from = this.From;
        to = this.To;
    }
}
