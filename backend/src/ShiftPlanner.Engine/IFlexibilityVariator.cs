namespace ShiftPlanner.Engine;

public interface IFlexibilityVariator<TResult>
{
    VariableFlexibilityCalculationResult<TResult> ExecuteWithVariableFlexibility(IVariableFlexibilityStrategy<TResult> strategy);
}
