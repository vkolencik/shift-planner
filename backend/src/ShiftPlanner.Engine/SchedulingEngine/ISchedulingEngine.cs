using ShiftPlanner.Domain;

namespace ShiftPlanner.Engine.SchedulingEngine;

internal interface ISchedulingEngine
{
    Schedule? Solve();
    void AddSumConstraint(IReadOnlyCollection<ShiftAssignment> shifts, Constraint constraint, uint bound);
    void AddVariableConstraint(ShiftAssignment shift, Constraint constraint, int value);
    void AddSumRelationConstraint(IReadOnlyCollection<ShiftAssignment> shifts1, Constraint constraint, IReadOnlyCollection<ShiftAssignment> shifts2);
}
