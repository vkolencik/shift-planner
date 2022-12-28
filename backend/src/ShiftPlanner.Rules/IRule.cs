namespace ShiftPlanner.Rules;

public interface IRule
{
    void Apply(IScheduleConstraintReceiver target);
}
