using System.IO;
using AutoMapper;
using DataBaseLayer.Entityes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot.Examples.DotNetCoreWebHook.Services;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;
using TelegramBot.Utils;

namespace Telegram.Bot.Examples.DotNetCoreWebHook
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IUpdateService, UpdateService>();
            services.AddSingleton<IBotService, BotService>();
            services.Configure<BotConfiguration>(Configuration.GetSection("BotConfiguration"));
            
            var sp = services.BuildServiceProvider();

            // Resolve IBotService for init boot
            var fooService = sp.GetService<IBotService>();

            services
                .AddControllers()
                .AddNewtonsoftJson();
            services.AddLetsEncrypt();

            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);

            //services.AddAutoMapper(typeof(Startup));
            services.AddControllersWithViews();

            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}