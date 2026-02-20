using Microsoft.AspNetCore.Mvc;

namespace TradeEngine.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Trade Engine Running");
        }
    }
}