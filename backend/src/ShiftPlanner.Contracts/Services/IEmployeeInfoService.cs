using ShiftPlanner.Domain;

namespace ShiftPlanner.Contracts.Services;

public interface IEmployeeInfoService
{
    Task<IReadOnlyCollection<EmployeeInfo>> GetEmployees(IList<Workplace> workplaces);
}
