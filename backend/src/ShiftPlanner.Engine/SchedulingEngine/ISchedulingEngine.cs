using ShiftPlanner.Domain;

namespace ShiftPlanner.Engine.SchedulingEngine;

internal interface ISchedulingEngine
{
    Schedule Solve();
    void AddSumConstraint(IReadOnlyCollection<Shift> shifts, Constraint constraint, int bound);
}
