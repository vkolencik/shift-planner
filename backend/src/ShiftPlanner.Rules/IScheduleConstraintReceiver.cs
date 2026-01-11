using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules;

public interface IScheduleConstraintReceiver
{
    void AddSumConstraint(IReadOnlyCollection<ShiftAssignment> shifts, Constraint constraint, uint bound);
    void AddVariableConstraint(ShiftAssignment shift, Constraint constraint, uint value);
    void AddSumRelationConstraint(IReadOnlyCollection<ShiftAssignment> shifts1, Constraint constraint, IReadOnlyCollection<ShiftAssignment> shifts2);
    void MaximizeSum(IReadOnlyCollection<ShiftAssignment> shiftAssignments);
}
