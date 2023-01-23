using ShiftPlanner.Domain;
using ShiftPlanner.Rules;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

internal class ScheduleValidatingConstraintReceiver : IScheduleValidatingConstraintReceiver
{
    private readonly Schedule _schedule;
    private readonly ScheduleErrorType _scheduleErrorType;
    private readonly List<ScheduleValidationError> _errors = new List<ScheduleValidationError>();

    public ScheduleValidatingConstraintReceiver(Schedule schedule, ScheduleErrorType scheduleErrorType)
    {
        _schedule = schedule;
        _scheduleErrorType = scheduleErrorType;
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

        var sumValue = shiftsInSchedule.Length;

        var isSatisfied = IsConstraintSatisfied(sumValue, constraint, bound);

        if (!isSatisfied)
        {
            var userName = GetUserName(shifts);
            var errorShifts = constraint == Constraint.Gte && userName is not null || shiftsInSchedule.Any() ? shiftsInSchedule : shifts;
            _errors.Add(new ScheduleValidationError(errorShifts.Select(s => s.Shift).ToArray(), GetUserName(shifts), _scheduleErrorType));
        }
    }

    private string? GetUserName(IReadOnlyCollection<ShiftAssignment> shifts)
    {
        var uniqueNames = shifts.Select(s => s.Employee.Name).ToHashSet();

        return uniqueNames.Count == 1 ? uniqueNames.Single() : null;
    }

    public void AddVariableConstraint(ShiftAssignment shift, Constraint constraint, uint value)
    {
        var shiftInSchedule = _schedule.Shifts.SingleOrDefault(ss => ss.Workplace == shift.Shift.Workplace && ss.Day == shift.Shift.Day);

        if (shiftInSchedule is null)
        {
            throw new InvalidOperationException($"Shift on {shift.Shift.Day} in {shift.Shift.Workplace} not found.");
        }

        var scheduleValue = shiftInSchedule.User.UserName == shift.Employee.Name ? 1 : 0;

        var isSatisfied = IsConstraintSatisfied(scheduleValue, constraint, value);

        if (!isSatisfied)
        {
            _errors.Add(new ScheduleValidationError(new[] {shift.Shift}, shift.Employee.Name, _scheduleErrorType));
        }
    }

    public void AddSumRelationConstraint(IReadOnlyCollection<ShiftAssignment> shifts1, Constraint constraint, IReadOnlyCollection<ShiftAssignment> shifts2)
    {
        var shifts1InSchedule = shifts1
            .Where(
                rs => _schedule.Shifts.Any(
                    ss =>
                        ss.Workplace == rs.Shift.Workplace
                        && ss.Day == rs.Shift.Day
                        && ss.User.UserName == rs.Employee.Name))
            .ToArray();

        var shifts2InSchedule = shifts2
            .Where(
                rs => _schedule.Shifts.Any(
                    ss =>
                        ss.Workplace == rs.Shift.Workplace
                        && ss.Day == rs.Shift.Day
                        && ss.User.UserName == rs.Employee.Name))
            .ToArray();

        var isSatisfied = IsConstraintSatisfied(shifts1InSchedule.Length, constraint, (uint) shifts2InSchedule.Length);
        
        if (!isSatisfied)
        {
            _errors.Add(new ScheduleValidationError(
                shifts1.Concat(shifts2).Select(s => s.Shift).ToHashSet().ToArray(),
                GetUserName(shifts1.Concat(shifts2).ToArray()),
                _scheduleErrorType));
        }
    }

    private static bool IsConstraintSatisfied(int testedValue, Constraint constraint, uint bound)
    {
        return constraint switch
        {
            Constraint.Eq => testedValue == bound,
            Constraint.Gte => testedValue >= bound,
            Constraint.Lte => testedValue <= bound,
            _ => throw new ArgumentOutOfRangeException(nameof(constraint), constraint, null)
        };
    }

    public IReadOnlyCollection<ScheduleValidationError> GetErrors() => _errors;
}
