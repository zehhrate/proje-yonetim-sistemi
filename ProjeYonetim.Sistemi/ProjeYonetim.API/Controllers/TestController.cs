using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProjeYonetim.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        // Bu endpoint'e herkes erişebilir.
        [HttpGet("public")]
        public IActionResult GetPublicData()
        {
            return Ok("Bu herkese açık bir bilgidir.");
        }

        // Bu endpoint'e SADECE giriş yapmış olanlar erişebilir.
        [HttpGet("private")]
        [Authorize]
        public IActionResult GetPrivateData()
        {
            return Ok("Bu GİZLİ bir bilgidir, sadece giriş yapanlar görür!");
        }
    }
}