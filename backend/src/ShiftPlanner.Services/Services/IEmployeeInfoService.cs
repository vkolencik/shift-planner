using ShiftPlanner.Domain;

namespace ShiftPlanner.Services.Services;

public interface IEmployeeInfoService
{
    Task<IReadOnlyCollection<EmployeeInfo>> GetEmployees(IList<Workplace> workplaces);
}
