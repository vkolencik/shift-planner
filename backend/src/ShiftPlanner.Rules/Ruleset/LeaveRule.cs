using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules.Ruleset;

public class LeaveRule : IRule
{
    private readonly IMonthScheduleParameters _parameters;

    public LeaveRule(IMonthScheduleParameters parameters)
    {
        _parameters = parameters;
    }

    public void Apply(IScheduleConstraintReceiver target)
    {
        foreach (var employeeMonth in _parameters.EmployeeMonths)
        {
            foreach (var leave in employeeMonth.Leaves)
            {
                foreach (var day in leave.Days)
                {
                    foreach (var workplace in _parameters.Workplaces)
                    {
                        target.AddVariableConstraint(new ShiftAssignment(employeeMonth.Employee, day, workplace), Constraint.Eq, 0);
                    }
                }
            }
        }
    }
}
