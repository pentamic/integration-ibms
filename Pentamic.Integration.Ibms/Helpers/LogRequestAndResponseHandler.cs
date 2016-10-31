using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Pentamic.Integration.Ibms.Helpers
{
    public class LogRequestAndResponseHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string path = HttpContext.Current.Server.MapPath("~/logs");
            var logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.RollingFile(path + "/log-{Date}.txt", shared: true).CreateLogger();
            // log request body
            string requestBody = await request.Content.ReadAsStringAsync();
            logger.Information("Request body: " +requestBody);

            var result = await base.SendAsync(request, cancellationToken);
            return result;
        }
    }
}