using afet_yonetim_net.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace afet_yonetim_net.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepremController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            // Bellekteki son depremleri dön
            return Ok(DepremStore.SonDepremler);
        }
    }
}
