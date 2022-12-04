using ShiftPlanner.Domain;

namespace ShiftPlanner.GoogleSheet;

public interface IEmployeeInfoService
{
    Task<IReadOnlyCollection<EmployeeInfo>> GetEmployees(IList<Workplace> workplaces);
}
