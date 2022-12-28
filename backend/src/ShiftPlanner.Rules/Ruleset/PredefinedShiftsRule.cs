using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules.Ruleset;

public class PredefinedShiftsRule : IRule
{
    private readonly IMonthScheduleParameters _parameters;

    public PredefinedShiftsRule(IMonthScheduleParameters parameters)
    {
        _parameters = parameters;
    }

    public void Apply(IScheduleConstraintReceiver target)
    {
        foreach (var fixedShift in _parameters.FixedShifts)
        {
            target.AddVariableConstraint(fixedShift, Constraint.Eq, 1);
        }
    }
}
