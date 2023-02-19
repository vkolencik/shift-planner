using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftPlanner.Domain;
using ShiftPlanner.Services.Contracts;
using ShiftPlanner.Services.Services;

namespace ShiftPlanner.Api.Controllers;

[Route("api/{tenant}/users/{userId}/requirements")]
[Authorize]
public class UsersRequirementsController : ControllerBase
{
    private readonly IRequirementsService _requirementsService;
    private readonly IPermissionsChecker _permissionsChecker;

    public UsersRequirementsController(IRequirementsService requirementsService, IPermissionsChecker permissionsChecker)
    {
        _requirementsService = requirementsService;
        _permissionsChecker = permissionsChecker;
    }

    // GET api/values
    [HttpGet("{year}/{month}")]
    public async Task<ActionResult<IDictionary<string, string>>> Get(
        [FromRoute] string tenant,
        [FromRoute] string userId,
        [FromRoute] int year,
        [FromRoute] int month)
    {
        if (!_permissionsChecker.CanReadRequirementsOfUser(userId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, $"User is not permitted to fetch requirements of user {userId}");
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
        [FromBody] RequirementInputModel requirementModel)
    {
        var date = new Day(year, month, day);
        
        if (!_permissionsChecker.CanModifyRequirement(date, userId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, $"User cannot modify requirements of user {userId}");
        }

        var requirement = new Requirement(new Tenant(tenant), userId, date, requirementModel.RequirementType);
        await _requirementsService.SetRequirement(requirement);

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
        var date = new Day(year, month, day);

        if (!(_permissionsChecker.CanModifyRequirement(date, userId) || _permissionsChecker.CanApproveRequirements()))
        {
            return StatusCode(StatusCodes.Status403Forbidden, $"User cannot fetch requirements of user {userId}");
        }

        DeleteResult result = await _requirementsService.DeleteRequirement(new Tenant(tenant), userId, date);

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
