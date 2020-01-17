using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Telegram.Bot.Examples.DotNetCoreWebHook
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .UseUrls("http://845108271884.sn.mynetname.net:5000;https://845108271884.sn.mynetname.net:5002")
                        ;
                });
    }
}
