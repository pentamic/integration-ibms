using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace Pentamic.Integration.Ibms.Helpers
{
    public class WebApiExceptionLogger : IExceptionLogger
    {

        /// <summary>
        /// 
        /// </summary>
        public static Action<HttpConfiguration> Register
            => c => c.Services.Add(typeof(IExceptionLogger),
                new WebApiExceptionLogger());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            HttpContext.Current.AddError(context.Exception);

            return Task.FromResult(0);
        }
    }
}