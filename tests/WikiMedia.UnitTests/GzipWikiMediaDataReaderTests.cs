using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using WikiMedia.Terminal;

namespace WikiMedia.UnitTests
{

    [TestClass]
    public class GzipWikiMediaDataReaderTests
    {
        private GzipWikimediaDataReader reader = null;

        [TestInitialize]
        public void TestInitialize()
        {
            var httpClient = new HttpClient(new FakeMessageHandler());
            reader = new GzipWikimediaDataReader(httpClient);
        }

        [TestMethod]
        public async Task Given_a_valid_datetime_When_GetDataByHourAsync_is_called_Then_returns_a_WikiMediaRows_Enumerable()
        {
            //Arrange
            var dateTime = DateTime.ParseExact("01/01/2021 02", "dd/MM/yyyy HH", CultureInfo.InvariantCulture);
            var wikiMediaRowsExpected = new WikiMediaRow[] {
                new WikiMediaRow
                {
                    hour = "2AM",
                    domain_code = "aa",
                    page_title = "Main_Page",
                    count_views = 6,
                    total_response_size = 0
                }
            };

            //Act
            var wikiMediaRowsResult = (await reader.GetDataByHourAsync(dateTime)).ToArray();

            //Assert
            wikiMediaRowsExpected.Should().BeEquivalentTo(wikiMediaRowsResult);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException),
            "StatusCode is not handled by DownloadFileAsync")]
        public async Task Given_400_status_code_When_DownloadFileAsync_is_Called_Then_ThrowException()
        {
            var url = "https://dumps.wikimedia.org/other/pageviews/2021/2021-01/pageviews-20210101-020000.gz";
            reader = new GzipWikimediaDataReader(new HttpClient(new FakeMessageHandler
                (async (r) =>
            {
                Assert.AreEqual(url, r.RequestUri.ToString());

                return new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            })));

            var contentResult = await reader.DownloadFileAsync(url);
        }

        [TestMethod]
        public async Task Given_a_valid_url_When_DownloadFileAsync_is_Called_Then_returns_plain_text_file()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "WikiMedia.UnitTests.prueba.gz";
            var url = "https://dumps.wikimedia.org/other/pageviews/2021/2021-01/pageviews-20210101-020000.gz";
            const string expected = "aa Main_Page 6 0";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                var contentResult = await reader.DownloadFileAsync(url);
                var streamReader = new StreamReader(contentResult);
                var result = await streamReader.ReadToEndAsync();

                Assert.AreEqual(expected, result);
            }
        }

        [TestMethod]
        public async Task Given_a_valid_gzip_file_When_ExtractGZipMemory_Then_returns_a_uncompressed_stream()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "WikiMedia.UnitTests.prueba.gz";
            const string expected = "aa Main_Page 6 0";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                var contentResult = await reader.ExtractGZipMemory(stream);
                var streamReader = new StreamReader(contentResult);
                var result = await streamReader.ReadToEndAsync();

                Assert.AreEqual(expected, result);
            }
        }

        [TestMethod]
        public void Given_a_valid_date_When_GetValuesDateTime_is_called_Then_returns_EDateTime()
        {
            //Arrange
            var dateTimeBetweenYears = DateTime.ParseExact("01/01/2021 02", "dd/MM/yyyy HH", CultureInfo.InvariantCulture);
            var dateTimeBetweenDays = DateTime.ParseExact("28/03/2021 00", "dd/MM/yyyy HH", CultureInfo.InvariantCulture);
            var dateTimeNormal = DateTime.ParseExact("29/03/2021 14", "dd/MM/yyyy HH", CultureInfo.InvariantCulture);

            var expected1 = new EDateTime() { yyyy = "2021", MM = "01", dd = "01", HH = "02", htt = "2AM" };
            var expected2 = new EDateTime() { yyyy = "2021", MM = "03", dd = "28", HH = "00", htt = "12AM" };
            var expected3 = new EDateTime() { yyyy = "2021", MM = "03", dd = "29", HH = "14", htt = "2PM" };

            //Act
            var result1 = reader.GetValuesDateTime(dateTimeBetweenYears);
            var result2 = reader.GetValuesDateTime(dateTimeBetweenDays);
            var result3 = reader.GetValuesDateTime(dateTimeNormal);

            //Assert
            expected1.Should().BeEquivalentTo(result1);
            expected2.Should().BeEquivalentTo(result2);
            expected3.Should().BeEquivalentTo(result3);
        }

        [TestMethod]
        public void Given_a_valid_date_When_GenerateUrl_is_called_Then_returns_valid_url()
        {
            //Arrange
            var date1 = DateTime.ParseExact("01/01/2021 02", "dd/MM/yyyy HH", CultureInfo.InvariantCulture);
            var date2 = DateTime.ParseExact("28/03/2021 00", "dd/MM/yyyy HH", CultureInfo.InvariantCulture);
            var date3 = DateTime.ParseExact("29/04/2020 14", "dd/MM/yyyy HH", CultureInfo.InvariantCulture);

            var expected1 = "https://dumps.wikimedia.org/other/pageviews/2021/2021-01/pageviews-20210101-020000.gz";
            var expected2 = "https://dumps.wikimedia.org/other/pageviews/2021/2021-03/pageviews-20210328-000000.gz";
            var expected3 = "https://dumps.wikimedia.org/other/pageviews/2020/2020-04/pageviews-20200429-140000.gz";

            //Act
            var result1 = reader.GenerateUrl(date1);
            var result2 = reader.GenerateUrl(date2);
            var result3 = reader.GenerateUrl(date3);

            //Assert
            Assert.AreEqual(expected1, result1);
            Assert.AreEqual(expected2, result2);
            Assert.AreEqual(expected3, result3);
        }

        [TestMethod]
        public void Given_a_valid_date_When_GenerateFileName_is_called_Then_returns_valid_filename()
        {
            //Arrange
            var date1 = DateTime.ParseExact("01/01/2021 02", "dd/MM/yyyy HH", CultureInfo.InvariantCulture);
            var date2 = DateTime.ParseExact("28/03/2021 00", "dd/MM/yyyy HH", CultureInfo.InvariantCulture);
            var date3 = DateTime.ParseExact("29/04/2020 14", "dd/MM/yyyy HH", CultureInfo.InvariantCulture);

            var expected1 = "pageviews-20210101-020000";
            var expected2 = "pageviews-20210328-000000";
            var expected3 = "pageviews-20200429-140000";

            //Act
            var result1 = reader.GenerateFileName(date1);
            var result2 = reader.GenerateFileName(date2);
            var result3 = reader.GenerateFileName(date3);

            //Assert
            Assert.AreEqual(expected1, result1);
            Assert.AreEqual(expected2, result2);
            Assert.AreEqual(expected3, result3);
        }

        [TestMethod]
        public void Given_a_valid_stream_When_ReadFile_is_called_Then_returns_WikiMediaRows()
        {
            //Arrange
            using (var streamValid = new MemoryStream())
            {
                var writer = new StreamWriter(streamValid);
                writer.WriteLine("aa Main_Page 6 0");
                writer.WriteLine("aa Special:Statistics 1 0");
                writer.Flush();
                streamValid.Seek(0, SeekOrigin.Begin);

                var wikiMediaRowsExpected = new WikiMediaRow[] {
                    new WikiMediaRow
                    {
                        domain_code = "aa",
                        page_title = "Main_Page",
                        count_views = 6,
                        total_response_size = 0
                    },
                    new WikiMediaRow
                    {
                        domain_code = "aa",
                        page_title = "Special:Statistics",
                        count_views = 1,
                        total_response_size = 0
                    }
                };


                //Act
                var wikiMediaRowsResult = reader.ReadFile(streamValid).ToArray();

                //Assert
                wikiMediaRowsExpected.Should().BeEquivalentTo(wikiMediaRowsResult);
            }
        }
    }
}
