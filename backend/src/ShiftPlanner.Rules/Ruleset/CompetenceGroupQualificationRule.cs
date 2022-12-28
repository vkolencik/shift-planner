using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules.Ruleset;

public class CompetenceGroupQualificationRule : IRule
{
    private readonly IMonthScheduleParameters _parameters;

    public CompetenceGroupQualificationRule(IMonthScheduleParameters parameters)
    {
        _parameters = parameters;
    }

    public void Apply(IScheduleConstraintReceiver target)
    {
        foreach (var competenceGroup in _parameters.CompetenceGroups)
        {
            foreach (var day in _parameters.Days)
            {
                IReadOnlyCollection<ShiftAssignment> employeesLevel1AndWorkplacesCombinations = _parameters.Employees
                    .Where(e => e.CompetenceLevel == CompetenceLevel.Level1)
                    .SelectMany(e => competenceGroup.Workplaces.Select(w => new ShiftAssignment(e, day, w)))
                    .ToArray();

                target.AddSumConstraint(employeesLevel1AndWorkplacesCombinations, Constraint.Gte, 1);
            }
        }

    }
}
