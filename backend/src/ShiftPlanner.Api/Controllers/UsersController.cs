using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftPlanner.Services.Services;

namespace ShiftPlanner.Api.Controllers;

[Route("api/{tenant}/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserOutputModel>>> GetUsers()
    {
        var users = await _userService.GetAllUsers();
        var outputModel = users.Select(u => new UserOutputModel(u.UserId, u.UserName, u.ActiveUntil.ToString()));
        return Ok(outputModel);
    }
}
