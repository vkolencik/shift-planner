using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShiftPlanner.Api.Controllers;

[Route("api/{tenant}/requirements")]
[Authorize("read:requirements_of_others")]
public class RequirementsController
{
    [HttpGet("{year}/{month}")]
    public async Task<ActionResult<IDictionary<string, string>>> Get(
        [FromRoute] string tenant,
        [FromRoute] int year,
        [FromRoute] int month)
    {
        throw new NotImplementedException();
    }
}
