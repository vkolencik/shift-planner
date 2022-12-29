using ShiftPlanner.Contracts;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules;

public interface ISchedulingParametersRecorder
{
    void RecordEmployeeStandard(Employee employeeMonthEmployee, EmployeeStandard employeeStandard);
}
