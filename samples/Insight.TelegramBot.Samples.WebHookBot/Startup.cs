using System.Net.Http;
using Insight.TelegramBot.Configurations;
using Insight.TelegramBot.Samples.Domain;
using Insight.TelegramBot.Web;
using Insight.TelegramBot.Web.Hosts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using Telegram.Bot;

namespace Insight.TelegramBot.Samples.WebHookBot
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public BotConfiguration BotConfiguration { get; } = new BotConfiguration();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            Configuration.GetSection(nameof(BotConfiguration)).Bind(BotConfiguration);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddNewtonsoftJson(
                    opt => opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver())
                .AddUpdateController();

            services.Configure<BotConfiguration>(Configuration.GetSection(nameof(BotConfiguration)));

            services.AddHttpClient();
            
            services.AddScoped<IBot, Bot>();
            services.AddScoped<IUpdateProcessor, UpdateProcessor>();
            
            services.AddTransient<ITelegramBotClient, TelegramBotClient>(c =>
                new TelegramBotClient(c.GetService<IOptions<BotConfiguration>>().Value.Token,
                    c.GetService<IHttpClientFactory>().CreateClient()));

            services.AddHostedService(c =>
                new TelegramBotWebHookHost(c.GetService<ITelegramBotClient>(),
                    c.GetService<IOptions<BotConfiguration>>().Value));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.AddUpdateControllerRoute(BotConfiguration.WebHookConfiguration.WebHookPath);
            });
        }
    }
}