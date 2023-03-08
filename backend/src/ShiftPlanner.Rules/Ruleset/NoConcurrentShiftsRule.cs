using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules.Ruleset;

public class NoConcurrentShiftsRule : IRule
{
    private readonly ISolverParameters _parameters;

    public NoConcurrentShiftsRule(ISolverParameters parameters)
    {
        _parameters = parameters;
    }

    public void Apply(IScheduleConstraintReceiver target)
    {
        foreach (var employee in _parameters.AllEmployees)
        {
            foreach (var day in _parameters.Days)
            {
                var employeesShiftsThatDay = _parameters.Workplaces
                    .Select(w => new ShiftAssignment(employee, day, w))
                    .ToArray();

                target.AddSumConstraint(employeesShiftsThatDay, Constraint.Lte, 1);
            }
        }
    }
}
