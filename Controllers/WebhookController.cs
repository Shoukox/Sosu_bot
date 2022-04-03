using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sosu.Services;
using Telegram.Bot.Types;

namespace Sosu.Controllers
{
    public class WebhookController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromServices] HandleUpdateService handleUpdateService,
                                              [FromBody] Update update)
        {
            await handleUpdateService.EchoAsync(update);
            return Ok();
        }
    }
}
