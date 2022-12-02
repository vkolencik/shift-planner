using Microsoft.AspNetCore.Mvc;

namespace ShiftPlanner.Api.Controllers;

[Route("/")]
public class LiveController : ControllerBase
{
    [HttpGet]
    public ActionResult Live()
    {
        return Ok("alive");
    }
}
