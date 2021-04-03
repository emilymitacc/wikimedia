using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WikiMedia.Terminal
{
    public class ConsoleOutput : IStatsOutput
    {
        public Task WriteOutput(WikiMediaOutputContext outputContext)
        {
            Console.WriteLine($"hour\tdomain_code\tpage_title\tcount_views");
            foreach (var wikiMedia in outputContext.WikiMediaRows)
            {
                Console.WriteLine($"{wikiMedia.hour}\t{wikiMedia.domain_code}\t{wikiMedia.page_title}\t{wikiMedia.count_views}");
            }

            return Task.CompletedTask;
        }
    }
}
