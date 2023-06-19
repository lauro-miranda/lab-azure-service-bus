using Microsoft.AspNetCore.Mvc;

namespace LAB.AzureServiceBus.Controllers.Default
{
    [ApiController, Route("")]
    public class MeController : ControllerBase
    {
        [HttpGet, Route("")]
        public IActionResult Get()
        {
            return Ok(new { name = "LAB.AzureServiceBus", version = "0.1" });
        }
    }
}