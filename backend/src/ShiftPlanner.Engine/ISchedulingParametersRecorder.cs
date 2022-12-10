using ShiftPlanner.Contracts;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Engine;

internal interface ISchedulingParametersRecorder
{
    void RecordEmployeeStandard(Employee employeeMonthEmployee, EmployeeStandard employeeStandard);
}
