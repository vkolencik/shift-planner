using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftPlanner.Api.Contracts;
using ShiftPlanner.Contracts;
using ShiftPlanner.Contracts.Services;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Controllers;

[Route("api/{tenant}/users/{userId}/requirements")]
[Authorize]
public class UsersRequirementsController : ControllerBase
{
    private readonly IRequirementsService _requirementsService;

    public UsersRequirementsController(IRequirementsService requirementsService)
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

        if (loggedInUserId != userId && !User.Claims.Any(c => c.Type == "permissions" && c.Value == Claims.ReadRequirementsOfOthers))
        {
            return StatusCode(StatusCodes.Status403Forbidden, $"User {loggedInUserId} cannot fetch requirements of user {userId}");
        }

        var requirements = await _requirementsService.GetRequirements(new Tenant(tenant), userId, new ScheduledMonth(year, month));
        var requirementsModel = requirements.ToDictionary(
            kvp => kvp.Key.Date.ToString("yyyy-MM-dd"),
            kvp => kvp.Value);

        return Ok(requirementsModel);
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
        var loggedInUserId = User.Identity.Name;

        if (loggedInUserId != userId && !User.HasClaim("permissions", Claims.ModifyRequirementsOfOthers))
        {
            return StatusCode(StatusCodes.Status403Forbidden, $"User {loggedInUserId} cannot fetch requirements of user {userId}");
        }

        await _requirementsService.SetRequirement(new RequirementModel(new Tenant(tenant), userId, new Day(year, month, day), requirement.RequirementType));

        return Ok();
    }

    [HttpDelete("{year}/{month}/{day}")]
    public async Task<ActionResult> Delete(
        [FromRoute] string tenant,
        [FromRoute] string userId,
        [FromRoute] int year,
        [FromRoute] int month,
        [FromRoute] int day)
    {
        var loggedInUserId = User.Identity.Name;

        if (loggedInUserId != userId
            && !(User.HasClaim("permissions", Claims.ModifyRequirementsOfOthers) || User.HasClaim("permissions", Claims.ApproveRequirementsOfOthers)))
        {
            return StatusCode(StatusCodes.Status403Forbidden, $"User {loggedInUserId} cannot fetch requirements of user {userId}");
        }

        DeleteResult result = await _requirementsService.DeleteRequirement(new Tenant(tenant), userId, new Day(year, month, day));

        return result switch
        {
            DeleteResult.Deleted => Ok(),
            DeleteResult.NotFound => NotFound()
        };
    }

    [HttpPut("{year}/{month}/{day}/approval")]
    [Authorize(Claims.ApproveRequirementsOfOthers)]
    public async Task<ActionResult> Approve(
        [FromRoute] string tenant,
        [FromRoute] string userId,
        [FromRoute] int year,
        [FromRoute] int month,
        [FromRoute] int day,
        [FromBody] ApprovalInputModel requirement)
    {
        if (!requirement.Approved)
        {
            return Ok();
        }

        var result = await _requirementsService.Approve(new Tenant(tenant), userId, new Day(year, month, day));

        return result switch
        {
            ApproveResult.Success => Ok(),
            ApproveResult.NotFound => NotFound(),
            ApproveResult.Failure => BadRequest(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public record ApprovalInputModel(bool Approved);

public record RequirementInputModel(string RequirementType);
