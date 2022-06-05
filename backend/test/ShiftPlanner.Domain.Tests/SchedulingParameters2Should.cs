using System;
using System.Collections.Generic;
using Xunit;

namespace ShiftPlanner.Domain.Tests;

public class SchedulingParameters2Should
{
    [Fact]
    public void ReturnWorkplaces()
    {
        var workplace1 = new Workplace("1");
        var workplace2 = new Workplace("2");
        
        var parameters = new SchedulingParameters2(
            new ScheduledMonth(2022, 2),
            Array.Empty<EmployeeMonth>(),
            new [] { new CompetenceGroup(new [] {workplace1}), new CompetenceGroup(new [] {workplace2})},
            ScheduleFlexibility.Strict);

        Assert.Equal(new HashSet<Workplace> {workplace1, workplace2}, parameters.Workplaces);
    }
}
