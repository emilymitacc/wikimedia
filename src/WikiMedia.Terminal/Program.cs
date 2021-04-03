using Autofac;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using Microsoft.Extensions.Http;
using Polly;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WikiMedia.Terminal
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var builder = new ContainerBuilder();
            
            var retryPolicy = Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.ServiceUnavailable)
                .WaitAndRetryAsync(20, i => TimeSpan.FromSeconds(7));

            builder.RegisterInstance(new HttpClient(new PolicyHttpMessageHandler(retryPolicy) { 
                InnerHandler = new HttpClientHandler()
            }));
            builder.RegisterType<GzipWikimediaDataReader>().AsImplementedInterfaces();
            builder.RegisterType<ConsoleOutput>().AsImplementedInterfaces();
            builder.RegisterType<SystemTimeService>().AsImplementedInterfaces();
            builder.RegisterType<WikiMediaPageViewsProcessor>().AsSelf();            

            var container = builder.Build();

            Console.WriteLine($"INICIO: {DateTime.Now}");

            var options = new WikiMediaRunOptions() { LastHours = 5 };
            var processor = container.Resolve<WikiMediaPageViewsProcessor>();
            processor.Process(options).Wait();

            Console.WriteLine($"FIN: {DateTime.Now}");

            Console.ReadLine();
        }     
    }
}
