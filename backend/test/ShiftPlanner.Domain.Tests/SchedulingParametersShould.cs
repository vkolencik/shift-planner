using System;
using System.Collections.Generic;
using Xunit;

namespace ShiftPlanner.Domain.Tests;

public class SchedulingParametersShould
{
    [Fact]
    public void ReturnWorkplaces()
    {
        var workplace1 = new Workplace("1");
        var workplace2 = new Workplace("2");

        var parameters = new MonthScheduleParameters(
            new ScheduledMonth(2022, 2),
            Array.Empty<EmployeeMonth>(),
            new[] {new CompetenceGroup(new[] {workplace1}), new CompetenceGroup(new[] {workplace2})},
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ISet<Workplace>>());

        Assert.Equal(new HashSet<Workplace> {workplace1, workplace2}, parameters.Workplaces);
    }
}
