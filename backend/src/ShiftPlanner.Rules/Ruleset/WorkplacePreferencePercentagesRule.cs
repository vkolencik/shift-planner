using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules.Ruleset;

public class WorkplacePreferencePercentagesRule : IRule
{
    private readonly IEmployeeStandardCalculator _standardCalculator;
    private readonly IMonthScheduleParameters _parameters;

    public WorkplacePreferencePercentagesRule(IEmployeeStandardCalculator standardCalculator, IMonthScheduleParameters parameters)
    {
        _standardCalculator = standardCalculator;
        _parameters = parameters;
    }

    public void Apply(IScheduleConstraintReceiver target)
    {
        _parameters.EmployeeMonths.ToList().ForEach(em => AddEmployeePreferencePercentages(target, em));
    }
    
    private void AddEmployeePreferencePercentages(
        IScheduleConstraintReceiver target,
        EmployeeMonth employeeMonth)
    {
        foreach (var capability in employeeMonth.Employee.Capabilities.WorkplaceCapabilities
                     .OfType<WorkplaceCapability.Green>())
        {
            var employeeStandardForWorkplace = _standardCalculator.ComputeStandard(employeeMonth, (double) capability.PreferencePercentage / 100);

            var employeeShiftsInMonthForWorkspace = _parameters.Month.Days
                .Select(d => new ShiftAssignment(employeeMonth.Employee, d, capability.Workplace))
                .ToList();

            target.AddSumConstraint(employeeShiftsInMonthForWorkspace, Constraint.Gte, employeeStandardForWorkplace.Minimum > 0 ? 1U : 0U);
            target.AddSumConstraint(employeeShiftsInMonthForWorkspace, Constraint.Lte, employeeStandardForWorkplace.Maximum);
        }
    }
}
