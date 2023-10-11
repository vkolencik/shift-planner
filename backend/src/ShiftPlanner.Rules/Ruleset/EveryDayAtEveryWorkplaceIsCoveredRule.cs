using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules.Ruleset;

public class EveryDayAtEveryWorkplaceIsCoveredRule : IRule
{
    private readonly ISolverParameters _parameters;

    public EveryDayAtEveryWorkplaceIsCoveredRule(ISolverParameters parameters)
    {
        _parameters = parameters;
    }

    public void Apply(IScheduleConstraintReceiver engine)
    {
        foreach (var day in _parameters.Days)
        {
            foreach (var workplace in _parameters.Workplaces)
            {
                var shiftsOnDayAtWorkplace = _parameters.AllEmployees
                    .Select(e => new ShiftAssignment(e, day, workplace))
                    .ToArray();

                engine.AddSumConstraint(shiftsOnDayAtWorkplace, workplace.Optional ? Constraint.Lte : Constraint.Eq, 1);
            }
        }

        var optionalWorkplaceShifts = _parameters.Workplaces.Where(w => w.Optional)
            .SelectMany(w => _parameters.Days.SelectMany(d => _parameters.AllEmployees.Select(e => new ShiftAssignment(e, d, w))))
            .ToArray();
        engine.MaximizeSum(optionalWorkplaceShifts);
    }
}
