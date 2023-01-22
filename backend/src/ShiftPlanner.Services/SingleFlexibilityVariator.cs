using ShiftPlanner.Domain;
using ShiftPlanner.Engine;

namespace ShiftPlanner.Services;

public class SingleFlexibilityVariator : IValidationFlexibilityVariator
{
    private readonly ScheduleFlexibility _scheduleFlexibility;

    public SingleFlexibilityVariator(ScheduleFlexibility scheduleFlexibility)
    {
        _scheduleFlexibility = scheduleFlexibility;
    }

    public VariableFlexibilityCalculationResult<TResult> ExecuteWithVariableFlexibility<TResult>(IVariableFlexibilityStrategy<TResult> strategy)
    {
        var isSsuccess = strategy.PerformWithFlexibilityValue(_scheduleFlexibility, out var result);
        return new VariableFlexibilityCalculationResult<TResult>(result, isSsuccess, _scheduleFlexibility);
    }
}
