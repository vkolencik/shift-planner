using ShiftPlanner.Domain;

namespace ShiftScheduler.Rules;

public interface IScheduleConstraintReceiver
{
    void AddSumConstraint(IReadOnlyCollection<ShiftAssignment> shifts, Constraint constraint, uint bound);
    void AddVariableConstraint(ShiftAssignment shift, Constraint constraint, int value);
    void AddSumRelationConstraint(IReadOnlyCollection<ShiftAssignment> shifts1, Constraint constraint, IReadOnlyCollection<ShiftAssignment> shifts2);
}
