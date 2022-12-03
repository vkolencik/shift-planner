using ShiftPlanner.Domain;

namespace ShiftPlanner.GoogleSheet;

public interface IEmployeeService
{
    Task<IReadOnlyCollection<EmployeeInfo>> GetEmployees(IList<Workplace> workplaces);
}
