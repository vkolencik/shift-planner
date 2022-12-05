namespace ShiftPlanner.Domain.Services;

public interface IEmployeeInfoService
{
    Task<IReadOnlyCollection<EmployeeInfo>> GetEmployees(IList<Workplace> workplaces);
}
