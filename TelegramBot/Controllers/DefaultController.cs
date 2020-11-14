using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Examples.DotNetCoreWebHook.Services;
using TelegramBot.Utils;

namespace TelegramBot.Controllers
{
    //[Route("/")]
    [ApiController]
    public class DefaultController : ControllerBase
    {
        //
        //    private IBotService _botService;
        private readonly ILogger<DefaultController> _logger;

        public DefaultController(ILogger<DefaultController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        [Route("/TP/public/index.php")]
        public async Task<ActionResult> IndexPhp()
        {
            var remoteIpAddress = Request?.HttpContext?.Connection?.RemoteIpAddress;
            _logger.LogInformation("Remote Ip Address {0}, {1}", remoteIpAddress, remoteIpAddress?.MapToIPv4());
            var infiniteStream = new InfiniteStream();
            return File(infiniteStream, "text/html");
        }

        //    [HttpPost]
        //    public async Task<OkResult> Post()
        //    {
        //        return Ok();
        //    }
    }
}