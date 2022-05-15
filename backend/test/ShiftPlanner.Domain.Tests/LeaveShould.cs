using System;
using Xunit;

namespace ShiftPlanner.Domain.Tests;

public class LeaveShould
{
    [Fact]
    public void HaveProperLength()
    {
        var leave = new Leave(new DateOnly(2022, 2, 1), new DateOnly(2022, 2, 1));
        
        Assert.Equal(1, leave.LengthDays);
    }
}
