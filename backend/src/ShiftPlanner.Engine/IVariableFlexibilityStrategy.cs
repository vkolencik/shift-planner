using ShiftPlanner.Domain;

namespace ShiftPlanner.Engine;

public interface IVariableFlexibilityStrategy<TResult>
{
    bool PerformWithFlexibilityValue(ScheduleFlexibility flexibility, out TResult result);
}
