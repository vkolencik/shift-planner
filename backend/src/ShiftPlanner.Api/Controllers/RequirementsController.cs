using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftPlanner.Domain;
using ShiftPlanner.Services.Services;

namespace ShiftPlanner.Api.Controllers;

[Route("api/{tenant}/requirements")]
public class RequirementsController : ControllerBase
{
    private readonly IRequirementsService _requirementsService;

    public RequirementsController(IRequirementsService requirementsService)
    {
        _requirementsService = requirementsService;
    }

    [HttpGet("{year}/{month}")]
    public async Task<ActionResult<IReadOnlyCollection<UserRequirementOutputModel>>> Get(
        [FromRoute] string tenant,
        [FromRoute] int year,
        [FromRoute] int month)
    {
        var requirements = await _requirementsService.GetRequirementsOverviewForMonth(new Tenant(tenant), new ScheduledMonth(year, month));
        var outputModel = requirements
            .Select(r => new UserRequirementOutputModel(r.UserId, r.UserName, r.Date.Date.ToString("yyyy-MM-dd"), r.RequirementType, r.Approved))
            .ToArray();
        return Ok(outputModel);
    }
}
