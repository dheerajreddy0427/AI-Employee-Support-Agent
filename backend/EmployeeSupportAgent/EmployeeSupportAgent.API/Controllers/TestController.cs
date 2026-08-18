using Microsoft.AspNetCore.Mvc;

namespace EmployeeSupportAgent.API.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("API Working");
    }
}