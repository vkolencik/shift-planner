namespace ShiftScheduler.Rules;

public interface IRule
{
    void Apply(IScheduleConstraintReceiver target);
}
