namespace ShiftPlanner.Domain;

public record struct ScheduleFlexibility(uint Degree)
{
    public static ScheduleFlexibility Strict => new ScheduleFlexibility(0);
}
