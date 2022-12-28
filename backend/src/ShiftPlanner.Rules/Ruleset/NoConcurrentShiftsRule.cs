using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules.Rules;

public class NoConcurrentShiftsRule : IRule
{
    private readonly ISolverParameters _schedulingParameters;

    public NoConcurrentShiftsRule(ISolverParameters schedulingParameters)
    {
        _schedulingParameters = schedulingParameters;
    }

    public void Apply(IScheduleConstraintReceiver target)
    {
        foreach (var employee in _schedulingParameters.Employees)
        {
            foreach (var day in _schedulingParameters.Days)
            {
                var employeesShiftsThatDay = _schedulingParameters.Workplaces
                    .Select(w => new ShiftAssignment(employee, day, w))
                    .ToArray();

                target.AddSumConstraint(employeesShiftsThatDay, Constraint.Lte, 1);
            }
        }
    }
}
