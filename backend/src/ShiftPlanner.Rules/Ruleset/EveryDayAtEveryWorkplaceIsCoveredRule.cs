using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules.Ruleset;

public class EveryDayAtEveryWorkplaceIsCoveredRule : IRule
{
    private readonly ISolverParameters _parameters;
    private readonly IReadOnlyCollection<Workplace> _optionalWorkplaces;

    public EveryDayAtEveryWorkplaceIsCoveredRule(ISolverParameters parameters, IReadOnlyCollection<Workplace> optionalWorkplaces)
    {
        _parameters = parameters;
        _optionalWorkplaces = optionalWorkplaces;
    }

    public void Apply(IScheduleConstraintReceiver engine)
    {
        foreach (var day in _parameters.Days)
        {
            foreach (var workplace in _parameters.Workplaces.Except(_optionalWorkplaces))
            {
                var shiftsOnDayAtWorkplace = _parameters.AllEmployees
                    .Select(e => new ShiftAssignment(e, day, workplace))
                    .ToArray();

                engine.AddSumConstraint(shiftsOnDayAtWorkplace, Constraint.Eq, 1);
            }
        }
    }
}
