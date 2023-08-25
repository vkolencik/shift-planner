namespace ShiftPlanner.Services.Contracts;

public record User(string UserId, string UserName, string Email, DateOnly ActiveUntil)
{
    public static readonly DateOnly MaxDate = new DateOnly(2999, 1, 1);
}
