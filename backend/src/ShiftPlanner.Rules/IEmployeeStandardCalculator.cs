using ShiftPlanner.Contracts;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules;

public interface IEmployeeStandardCalculator
{
    EmployeeStandard ComputeStandard(
        EmployeeMonth employeeMonth,
        double multiplier,
        double flexibilityMultiplier = 1);
}
