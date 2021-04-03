using ICSharpCode.SharpZipLib.GZip;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WikiMedia.Terminal
{
    public class GzipWikimediaDataReader : IWikimediaDataReader
    {
        public static string principalUrl = "https://dumps.wikimedia.org/other/pageviews";
        private readonly HttpClient httpClient;

        public GzipWikimediaDataReader(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<IEnumerable<WikiMediaRow>> GetDataByHourAsync(DateTime dateTime)
        {
            var url = GenerateUrl(dateTime);
            var valuesDate = GetValuesDateTime(dateTime);
            var hour = valuesDate.htt;
            var content = await DownloadFileAsync(url);
            return ReadFile(content).Select(l =>
            {
                l.hour = hour;
                return l;
            });
        }

        public async Task<Stream> DownloadFileAsync(string url)
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            using (var fileContent = await response.Content.ReadAsStreamAsync())
            {
                var extracted = await ExtractGZipMemory(fileContent);
                return extracted;
            }
        }

        public async Task<Stream> ExtractGZipMemory(Stream sm)
        {
            var extractedStream = new MemoryStream();
            using (GZipInputStream gzipStream = new GZipInputStream(sm))
            {
                // Use a 4K buffer. Any larger is a waste.    
                await gzipStream.CopyToAsync(extractedStream, 4096);
            }
            extractedStream.Seek(0, SeekOrigin.Begin);
            return extractedStream;
        }

        public IEnumerable<WikiMediaRow> ReadFile(Stream fileContent)
        {
            var rows = new List<WikiMediaRow>();
            using (var file = new StreamReader(fileContent))
            {
                while (!file.EndOfStream)
                {
                    var line = file.ReadLine();
                    string[] parts = line.Split(' ');
                    rows.Add(new WikiMediaRow
                    {
                        domain_code = parts[0],
                        page_title = parts[1],
                        count_views = long.Parse(parts[2]),
                        total_response_size = long.Parse(parts[3]),
                    });
                }
            }
            return rows;
        }

        public EDateTime GetValuesDateTime(DateTime dateTime)
        {
            var newDateTime = new EDateTime();

            newDateTime.yyyy = dateTime.ToString("yyyy", CultureInfo.InvariantCulture);
            newDateTime.MM = dateTime.ToString("MM", CultureInfo.InvariantCulture);
            newDateTime.dd = dateTime.ToString("dd", CultureInfo.InvariantCulture);
            newDateTime.HH = dateTime.ToString("HH", CultureInfo.InvariantCulture);
            newDateTime.htt = dateTime.ToString("htt", CultureInfo.InvariantCulture);

            return newDateTime;
        }

        public string GenerateFileName(DateTime dateTime)
        {
            var value = GetValuesDateTime(dateTime);
            return $"pageviews-{value.yyyy}{value.MM}{value.dd}-{value.HH}0000";
        }

        public string GenerateUrl(DateTime dateTime)
        {
            var value = GetValuesDateTime(dateTime);
            var fileName = GenerateFileName(dateTime);
            return $"{principalUrl}/{value.yyyy}/{value.yyyy}-{value.MM}/{fileName}.gz";
        }
    }
}
