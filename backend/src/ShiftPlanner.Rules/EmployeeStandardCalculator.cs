using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules;

public class EmployeeStandardCalculator : IEmployeeStandardCalculator
{
    private readonly ScheduleFlexibility _flexibility;
    private readonly IMonthScheduleParameters _parameters;

    public EmployeeStandardCalculator(ScheduleFlexibility flexibility, IMonthScheduleParameters parameters)
    {
        _flexibility = flexibility;
        _parameters = parameters;
    }

    public EmployeeStandard ComputeStandard(
        EmployeeMonth employeeMonth,
        double multiplier,
        double flexibilityMultiplier = 1)
    {
        const int leaveFactor = 1;
        var daysCount = (double) _parameters.Month.Days.Count;
        var totalRequiredCapacity = daysCount * _parameters.Workplaces.Count;
        var capacityReductionByLeaves = employeeMonth.Leaves.Where(l => l.ReducesCapacity).Sum(l => l.LengthDays);
        var totalEmployeeCapacity = (double) _parameters.EmployeeMonths.Sum(em => daysCount - leaveFactor * em.Leaves.Where(l => l.ReducesCapacity).Sum(l => l.LengthDays));
        var employeeCapacity = daysCount - leaveFactor * capacityReductionByLeaves;
        var flexibilityModifier = flexibilityMultiplier * _flexibility.Degree / 10;
        var preciseStandard = totalRequiredCapacity * employeeCapacity / totalEmployeeCapacity;
        var standardLb = Math.Max(employeeMonth.Employee.Workload.MinimumShiftsPerMonth * employeeCapacity / daysCount, multiplier * preciseStandard - flexibilityModifier);
        var standardUb = Math.Min(employeeMonth.Employee.Workload.MaximumShiftsPerMonth * employeeCapacity / daysCount, multiplier * preciseStandard + flexibilityModifier);

        return new EmployeeStandard(
            (uint) Math.Floor(standardLb),
            (uint) Math.Ceiling(standardUb + 0));
    }
}
