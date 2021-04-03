using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Net.Http;
using System.Threading;

namespace WikiMedia.UnitTests
{
    public class FakeMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> requestDelegate;

        public FakeMessageHandler(Func<HttpRequestMessage,Task<HttpResponseMessage>> requestDelegate = null)
        {
            this.requestDelegate = requestDelegate;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (requestDelegate != null) return await requestDelegate(request);

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "WikiMedia.UnitTests.prueba.gz";
            var fakeResult = assembly.GetManifestResourceStream(resourceName);

            return new HttpResponseMessage
            {
                Content = new StreamContent(fakeResult),
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }
    }
}
