using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using WikiMedia.Core;
using WikiMedia.Core.Interfaces;
using WikiMedia.Domain;

namespace WikiMedia.UnitTests
{

    [TestClass]
    public class WikiMediaPageViewsProcessorTests
    {
        private IWikimediaDataReader reader;
        private IStatsOutput output;
        private IDateTimeService timeService;
        private ILogger<WikiMediaPageViewsProcessor> logger;
        private WikiMediaPageViewsProcessor processor = null;

        [TestInitialize]
        public void TestInitialize()
        {
            reader = A.Fake<IWikimediaDataReader>();
            output = A.Fake<IStatsOutput>();
            timeService = A.Fake<IDateTimeService>();
            logger = A.Fake<ILogger<WikiMediaPageViewsProcessor>>();
            processor = new WikiMediaPageViewsProcessor(reader, output, timeService, logger);
        }

        [TestMethod]
        public void Given_options_with_5_hours_When_Process_is_called_Then_call_output()
        {
            var options = new WikiMediaRunOptions() { LastHours = 5 };
            
        }

        [TestMethod]
        public void Given_a_valid_wikimediarows_When_DomainCodeAndPageViewWithMaxViews_is_called_Then_returns_a_list_with_max_views_from_domaincode_with_pagetitle()
        {
            //Arrange
            var list = new WikiMediaRow[] 
            {
                new WikiMediaRow
                {
                    hour = "2AM",
                    domain_code = "aa",
                    page_title = "Main_Page",
                    count_views = 6,
                    total_response_size = 0
                },
                new WikiMediaRow
                {
                    hour = "2AM",
                    domain_code = "aa",
                    page_title = "main_Page",
                    count_views = 6,
                    total_response_size = 0
                },
                new WikiMediaRow
                {
                    hour = "2AM",
                    domain_code = "aa",
                    page_title = "Microsoft",
                    count_views = 1,
                    total_response_size = 0
                },
                new WikiMediaRow
                {
                    hour = "2AM",
                    domain_code = "en",
                    page_title = "Office",
                    count_views = 100,
                    total_response_size = 0
                },
                new WikiMediaRow
                {
                    hour = "2AM",
                    domain_code = "en",
                    page_title = "Excel",
                    count_views = 10,
                    total_response_size = 0
                }
            };          

            var wikiMediaRowsExpected = new WikiMediaRow[]
            {
                new WikiMediaRow
                {
                    hour = "2AM",
                    domain_code = "aa",
                    page_title = "Main_Page",
                    count_views = 6,
                    total_response_size = 0
                },
                new WikiMediaRow
                {
                    hour = "2AM",
                    domain_code = "aa",
                    page_title = "main_Page",
                    count_views = 6,
                    total_response_size = 0
                },
                new WikiMediaRow
                {
                    hour = "2AM",
                    domain_code = "en",
                    page_title = "Office",
                    count_views = 100,
                    total_response_size = 0
                }
            };

            //Act
            var wikiMediaRowsResult = processor.SummarizeViewsPerDomainCodeAndPageTitle(list);

            //Assert
            wikiMediaRowsExpected.Should().BeEquivalentTo(wikiMediaRowsResult);
        }

        [TestMethod]
        public void Given_a_valid_date_When_GetListDateWithHour_is_called_Then_returns_a_list_from_past_5_hours()
        {
            //Arrange
            var dateTimeBetweenYears = DateTime.ParseExact("01/01/2021 02", "dd/MM/yyyy HH", CultureInfo.InvariantCulture);
            var dateTimeBetweenDays = DateTime.ParseExact("28/03/2021 03", "dd/MM/yyyy HH", CultureInfo.InvariantCulture);
            var dateTimeNormal = DateTime.ParseExact("29/03/2021 14", "dd/MM/yyyy HH", CultureInfo.InvariantCulture);

            var expected1 = new List<DateTime>();
            var expected2 = new List<DateTime>();
            var expected3 = new List<DateTime>();

            expected1.Add(DateTime.ParseExact("01/01/2021 01", "dd/MM/yyyy HH", CultureInfo.InvariantCulture));
            expected1.Add(DateTime.ParseExact("01/01/2021 00", "dd/MM/yyyy HH", CultureInfo.InvariantCulture));
            expected1.Add(DateTime.ParseExact("31/12/2020 23", "dd/MM/yyyy HH", CultureInfo.InvariantCulture));
            expected1.Add(DateTime.ParseExact("31/12/2020 22", "dd/MM/yyyy HH", CultureInfo.InvariantCulture));
            expected1.Add(DateTime.ParseExact("31/12/2020 21", "dd/MM/yyyy HH", CultureInfo.InvariantCulture));

            expected2.Add(DateTime.ParseExact("28/03/2021 02", "dd/MM/yyyy HH", CultureInfo.InvariantCulture));
            expected2.Add(DateTime.ParseExact("28/03/2021 01", "dd/MM/yyyy HH", CultureInfo.InvariantCulture));
            expected2.Add(DateTime.ParseExact("28/03/2021 00", "dd/MM/yyyy HH", CultureInfo.InvariantCulture));
            expected2.Add(DateTime.ParseExact("27/03/2021 23", "dd/MM/yyyy HH", CultureInfo.InvariantCulture));
            expected2.Add(DateTime.ParseExact("27/03/2021 22", "dd/MM/yyyy HH", CultureInfo.InvariantCulture));

            expected3.Add(DateTime.ParseExact("29/03/2021 13", "dd/MM/yyyy HH", CultureInfo.InvariantCulture));
            expected3.Add(DateTime.ParseExact("29/03/2021 12", "dd/MM/yyyy HH", CultureInfo.InvariantCulture));
            expected3.Add(DateTime.ParseExact("29/03/2021 11", "dd/MM/yyyy HH", CultureInfo.InvariantCulture));
            expected3.Add(DateTime.ParseExact("29/03/2021 10", "dd/MM/yyyy HH", CultureInfo.InvariantCulture));
            expected3.Add(DateTime.ParseExact("29/03/2021 09", "dd/MM/yyyy HH", CultureInfo.InvariantCulture));

            //Act
            var result1 = processor.GetListDateWithHour(dateTimeBetweenYears, 5);
            var result2 = processor.GetListDateWithHour(dateTimeBetweenDays, 5);
            var result3 = processor.GetListDateWithHour(dateTimeNormal, 5);

            //Assert
            CollectionAssert.AreEqual(expected1, result1);
            CollectionAssert.AreEqual(expected2, result2);
            CollectionAssert.AreEqual(expected3, result3);
        }
    }
}
