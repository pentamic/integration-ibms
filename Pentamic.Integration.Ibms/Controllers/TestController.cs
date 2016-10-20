using Newtonsoft.Json;
using Pentamic.Integration.Ibms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace Pentamic.Integration.Ibms.Controllers
{
    [RoutePrefix("api/test")]
    public class TestController : ApiController
    {
        [HttpPost]
        [Route("string")]
        public string String([FromBody]string value)
        {
            return $"String received: {value}";
        }

        [HttpPost]
        [Route("base64")]
        public Package Base64([FromBody]string value)
        {
            if (value == null)
            {
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
                var decodedData = Convert.FromBase64String(value);
                var jsonString = Encoding.UTF8.GetString(decodedData);
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
        public Package Json([FromBody]Package package)
        {
            return new Package
            {
                ProtocolId = package.ProtocolId,
                ProtocolStatus = true,
                Partner_name = "",
                Partner_pass = "",
                ErrCode = 0,
                ErrMessage = null
            };
        }

    }
}
