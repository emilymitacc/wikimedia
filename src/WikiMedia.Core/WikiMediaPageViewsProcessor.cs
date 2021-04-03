using System;
using System.Collections.Generic;
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

        public WikiMediaPageViewsProcessor(IWikimediaDataReader wikimediaDataReader, IStatsOutput statsOutput, IDateTimeService dateTimeService)
        {
            this.wikimediaDataReader = wikimediaDataReader;
            this.statsOutput = statsOutput;
            this.dateTimeService = dateTimeService;
        }

        public async Task Process(WikiMediaRunOptions options)
        {
            var listDateTime = GetListDateWithHour(dateTimeService.UtcNow, options.LastHours);

            var tasks = listDateTime.Select(async(dt) =>
            {
                var rawData = await wikimediaDataReader.GetDataByHourAsync(dt);
                var result = SummarizeViewsPerDomainCodeAndPageTitle(rawData);
                var wikiMediaOutputContext = new WikiMediaOutputContext(result);
                return wikiMediaOutputContext;
            });

            await Task.WhenAll(tasks);

            foreach (var taskCompleted in tasks)
            {
                await statsOutput.WriteOutput(taskCompleted.Result);
            }
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
            var consolidatedRows = from w in wikiMediaRows.AsParallel()
                                   group w by new { w.hour, w.domain_code, w.page_title }
                                   into q
                                   select new WikiMediaRow
                                   {
                                       hour = q.Key.hour,
                                       domain_code = q.Key.domain_code,
                                       page_title = q.Key.page_title,
                                       count_views = q.Sum(x => x.count_views)
                                   };

            var maxViewsPerDomainCode = from w in consolidatedRows.AsParallel()
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

            return result.ToArray();
        }
    }

    public class SystemTimeService : IDateTimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;

        public DateTime Now => DateTime.Now;
    }
}