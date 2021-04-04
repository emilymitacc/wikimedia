using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WikiMedia.Core;
using WikiMedia.Core.Interfaces;
using WikiMedia.Infrastructure;

namespace WikiMedia.Terminal
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var builder = new HostBuilder();
            builder.ConfigureLogging((context, b) =>
            {
#if DEBUG
                b.AddConsole();
#endif
                b.AddDebug();
            });
            builder.ConfigureServices(services =>
            {                                
                services.AddHttpClient("wikimedia")                
                  .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(Math.Min(30 * retryAttempt, 120))));

                services.AddScoped<IWikimediaDataReader, GzipWikimediaDataReader>();
                services.AddScoped<IStatsOutput, ConsoleOutput>();
                services.AddScoped<IDateTimeService, SystemTimeService>();
                services.AddScoped<WikiMediaPageViewsProcessor>();
            });

            var host = builder.Build();
            using (host)
            {
                var processor = host.Services.GetService(typeof(WikiMediaPageViewsProcessor)) as WikiMediaPageViewsProcessor;
                var options = new WikiMediaRunOptions() { LastHours = 5 };
                await processor.Process(options);
            }
        }
    }
}
