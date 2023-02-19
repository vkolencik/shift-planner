using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Controllers;

class Clock : IClock
{
    public Day Today => new Day(DateOnly.FromDateTime(DateTime.Now));
}
