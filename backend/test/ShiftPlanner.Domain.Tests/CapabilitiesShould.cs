using System;
using System.Collections.Generic;
using Xunit;

namespace ShiftPlanner.Domain.Tests;

public class CapabilitiesShould
{
    [Fact]
    public void ReturnAllowedWorkplaces()
    {
        Workplace w1 = new Workplace("w1");
        Workplace w2 = new Workplace("w2");
        Workplace w3 = new Workplace("w3");
        Capabilities capabilities = new Capabilities(new WorkplaceCapability[]
        {
            new WorkplaceCapability.Green(w1, 100),
            new WorkplaceCapability.Yellow(w2), 
            new WorkplaceCapability.Red(w3), 
        });

        Assert.Equal(new HashSet<Workplace> {w1, w2}, capabilities.AllowedWorkplaces);
    }

    [Theory]
    [InlineData(40)]
    [InlineData(60)]
    public void Throw_GivenInvalidPercentageSum(uint percentage)
    {
        Workplace w1 = new Workplace("w1");
        Workplace w2 = new Workplace("w2");

        Assert.Throws<ArgumentException>(
            () => new Capabilities(
                new WorkplaceCapability[]
                {
                    new WorkplaceCapability.Green(w1, 50),
                    new WorkplaceCapability.Green(w2, percentage)
                }));
    }
}
