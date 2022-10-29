using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShiftPlanner.Api.Controllers;

[Route("api/{tenant}/users/{userId}/requirements")]
[Authorize]
public class RequirementsController : ControllerBase
{
    // GET api/values
    [HttpGet("{year}/{month}")]
    public ActionResult<IDictionary<string, string>> Get([FromRoute] string tenant, [FromRoute] string userId)
    {
        var loggedInUserId = User.Identity.Name;
        
        if (loggedInUserId != userId)
        {
            return Unauthorized($"User {loggedInUserId} cannot fetch requirements of user {userId}");
        }
       
        return Ok(new Dictionary<string, string>
        {
            ["2022-12-06"] = "soft-yes",
            ["2022-12-08"] = "soft-yes",
            ["2022-12-21"] = "leave",
            ["2022-12-13"] = "hard-no"
        });
    }

    [HttpPut("{year}/{month}/{day}")]
    public void Put([FromBody] string requirementType)
    {

    }
}
