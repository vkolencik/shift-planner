using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules.Ruleset;

public class ExternalEmployeeRule : IRule
{
    private readonly IMonthScheduleParameters _parameters;


    public ExternalEmployeeRule(IMonthScheduleParameters parameters)
    {
        _parameters = parameters;
    }

    public void Apply(IScheduleConstraintReceiver target)
    {
        foreach (var externalEmployee in _parameters.AllEmployees.Except(_parameters.Employees))
        {
            foreach (var day in _parameters.Days)
            {
                foreach (var workplace in _parameters.Workplaces)
                {
                    if (!_parameters.FixedShifts.Any(fs => fs.Shift.Workplace == workplace && fs.Shift.Day == day && fs.Employee == externalEmployee))
                    {
                        target.AddVariableConstraint(new ShiftAssignment(externalEmployee, day, workplace), Constraint.Eq, 0);
                    }
                }
            }
        }
    }
}
