using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class Package
    {
        public int ProtocolId { get; set; }
        public int ErrCode { get; set; }
        public string ErrMessage { get; set; }
        public bool ProtocolStatus { get; set; }
        public string Partner_name { get; set; }
        public string Partner_pass { get; set; }
    }
}