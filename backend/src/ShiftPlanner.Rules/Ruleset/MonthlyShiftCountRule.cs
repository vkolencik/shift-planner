using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules.Ruleset;

public class MonthlyShiftCountRule : IRule
{
    private readonly IMonthScheduleParameters _parameters;
    private readonly ISchedulingParametersRecorder _recorder;
    private readonly IEmployeeStandardCalculator _standardCalculator;

    public MonthlyShiftCountRule(
        IEmployeeStandardCalculator standardCalculator,
        IMonthScheduleParameters parameters,
        ISchedulingParametersRecorder recorder)
    {
        _standardCalculator = standardCalculator;
        _parameters = parameters;
        _recorder = recorder;
    }

    public void Apply(IScheduleConstraintReceiver target)
    {
        _parameters.EmployeeMonths.ToList().ForEach(em => AddEmployeeStandardRules(target, _recorder, em));
    }
    
    private void AddEmployeeStandardRules(
        IScheduleConstraintReceiver engine,
        ISchedulingParametersRecorder recorder,
        EmployeeMonth employeeMonth)
    {
        var employeeStandard = _standardCalculator.ComputeStandard(employeeMonth, 1);

        var employeeShiftsInMonth = _parameters.Month.Days
            .SelectMany(d => _parameters.Workplaces.Select(w => new ShiftAssignment(employeeMonth.Employee, d, w)))
            .ToList();

        engine.AddSumConstraint(employeeShiftsInMonth, Constraint.Gte, employeeStandard.Minimum);
        engine.AddSumConstraint(employeeShiftsInMonth, Constraint.Lte, employeeStandard.Maximum);
        recorder.RecordEmployeeStandard(employeeMonth.Employee, employeeStandard);
    }
}
