using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine;
using ShiftPlanner.Rules;

namespace ShiftPlanner.Engine;

internal static class Rules
{
    public static void EnforceFixedShifts(MonthScheduleParameters monthScheduleParameters, ISchedulingEngine engine)
    {
        foreach (var fixedShift in monthScheduleParameters.FixedShifts)
        {
            engine.AddVariableConstraint(fixedShift, Constraint.Eq, 1);
        }
    }
}
