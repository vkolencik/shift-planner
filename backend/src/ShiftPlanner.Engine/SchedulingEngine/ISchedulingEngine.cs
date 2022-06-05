using ShiftPlanner.Domain;

namespace ShiftPlanner.Engine.SchedulingEngine;

internal interface ISchedulingEngine
{
    Schedule Solve();
    void AddSumConstraint(IReadOnlyCollection<ShiftAssignment> shifts, Constraint constraint, uint bound);
    void AddVariableConstraint(ShiftAssignment shift, Constraint constraint, int value);
}
