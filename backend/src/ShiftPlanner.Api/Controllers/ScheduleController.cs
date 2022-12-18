using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftPlanner.Contracts;
using ShiftPlanner.Contracts.Services;
using ShiftPlanner.Domain;
using ShiftPlanner.Services;

namespace ShiftPlanner.Api.Controllers;

[Route("/api/{tenant}/schedule")]
[Authorize(Policy = Claims.ReadRequirementsOfOthers)]
public class ScheduleController : ControllerBase
{
    private IStoredScheduleService _storedScheduleService;

    public ScheduleController(IStoredScheduleService storedScheduleService)
    {
        _storedScheduleService = storedScheduleService;
    }

    [HttpGet("{year}/{month}")]
    public async Task<ActionResult<Schedule>> GetSchedule(int year, int month)
    {
        var storedSchedule = await _storedScheduleService.GetSchedule(new ScheduledMonth(year, month));
        return Ok(storedSchedule);
    }
}
