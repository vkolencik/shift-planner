using ShiftPlanner.Domain;
using ShiftPlanner.GoogleSheet;

namespace ShiftPlanner.ScheduleCalculator;

internal class SchedulingParametersAggregator : ISchedulingParametersAggregator
{
    private readonly IEmployeeInfoService _employeeInfoService;

    public SchedulingParametersAggregator(IEmployeeInfoService employeeInfoService)
    {
        _employeeInfoService = employeeInfoService;
    }

    public async Task<MonthScheduleParameters> GetSchedulingParameters(ScheduledMonth month)
    {
        var employeeInfos = await _employeeInfoService.GetEmployees(Workplace.Workplaces);


        var employeeMonths = employeeInfos.Select(
            ei => new EmployeeMonth(
                new Employee(ei.Name, ei.CompetenceLevel, ei.Capabilities, ei.Workload)))
            .ToArray();
        
        return new MonthScheduleParameters(
            month,
            employeeMonths,
            CompetenceGroup.CompetenceGroups);
    }
}
