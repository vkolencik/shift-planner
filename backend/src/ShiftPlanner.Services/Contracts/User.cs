using ShiftPlanner.Domain;

namespace ShiftPlanner.Services.Contracts;

public record User(string UserId, string UserName, string Email, ScheduledMonth ActiveUntil)
{
    public static readonly ScheduledMonth MaxDate = new ScheduledMonth(2999, 1);
}
