using ShiftPlanner.Domain;

namespace ShiftPlanner.GoogleSheet;

public interface IEmployeeService
{
    Task<IReadOnlyCollection<Employee>> GetEmployees(IList<Workplace> workplaces);
}
