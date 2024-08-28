using Microsoft.AspNetCore.Mvc;

namespace Dynamic_Health_Check_Demo.Controllers;

[ApiController]
[Route("[controller]")]
public class LogController(ILogger<LogController> logger) : ControllerBase
{
    [HttpGet("logCritical")]
    public IActionResult LogCritical()
    {
        logger.LogCritical("log critical");
        return Ok("log critical");
    }

    [HttpGet("logError")]
    public IActionResult LogError()
    {
        logger.LogError("log error");
        return Ok("log error");
    }
}