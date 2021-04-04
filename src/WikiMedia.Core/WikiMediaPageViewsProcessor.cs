using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiMedia.Terminal
{
    public class WikiMediaPageViewsProcessor
    {
        private readonly IWikimediaDataReader wikimediaDataReader;
        private readonly IStatsOutput statsOutput;
        private readonly IDateTimeService dateTimeService;
        private readonly ILogger<WikiMediaPageViewsProcessor> logger;

        public WikiMediaPageViewsProcessor(IWikimediaDataReader wikimediaDataReader, IStatsOutput statsOutput, IDateTimeService dateTimeService
            , ILogger<WikiMediaPageViewsProcessor> logger)
        {
            this.wikimediaDataReader = wikimediaDataReader;
            this.statsOutput = statsOutput;
            this.dateTimeService = dateTimeService;
            this.logger = logger;
        }

        public async Task Process(WikiMediaRunOptions options)
        {
            var overallStopWatch = new Stopwatch();
            overallStopWatch.Start();
            var listDateTime = GetListDateWithHour(dateTimeService.UtcNow, options.LastHours);
            
            logger.LogInformation($"Starting with {options.LastHours} task(s)");

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var dataTasks = listDateTime.Select((dt) => Task.Run(async () =>
            {
                logger.LogInformation($"Starting to download: {dt}");
                var rawData = await wikimediaDataReader.GetDataByHourAsync(dt);
                logger.LogInformation($"Starting to summarize: {dt}");
                return rawData;

            })).ToArray();            

            await Task.WhenAll(dataTasks);
            stopWatch.Stop();
            logger.LogInformation($"Download and grouping completed after {stopWatch.Elapsed.TotalSeconds:0.00}s");

            var chunks = new List<WikiMediaOutputContext>();
            stopWatch.Restart();
            logger.LogInformation($"Starting the summarize process:");
            for (int i = 0; i < dataTasks.Length; i++)
            {
                logger.LogInformation($"Starting to summarize: {i+1}/{dataTasks.Length}");
                var result = SummarizeViewsPerDomainCodeAndPageTitle(dataTasks[i].Result);
                logger.LogInformation($"Summarizing process completed: {i+1}/{dataTasks.Length}");

                var wikiMediaOutputContext = new WikiMediaOutputContext(result);
                chunks.Add(wikiMediaOutputContext);
            }            
            stopWatch.Stop();
            logger.LogInformation($"Summarizing process completed after {stopWatch.Elapsed.TotalSeconds:0.00}s");

            foreach (var chunk in chunks)
            {
                await statsOutput.WriteOutput(chunk);
            }

            logger.LogInformation($"All the process completed after {overallStopWatch.Elapsed.TotalSeconds:0.00}s");
        }

        public List<DateTime> GetListDateWithHour(DateTime dateTime, int lastHours)
        {
            var lista = new List<DateTime>();

            for (int i = 1; i <= lastHours; i++)
            {
                lista.Add(dateTime.AddHours(-i));
            }

            return lista;
        }

        public IEnumerable<WikiMediaRow> SummarizeViewsPerDomainCodeAndPageTitle(IEnumerable<WikiMediaRow> wikiMediaRows)
        {
            var consolidatedRows = from w in wikiMediaRows
                                   group w by new { w.hour, w.domain_code, w.page_title }
                                   into q
                                   select new WikiMediaRow
                                   {
                                       hour = q.Key.hour,
                                       domain_code = q.Key.domain_code,
                                       page_title = q.Key.page_title,
                                       count_views = q.Sum(x => x.count_views)
                                   };

            var maxViewsPerDomainCode = from w in consolidatedRows
                                        group w by new { w.hour, w.domain_code }
                                        into q
                                        select new WikiMediaRow
                                        {
                                            hour = q.Key.hour,
                                            domain_code = q.Key.domain_code,
                                            count_views = q.Max(x => x.count_views)
                                        };

            var result = from cr in consolidatedRows
                         join mv in maxViewsPerDomainCode
                         on new { cr.hour, cr.domain_code, cr.count_views }
                         equals new { mv.hour, mv.domain_code, mv.count_views }
                         select new WikiMediaRow
                         {
                             hour = cr.hour,
                             domain_code = cr.domain_code,
                             page_title = cr.page_title,
                             count_views = mv.count_views
                         };

            return result.AsParallel().ToArray();
        }
    }

    public class SystemTimeService : IDateTimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;

        public DateTime Now => DateTime.Now;
    }
}