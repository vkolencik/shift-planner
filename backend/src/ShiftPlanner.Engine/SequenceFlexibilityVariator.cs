using ShiftPlanner.Domain;

namespace ShiftPlanner.Engine;

public class SequenceFlexibilityVariator : IFlexibilityVariator
{
    private const uint InitialFlexibilityDegree = 0u;
    private readonly ScheduleFlexibility _maxFlexibility;

    public SequenceFlexibilityVariator(ScheduleFlexibility maxFlexibility)
    {
        _maxFlexibility = maxFlexibility;
    }

    public VariableFlexibilityCalculationResult<TResult> ExecuteWithVariableFlexibility<TResult>(IVariableFlexibilityStrategy<TResult> strategy)
    {
        if (_maxFlexibility.Degree < InitialFlexibilityDegree)
        {
            throw new ArgumentException($"Max flexibility ({_maxFlexibility.Degree}) is lower than initial flexibility ({InitialFlexibilityDegree})");
        }
        
        TResult? result = default(TResult);

        for (var flexibilityDegree = InitialFlexibilityDegree; flexibilityDegree <= _maxFlexibility.Degree; flexibilityDegree++)
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
