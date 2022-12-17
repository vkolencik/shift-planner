using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftPlanner.Contracts;

namespace ShiftPlanner.Api.Controllers;

[Route("/api/{tenant}/schedule")]
[Authorize(Policy = Claims.ReadRequirementsOfOthers)]
public class ScheduleController : ControllerBase
{
    [HttpGet("{year}/{month}")]
    public async Task<ActionResult<StoredSchedule>> GetSchedule(int year, int month)
    {
        return Ok(new StoredSchedule(Array.Empty<ScheduledShift>()));
    }
}
