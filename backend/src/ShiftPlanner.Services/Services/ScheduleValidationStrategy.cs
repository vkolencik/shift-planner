using ShiftPlanner.Domain;
using ShiftPlanner.Engine;
using ShiftPlanner.Rules;
using ShiftPlanner.Rules.Ruleset;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

internal class ScheduleValidationStrategy : IVariableFlexibilityStrategy<ScheduleValidationResult>
{
    private readonly Schedule _schedule;
    private readonly IRulesetFactory _rulesetFactory;
    private readonly IValidatingReceiverFactory _receiverFactory;
    private readonly IReadOnlyDictionary<Type, ScheduleErrorType> _ruleErrorMapping;

    public ScheduleValidationStrategy(
        Schedule schedule,
        MonthScheduleParameters parameters,
        IRulesetFactory rulesetFactory,
        IValidatingReceiverFactory receiverFactory)
    {
        _schedule = schedule;
        _rulesetFactory = rulesetFactory;
        _receiverFactory = receiverFactory;
        Parameters = parameters;

        _ruleErrorMapping = new Dictionary<Type, ScheduleErrorType>
        {
            [typeof(EveryDayAtEveryWorkplaceIsCoveredRule)] = ScheduleErrorType.EveryDayIsCovered,
            [typeof(NoConcurrentShiftsRule)] = ScheduleErrorType.NoConcurrentShifts,
            [typeof(NoConsecutiveShiftsRule)] = ScheduleErrorType.NoConsecutiveShifts,
            [typeof(NoAlternativeDayShiftsRule)] = ScheduleErrorType.NoAlternativeDayShifts,
            [typeof(AlwaysScheduleAlternatesOverWeekendsRule)] = ScheduleErrorType.AlwaysAlternatesOverWeekends,
            [typeof(WeekendShiftLimitRule)] = ScheduleErrorType.WeekendLimit,
            [typeof(MinNumberOfWeekendShiftsRule)] = ScheduleErrorType.MinNumberOfWeekends,
            [typeof(NoSubsequentWeekendsRule)] = ScheduleErrorType.NoSubsequentWeekends,
            [typeof(LeaveRule)] = ScheduleErrorType.Leaves,
            [typeof(WorkplaceCapabilityRule)] = ScheduleErrorType.WorkplaceCapability,
            [typeof(CompetenceGroupQualificationRule)] = ScheduleErrorType.CompetenceGroupQualification,
            [typeof(PredefinedShiftsRule)] = ScheduleErrorType.PredefinedShifts,
            [typeof(MonthlyShiftCountRule)] = ScheduleErrorType.MonthlyShiftCount,
            [typeof(WorkplacePreferencePercentagesRule)] = ScheduleErrorType.WorkplacePreferencePercentage,
            [typeof(EvenSpreadWithinMonthRule)] = ScheduleErrorType.EvenSpreadWithinMonth
        };
    }

    public bool PerformWithFlexibilityValue(ScheduleFlexibility flexibility, out ScheduleValidationResult result)
    {
        var rules = _rulesetFactory.CreateRuleset(Parameters, flexibility, new SchedulingParametersRecorder());
        var errors = new List<ScheduleValidationError>();

        foreach (var rule in rules)
        {
            var errorType = _ruleErrorMapping[rule.GetType()];

            var receiver = _receiverFactory.GetReceiver(_schedule, errorType);

            rule.Apply(receiver);
            errors.AddRange(receiver.GetErrors());
        }

        result = new ScheduleValidationResult(errors);
        return !errors.Any();
    }

    public MonthScheduleParameters Parameters { get; }
}

public interface IScheduleValidatingConstraintReceiver : IScheduleConstraintReceiver
{
    IReadOnlyCollection<ScheduleValidationError> GetErrors();
}

public interface IValidatingReceiverFactory
{
    IScheduleValidatingConstraintReceiver GetReceiver(Schedule schedule, ScheduleErrorType scheduleErrorType);
}
