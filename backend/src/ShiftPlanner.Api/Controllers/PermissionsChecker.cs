using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Controllers;

class PermissionsChecker : IPermissionsChecker
{
    private const int DefaultRequirementsDeadlineDate = 5;
    private const int RequirementsOpenDate = 25;
    private readonly IUserAccessor _userAccessor;
    private readonly IClock _clock;
    private readonly IDictionary<ScheduledMonth, int> RequirementsDeadlineDateExceptions;

    public PermissionsChecker(IUserAccessor userAccessor, IClock clock)
    {
        _userAccessor = userAccessor;
        _clock = clock;
        RequirementsDeadlineDateExceptions = new Dictionary<ScheduledMonth, int>
        {
            [new ScheduledMonth(2023, 11)] = 15
        };
    }

    public bool CanReadRequirementsOfUser(string userId)
    {
        return userId == _userAccessor.GetLoggedInUserId() || _userAccessor.HasClaim(Claims.ReadRequirementsOfOthers);
    }

    public bool CanModifyRequirement(Day day, string userId, string requirementType)
    {
        if (_userAccessor.HasClaim(Claims.ModifyRequirementsOfOthers))
        {
            return true;
        }

        if (userId != _userAccessor.GetLoggedInUserId())
        {
            return false;
        }

        return requirementType switch
        {
            "leave" => IsLeaveModifiableOnDay(day),
            _ => IsModifiableOnDay(day)
        };
    }

    private bool IsModifiableOnDay(Day editedDay)
    {
        var today = _clock.Today;

        var activeMonthAfterMonths = today.DayOfMonth < RequirementsOpenDate ? 1 : 2;
        var activeMonth = today.Date.AddMonths(activeMonthAfterMonths);

        if (activeMonth.Month == August)
        {
            return false;
        }

        if (editedDay.Year == activeMonth.Year && (
                editedDay.Month == activeMonth.Month
                || IsSummerMonth(editedDay.Month) && activeMonth.Month == July))
        {
            if (!RequirementsDeadlineDateExceptions.TryGetValue(new ScheduledMonth(activeMonth.Year, activeMonth.Month), out var deadlineDate))
            {
                deadlineDate = DefaultRequirementsDeadlineDate;
            }

            var isReadOnly = activeMonthAfterMonths == 1 && today.DayOfMonth > deadlineDate;

            return !isReadOnly;
        }

        return false;
    }

    private bool IsSummerMonth(int month) => month == July || month == August;

    private const int July = 7;
    private const int August = 8;

    private bool IsLeaveModifiableOnDay(Day day)
    {
        var today = _clock.Today;

        var endOfYear = new DateOnly(today.Year, 12, 31);

        return day.Date <= endOfYear;
    }

    public bool CanApproveRequirements() => _userAccessor.HasClaim(Claims.ApproveRequirementsOfOthers);
}
