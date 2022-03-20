namespace ShiftPlanner.Engine;

internal record Day(DateOnly Date)
{
    public int Index() => Date.Day;
}