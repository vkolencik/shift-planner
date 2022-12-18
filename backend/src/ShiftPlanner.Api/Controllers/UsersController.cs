using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftPlanner.Contracts.Services;

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
    [Authorize(Policy = Claims.ReadRequirementsOfOthers)]
    public async Task<ActionResult<IEnumerable<UserOutputModel>>> GetUsers()
    {
        var users = await _userService.GetAllUsers();
        var outputModel = users.Select(u => new UserOutputModel(u.UserId, u.UserName));
        return Ok(outputModel);
    }
}

public record UserOutputModel(string UserId, string UserName);
