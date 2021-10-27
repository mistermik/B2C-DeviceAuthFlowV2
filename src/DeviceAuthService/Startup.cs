using Ltwlf.Azure.B2C;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;



[assembly: WebJobsStartup(typeof(Startup))]

namespace Ltwlf.Azure.B2C
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = builder.GetContext().Configuration;


            builder.Services.AddOptions<ConfigOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("Config").Bind(settings);
                });

            builder.Services.AddSingleton<PageFactory>();

            builder.Services.AddHttpClient();

            builder.Services.Configure<HttpOptions>(options => options.RoutePrefix = string.Empty);
            var test = (config.GetValue<string>("Config:Redis"));
            var redis = ConnectionMultiplexer.Connect(config.GetValue<string>("Config:Redis"));
            
            
            //var redis  = ConnectionMultiplexer.Connect("127.0.0.1");

            builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
        }
    }
}