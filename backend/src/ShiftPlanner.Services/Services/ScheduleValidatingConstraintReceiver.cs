using ShiftPlanner.Domain;
using ShiftPlanner.Rules;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

internal class ScheduleValidatingConstraintReceiver : IScheduleValidatingConstraintReceiver
{
    private readonly Schedule _schedule;
    private readonly List<ScheduleValidationError> _errors = new List<ScheduleValidationError>();

    public ScheduleValidatingConstraintReceiver(Schedule schedule)
    {
        _schedule = schedule;
    }

    public void AddSumConstraint(IReadOnlyCollection<ShiftAssignment> shifts, Constraint constraint, uint bound)
    {
        var shiftsInSchedule = shifts
            .Where(
                rs => _schedule.Shifts.Any(
                    ss =>
                        ss.Workplace == rs.Shift.Workplace
                        && ss.Day == rs.Shift.Day
                        && ss.User.UserName == rs.Employee.Name))
            .ToArray();

        if (shiftsInSchedule.Count() == bound)
        {
            return;
        }
        
        _errors.Add(new ScheduleValidationError(shiftsInSchedule.Select(s => s.Shift).ToArray(), null, ScheduleErrorType.Unknown));
        
    }

    public void AddVariableConstraint(ShiftAssignment shift, Constraint constraint, int value)
    {
        throw new NotImplementedException();
    }

    public void AddSumRelationConstraint(IReadOnlyCollection<ShiftAssignment> shifts1, Constraint constraint, IReadOnlyCollection<ShiftAssignment> shifts2)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<ScheduleValidationError> GetErrors() => _errors;
}
