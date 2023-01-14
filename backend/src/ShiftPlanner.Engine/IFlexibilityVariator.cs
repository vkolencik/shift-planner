namespace ShiftPlanner.Engine;

public interface IFlexibilityVariator
{
    VariableFlexibilityCalculationResult<TResult> ExecuteWithVariableFlexibility<TResult>(IVariableFlexibilityStrategy<TResult> strategy);
}
