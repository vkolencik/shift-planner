using ShiftPlanner.Domain;
using Xunit;

namespace ShiftPlanner.Engine.Tests;

public class SequenceFlexibilityVariatorShould
{
    [Fact]
    public void ReturnFirstResult_WhenItsSuccessful()
    {
        var strategy = new TestStrategy(ScheduleFlexibility.Strict, 13, 7);
        var variator = new SequenceFlexibilityVariator<int>(new ScheduleFlexibility(20));

        var result = variator.ExecuteWithVariableFlexibility(strategy);

        Assert.True(result.IsSuccess);
        Assert.Equal(13, result.Result);
        Assert.Equal(ScheduleFlexibility.Strict, result.AttainedFlexibility);
    }

    [Fact]
    public void IncreaseFlexibilityUntilSuccessIsAttained()
    {
        var successfulFlexibilityThreshold = new ScheduleFlexibility(5);
        var strategy = new TestStrategy(successfulFlexibilityThreshold, 17, 7);
        var variator = new SequenceFlexibilityVariator<int>(new ScheduleFlexibility(20));

        var result = variator.ExecuteWithVariableFlexibility(strategy);

        Assert.True(result.IsSuccess);
        Assert.Equal(17, result.Result);
        Assert.Equal(successfulFlexibilityThreshold, result.AttainedFlexibility);
    }

    [Fact]
    public void ReturnLastResult_WhenSuccessIsntAttained()
    {
        var strategy = new TestStrategy(new ScheduleFlexibility(500), 17, 7);
        var maxFlexibility = new ScheduleFlexibility(20u);
        var variator = new SequenceFlexibilityVariator<int>(maxFlexibility);

        var result = variator.ExecuteWithVariableFlexibility(strategy);

        Assert.False(result.IsSuccess);
        Assert.Equal(7, result.Result);
        Assert.Equal(maxFlexibility, result.AttainedFlexibility);
    }

    private class TestStrategy : IVariableFlexibilityStrategy<int>
    {
        private ScheduleFlexibility _successfulFlexibilityThreshold;
        private int _successResult;
        private int _failureResult;

        public TestStrategy(ScheduleFlexibility successfulFlexibilityThreshold, int successResult, int failureResult)
        {
            _successfulFlexibilityThreshold = successfulFlexibilityThreshold;
            _successResult = successResult;
            _failureResult = failureResult;
        }

        public bool PerformWithFlexibilityValue(ScheduleFlexibility flexibility, out int result)
        {
            if (flexibility == _successfulFlexibilityThreshold)
            {
                result = _successResult;
                return true;
            }

            result = _failureResult;
            return false;
        }
    }
}
