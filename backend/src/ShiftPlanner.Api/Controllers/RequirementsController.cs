using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Controllers;

[Route("api/{tenant}/users/{userId}/requirements")]
[Authorize]
public class RequirementsController : ControllerBase
{
    private readonly IRequirementsService _requirementsService;

    public RequirementsController(IRequirementsService requirementsService)
    {
        _requirementsService = requirementsService;
    }

    // GET api/values
    [HttpGet("{year}/{month}")]
    public async Task<ActionResult<IDictionary<string, string>>> Get(
        [FromRoute] string tenant,
        [FromRoute] string userId,
        [FromRoute] int year,
        [FromRoute] int month)
    {
        var loggedInUserId = User.Identity.Name;
        
        if (loggedInUserId != userId)
        {
            return Unauthorized($"User {loggedInUserId} cannot fetch requirements of user {userId}");
        }

        var requirements = await _requirementsService.GetRequirements(new Tenant(tenant), userId, new ScheduledMonth(year, month));
        return Ok(requirements);
    }

    [HttpPut("{year}/{month}/{day}")]
    public async Task<ActionResult> Put(
        [FromRoute] string tenant,
        [FromRoute] string userId,
        [FromRoute] int year,
        [FromRoute] int month,
        [FromRoute] int day,
        [FromBody] RequirementInputModel requirement)
    {
        await _requirementsService.SetRequirement(new Tenant(tenant), userId, new Day(year, month, day), requirement.RequirementType);

        return Ok();
    }
}

public record RequirementInputModel(string RequirementType);
