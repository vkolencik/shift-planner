using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules.Rules;

internal class EveryDayAtEveryWorkplaceIsCoveredRule : IRule
{
    private readonly ISolverParameters _schedulingParameters;

    public EveryDayAtEveryWorkplaceIsCoveredRule(ISolverParameters schedulingParameters)
    {
        _schedulingParameters = schedulingParameters;
    }

    public void Apply(IScheduleConstraintReceiver engine)
    {
        foreach (var day in _schedulingParameters.Days)
        {
            foreach (var workplace in _schedulingParameters.Workplaces)
            {
                var shiftsOnDayAtWorkplace = _schedulingParameters.Employees
                    .Select(e => new ShiftAssignment(e, day, workplace))
                    .ToArray();

                engine.AddSumConstraint(shiftsOnDayAtWorkplace, Constraint.Eq, 1);
            }
        }
    }
}
