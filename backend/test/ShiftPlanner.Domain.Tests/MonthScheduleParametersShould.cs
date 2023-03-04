using System;
using System.Collections.Generic;
using Xunit;

namespace ShiftPlanner.Domain.Tests;

public class MonthScheduleParametersShould
{
    [Fact]
    public void ReturnLastMonthShiftsForActiveEmployees()
    {
        var activeEmployee = new Employee("active_employee", CompetenceLevel.Level1, Capabilities.Empty, Workload.Infinite);
        var inactiveEmployee = new Employee("inactive_employee", CompetenceLevel.Level1, Capabilities.Empty, Workload.Infinite);

        var workplace = new Workplace("w1");

        var activeEmployeeLastMonthShift = new ShiftAssignment(activeEmployee, new Day(2000, 1, 1), workplace);
        var inactiveEmployeeLastMonthShift = new ShiftAssignment(inactiveEmployee, new Day(2000, 1, 2), workplace);
        var parameters = new MonthScheduleParameters(
            new ScheduledMonth(2000, 1), 
            new EmployeeMonth[] { new EmployeeMonth(activeEmployee) },
            Array.Empty<CompetenceGroup>(),
            new []
            {
                activeEmployeeLastMonthShift, 
                inactiveEmployeeLastMonthShift 
            },
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ISet<Workplace>>());

        var activeEmployeeLastMonthShifts = parameters.ActiveEmployeeLastMonthShifts;
        
        var returnedActiveEmployeeShift = Assert.Single(activeEmployeeLastMonthShifts);

        Assert.Equal(activeEmployeeLastMonthShift, returnedActiveEmployeeShift);
    } 
}
