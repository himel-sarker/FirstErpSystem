using Microsoft.AspNetCore.Mvc;

namespace FirstErpSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    //Added By Himel Sarkar 08-06-2025
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status  = "Healthy",
            system  = "First ERP System",
            version = "1.0.0",
            time    = DateTime.UtcNow
        });
    }
    //End By Himel Sarkar 08-06-2025
}
