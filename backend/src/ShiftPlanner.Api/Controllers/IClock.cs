using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Controllers;

public interface IClock
{
    Day Today { get; } 
}
