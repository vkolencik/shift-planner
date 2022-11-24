namespace ShiftPlanner.Engine.SchedulingEngine;

internal record VariableName(string Value)
{
    public static VariableName Schedule(int employee, int day, int workplace) =>
        new VariableName($"shifts_e{employee}_d{day}_w{workplace}");
}
