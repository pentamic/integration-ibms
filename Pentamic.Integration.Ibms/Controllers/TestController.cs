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
        public Protocol Base64([FromBody]string value)
        {
            Log.Information("Request: ,{Content-Type}, {Charset}", Request.Content.Headers.ContentType.MediaType, Request.Content.Headers.ContentType.CharSet);
            if (value == null)
            {
                Log.Warning("Received null string");
                return new Protocol
                {
                    protocol_id = 0,
                    branch_id = 0,
                    protocol_status = false,
                    partner_name = "",
                    partner_pass = "",
                    err_code = 0,
                    err_message = "Input string is null"
                };
            }
            try
            {
                Log.Information("Received base64 string {value}", value);
                var decodedData = Convert.FromBase64String(value);
                var jsonString = Encoding.UTF8.GetString(decodedData);
                Log.Information("Decoded json string from base64 {0}", jsonString);
                var model = JsonConvert.DeserializeObject<Protocol>(jsonString);
                return new Protocol
                {
                    protocol_id = model.protocol_id,
                    branch_id = model.branch_id,
                    protocol_status = true,
                    partner_name = "",
                    partner_pass = "",
                    err_code = 0,
                    err_message = null
                };
            }
            catch (Exception e)
            {
                Log.Error(e, "Error get package value");
                return new Protocol
                {
                    protocol_id = 0,
                    branch_id = 0,
                    protocol_status = false,
                    partner_name = "",
                    partner_pass = "",
                    err_code = 0,
                    err_message = e.Message
                };
            }
        }


        [HttpPost]
        [Route("json")]
        public IHttpActionResult Json(Protocol protocol)
        {
            Log.Information("Request: {MediaType}", Request.Content.Headers.ContentType.MediaType);
            Log.Information("Received package: {Package}", JsonConvert.SerializeObject(protocol));
            var result = protocol ?? new Protocol
            {
                protocol_status = false
            };
            return Ok(result);
        }

        [HttpPost]
        [Route("json2")]
        public HttpResponseMessage Json2(Protocol package)
        {
            Log.Information("Request: {MediaType}", Request.Content.Headers.ContentType.MediaType);
            Log.Information("Received package: {Package}", JsonConvert.SerializeObject(package));
            var result = package ?? new Protocol
            {
                protocol_status = false
            };
            return new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8, "application/json"),
            };
        }

        [HttpPost]
        [Route("json3")]
        public Protocol Json3([FromBody]Protocol package)
        {
            Log.Information("Request: {MediaType}", Request.Content.Headers.ContentType.MediaType);
            Log.Information("Received package: {Package}", JsonConvert.SerializeObject(package));
            return package ?? new Protocol
            {
                protocol_status = false
            };
        }
    }
}
