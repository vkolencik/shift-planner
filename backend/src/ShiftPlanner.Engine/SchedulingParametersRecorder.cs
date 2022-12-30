using ShiftPlanner.Domain;
using ShiftPlanner.Rules;

namespace ShiftPlanner.Engine;

public class SchedulingParametersRecorder : ISchedulingParametersRecorder
{
    private readonly Dictionary<Employee, EmployeeStandard> _recordedStandards = new Dictionary<Employee, EmployeeStandard>();

    public void RecordEmployeeStandard(Employee employeeMonthEmployee, EmployeeStandard employeeStandard)
    {
        _recordedStandards.Add(employeeMonthEmployee, employeeStandard);
    }

    public void Reset()
    {
        _recordedStandards.Clear();
    }

    public SolutionParameters GetParameters => new SolutionParameters(_recordedStandards);
}
