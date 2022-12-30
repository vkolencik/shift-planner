using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftPlanner.Domain;
using ShiftPlanner.Services.Contracts;
using ShiftPlanner.Services.Services;

namespace ShiftPlanner.Api.Controllers;

[Route("/api/{tenant}/schedule")]
[Authorize]
public class ScheduleController : ControllerBase
{
    private IStoredScheduleService _storedScheduleService;

    public ScheduleController(IStoredScheduleService storedScheduleService)
    {
        _storedScheduleService = storedScheduleService;
    }

    [HttpGet("{year}/{month}")]
    public async Task<ActionResult<ScheduleOutputModel>> GetSchedule(int year, int month)
    {
        var storedSchedule = await _storedScheduleService.GetSchedule(new ScheduledMonth(year, month));

        if (storedSchedule is null)
        {
            return NoContent();
        }
        
        var outputModel = new ScheduleOutputModel(storedSchedule.Shifts.Select(ConvertToOutput).ToArray());
        return Ok(outputModel);
    }

    private ShiftOutputModel ConvertToOutput(ScheduledShift shift) =>
        new ShiftOutputModel(shift.Day.Date.ToString("yyyy-MM-dd"), shift.Workplace.Name, new UserOutputModel(shift.User.UserId, shift.User.UserName));

    public record ScheduleOutputModel(IReadOnlyList<ShiftOutputModel> Shifts);

    public record ShiftOutputModel(string Day, string Workplace, UserOutputModel User);
}

