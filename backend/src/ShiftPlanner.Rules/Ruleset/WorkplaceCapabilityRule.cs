using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules.Ruleset;

public class WorkplaceCapabilityRule : IRule
{
    private readonly ISolverParameters _parameters;

    public WorkplaceCapabilityRule(ISolverParameters parameters)
    {
        _parameters = parameters;
    }

    public void Apply(IScheduleConstraintReceiver target)
    {
        foreach (var employee in _parameters.Employees)
        {
            var incapableWorkplaces = _parameters.Workplaces.Except(employee.Capabilities.AllowedWorkplaces);
        
            foreach (var incapableWorkplace in incapableWorkplaces)
            {
                foreach (var day in _parameters.Days)
                {
                    target.AddVariableConstraint(new ShiftAssignment(employee, day, incapableWorkplace), Constraint.Eq, 0);
                }
            }
        }
    }
}
