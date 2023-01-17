using ShiftPlanner.Domain;
using ShiftPlanner.Rules;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

internal class ScheduleValidatingConstraintReceiver : IScheduleValidatingConstraintReceiver
{
    private readonly Schedule _schedule;

    public ScheduleValidatingConstraintReceiver(Schedule schedule)
    {
        _schedule = schedule;
    }

    public void AddSumConstraint(IReadOnlyCollection<ShiftAssignment> shifts, Constraint constraint, uint bound)
    {
        throw new NotImplementedException();
    }

    public void AddVariableConstraint(ShiftAssignment shift, Constraint constraint, int value)
    {
        throw new NotImplementedException();
    }

    public void AddSumRelationConstraint(IReadOnlyCollection<ShiftAssignment> shifts1, Constraint constraint, IReadOnlyCollection<ShiftAssignment> shifts2)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<ScheduleValidationError> GetErrors()
    {
        throw new NotImplementedException();
    }
}
