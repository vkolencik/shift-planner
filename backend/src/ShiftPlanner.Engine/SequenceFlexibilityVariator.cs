using ShiftPlanner.Domain;

namespace ShiftPlanner.Engine;

public class SequenceFlexibilityVariator<TResult> : IFlexibilityVariator<TResult>
{
    private readonly ScheduleFlexibility _maxFlexibility;

    public SequenceFlexibilityVariator(ScheduleFlexibility maxFlexibility)
    {
        _maxFlexibility = maxFlexibility;
    }

    public VariableFlexibilityCalculationResult<TResult> ExecuteWithVariableFlexibility(IVariableFlexibilityStrategy<TResult> strategy)
    {
        TResult? result = default(TResult);

        for (var flexibilityDegree = 0u; flexibilityDegree <= _maxFlexibility.Degree; flexibilityDegree++)
        {
            var flexibility = new ScheduleFlexibility(flexibilityDegree);
            var isSuccess = strategy.PerformWithFlexibilityValue(flexibility, out result);
            if (isSuccess)
            {
                return new VariableFlexibilityCalculationResult<TResult>(result, isSuccess, flexibility);
            }
        }

        if (result is null)
        {
            throw new InvalidOperationException("Result is null");
        }

        return new VariableFlexibilityCalculationResult<TResult>(result, false, _maxFlexibility);
    }
}
