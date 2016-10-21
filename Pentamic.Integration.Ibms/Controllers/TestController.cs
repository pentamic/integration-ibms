using Newtonsoft.Json;
using Pentamic.Integration.Ibms.Models;
using Serilog;
using SerilogWeb.Classic.Enrichers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Http;
using Pentamic.Integration.Ibms.Helpers;

namespace Pentamic.Integration.Ibms.Controllers
{
    [RoutePrefix("api/test")]
    public class TestController : ApiController
    {
        public TestController()
        {
            string path = HttpContext.Current.Server.MapPath("~/logs");
            Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose().Enrich.With<HttpRequestIdEnricher>().WriteTo.RollingFile(path + "/log-{Date}.txt", shared: true).CreateLogger();
        }

        [HttpPost]
        [Route("string")]
        public string String([FromBody]string value)
        {
            Log.Information("Request: {MediaType}", Request.Content.Headers.ContentType.MediaType);
            Log.Information("Received string: {value}", value);
            return $"String received: {value}";
        }

        [HttpPost]
        [Route("base64")]
        public Package Base64([FromBody]string value)
        {
            Log.Information("Request: ,{Content-Type}, {Charset}", Request.Content.Headers.ContentType.MediaType, Request.Content.Headers.ContentType.CharSet);
            if (value == null)
            {
                Log.Warning("Received null string");
                return new Package
                {
                    ProtocolId = 0,
                    ProtocolStatus = false,
                    Partner_name = "",
                    Partner_pass = "",
                    ErrCode = 0,
                    ErrMessage = "Input string is null"
                };
            }
            try
            {
                Log.Information("Received base64 string {value}", value);
                var decodedData = Convert.FromBase64String(value);
                var jsonString = Encoding.UTF8.GetString(decodedData);
                Log.Information("Decoded json string from base64 {0}", jsonString);
                var model = JsonConvert.DeserializeObject<Package>(jsonString);
                return new Package
                {
                    ProtocolId = model.ProtocolId,
                    ProtocolStatus = true,
                    Partner_name = "",
                    Partner_pass = "",
                    ErrCode = 0,
                    ErrMessage = null
                };
            }
            catch (Exception e)
            {
                Log.Error(e, "Error get package value");
                return new Package
                {
                    ProtocolId = 0,
                    ProtocolStatus = false,
                    Partner_name = "",
                    Partner_pass = "",
                    ErrCode = 0,
                    ErrMessage = e.Message
                };
            }
        }


        [HttpPost]
        [Route("json")]
        public IHttpActionResult Json(Package protocol)
        {
            Log.Information("Request: {MediaType}", Request.Content.Headers.ContentType.MediaType);
            Log.Information("Received package: {Package}", JsonConvert.SerializeObject(protocol));
            var result = protocol ?? new Package
            {
                ProtocolStatus = false
            };
            return Ok(result);
        }

        [HttpPost]
        [Route("json2")]
        public HttpResponseMessage Json2(Package package)
        {
            Log.Information("Request: {MediaType}", Request.Content.Headers.ContentType.MediaType);
            Log.Information("Received package: {Package}", JsonConvert.SerializeObject(package));
            var result = package ?? new Package
            {
                ProtocolStatus = false
            };
            return new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8, "application/json"),
            };
        }

        [HttpPost]
        [Route("json3")]
        public Package Json3([FromBody]Package package)
        {
            Log.Information("Request: {MediaType}", Request.Content.Headers.ContentType.MediaType);
            Log.Information("Received package: {Package}", JsonConvert.SerializeObject(package));
            return package ?? new Package
            {
                ProtocolStatus = false
            };
        }
    }
}
