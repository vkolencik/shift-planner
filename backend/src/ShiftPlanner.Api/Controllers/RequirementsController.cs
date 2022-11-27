using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftPlanner.Api.Contracts;

namespace ShiftPlanner.Api.Contracts
{
}

namespace ShiftPlanner.Api.Controllers
{
    [Route("api/{tenant}/requirements")]
    [Authorize("read:requirements_of_others")]
    public class RequirementsController : ControllerBase
    {
        private readonly IRequirementsService _requirementsService;

        public RequirementsController(IRequirementsService requirementsService)
        {
            _requirementsService = requirementsService;
        }

        [HttpGet("{year}/{month}")]
        public async Task<ActionResult<IReadOnlyCollection<UserRequirementModel>>> Get(
            [FromRoute] string tenant,
            [FromRoute] int year,
            [FromRoute] int month)
        {
            var requirements = await _requirementsService.GetRequirementsOverviewForMonth();
            return Ok(requirements);
        }
    }
}
