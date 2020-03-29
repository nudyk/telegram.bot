namespace Telegram.Bot.Examples.DotNetCoreWebHook
{
    public class BotConfiguration
    {
        public string BotToken { get; set; }
        public string BotName { get; set; }
        public string CallbackUrl { get; set; }

        public string Socks5Host { get; set; }

        public string Socks5Port { get; set; }
    }
}
