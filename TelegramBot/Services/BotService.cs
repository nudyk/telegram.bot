using Microsoft.Extensions.Options;
using MihaZupan;
using Telegram.Bot.Types;

namespace Telegram.Bot.Examples.DotNetCoreWebHook.Services
{
    public class BotService : IBotService
    {
        private readonly BotConfiguration _config;

        public BotService(IOptions<BotConfiguration> config)
        {
            _config = config.Value;
            Client = new TelegramBotClient(_config.BotToken);
            WebhookInfo webhoks = Client.GetWebhookInfoAsync().Result;
            if (webhoks != null)
            {
                Client.DeleteWebhookAsync().Wait();
            }

            if (!string.IsNullOrEmpty(_config.CallbackUrl))
            {
                var hook = _config.CallbackUrl;
                Client.SetWebhookAsync(hook).Wait();
            }
        }

        public TelegramBotClient Client { get; }
    }
}
