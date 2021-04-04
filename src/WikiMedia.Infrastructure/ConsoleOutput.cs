using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WikiMedia.Terminal
{
    public class ConsoleOutput : IStatsOutput
    {
        private readonly ILogger<ConsoleOutput> logger;

        public ConsoleOutput(ILogger<ConsoleOutput> logger)
        {
            this.logger = logger;
        }

        public Task WriteOutput(WikiMediaOutputContext outputContext)
        {
            return Task.Run(() =>
            {
                logger.LogInformation("Started: WriteOutput");
                Console.WriteLine($"hour\tdomain_code\tpage_title\tcount_views");
                foreach (var wikiMedia in outputContext.WikiMediaRows)
                {
                    Console.WriteLine($"{wikiMedia.hour}\t{wikiMedia.domain_code}\t{wikiMedia.page_title}\t{wikiMedia.count_views}");
                }
                logger.LogInformation("Finished: WriteOutput");
            });
        }
    }
}
