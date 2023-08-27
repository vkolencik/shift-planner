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
    private readonly IScheduleValidator _scheduleValidator;
    private readonly IUserService _userService;
    private readonly ISchedulingParametersAggregator _paramsAggregator;

    public ScheduleController(
        IStoredScheduleService storedScheduleService,
        IScheduleValidator scheduleValidator,
        IUserService userService)
    {
        _storedScheduleService = storedScheduleService;
        _scheduleValidator = scheduleValidator;
        _userService = userService;
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

    [HttpPut("{year}/{month}/:validate")]
    public async Task<ActionResult<ScheduleValidationOutputModel>> ValidateSchedule(
        [FromRoute] string tenant,
        [FromRoute] int year,
        [FromRoute] int month,
        [FromBody] ScheduleInputModel scheduleInputModel)
    {
        var users = await _userService.GetAllUsers();
        var schedule = ConvertInputModel(scheduleInputModel, users);

        var validationErrors = await _scheduleValidator.ValidateSchedule(new Tenant(tenant), new ScheduledMonth(year, month), schedule);

        var errorOutputModels = validationErrors
            .Select(
                e => new ValidationErrorOutputModel(
                    e.Shifts.Select(s => new ShiftOutputModel(s.Day.Date.ToString("yyyy-MM-dd"), s.Workplace.Name)).ToArray(),
                    e.UserName == null ? null : new ScheduleUserOutputModel(users.SingleOrDefault(u => u.UserName == e.UserName)?.UserId ?? "", e.UserName),
                    e.ErrorType.ToString())).ToArray();
        var outputModel = new ScheduleValidationOutputModel(!validationErrors.Any(), errorOutputModels);

        return Ok(outputModel);
    }

    private static Schedule ConvertInputModel(ScheduleInputModel scheduleInputModel, IReadOnlyCollection<User> users)
    {
        return new Schedule(
            scheduleInputModel.Shifts
                .Select(
                    s => new ScheduledShift(
                        new Day(DateOnly.ParseExact(s.Day, "yyyy-MM-dd")),
                        new Workplace(s.Workplace),
                        users.Single(u => u.UserId == s.UserId)))
                .ToArray());
    }

    [HttpPut("{year}/{month}/:save")]
    [Authorize(Claims.ModifySchedule)]
    public async Task<ActionResult> SaveSchedule(
        [FromRoute] string tenant,
        [FromRoute] int year,
        [FromRoute] int month,
        [FromBody] ScheduleInputModel scheduleInputModel)
    {
        var users = await _userService.GetAllUsers();
        
        var schedule = ConvertInputModel(scheduleInputModel, users);

        await _storedScheduleService.StoreSchedule(new Tenant(tenant), new ScheduledMonth(year, month), schedule);
        
        return Ok();
    }

    private ShiftAssignmentOutputModel ConvertToOutput(ScheduledShift shift) =>
        new ShiftAssignmentOutputModel(shift.Day.Date.ToString("yyyy-MM-dd"), shift.Workplace.Name, new UserOutputModel(shift.User.UserId, shift.User.UserName, shift.User.ActiveUntil.ToString()));
}

public record ScheduleOutputModel(IReadOnlyList<ShiftAssignmentOutputModel> Shifts);

public record ShiftAssignmentOutputModel(string Day, string Workplace, UserOutputModel User);

public record ShiftOutputModel(string Day, string Workplace);

public record ScheduleInputModel(IReadOnlyCollection<ShiftAssignmentInputModel> Shifts);

public record ShiftAssignmentInputModel(string Day, string Workplace, string UserId);

public record ScheduleValidationOutputModel(bool IsValid, IReadOnlyCollection<ValidationErrorOutputModel> ValidationErrors);

public record ValidationErrorOutputModel(IReadOnlyCollection<ShiftOutputModel> Shifts, ScheduleUserOutputModel? User, string Error);
