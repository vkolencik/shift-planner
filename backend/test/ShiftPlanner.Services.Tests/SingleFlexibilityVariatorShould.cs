using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Engine;

namespace ShiftPlanner.Services.Tests;

public class SingleFlexibilityVariatorShould
{
    [Fact]
    public void ExecuteWithSingleFlexibility()
    {
        var strategyMock = new Mock<IVariableFlexibilityStrategy<int>>();
        var flexibilityValues = new List<ScheduleFlexibility>();
        int result = 3;
        strategyMock.Setup(_ => _.PerformWithFlexibilityValue(Capture.In(flexibilityValues), out result)).Returns(true);
        
        var variator = new SingleFlexibilityVariator(new ScheduleFlexibility(23));

        var strategyResult = variator.ExecuteWithVariableFlexibility(strategyMock.Object);

        Assert.True(strategyResult.IsSuccess);
        Assert.Equal(23u, strategyResult.AttainedFlexibility.Degree);
        var flexibility = Assert.Single(flexibilityValues);
        Assert.Equal(23u, flexibility.Degree);
    }
}
