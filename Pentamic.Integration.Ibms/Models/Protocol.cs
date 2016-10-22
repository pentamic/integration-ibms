using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class Protocol
    {
        public int protocol_id { get; set; }
        public int branch_id { get; set; }
        public int err_code { get; set; }
        public string err_message { get; set; }
        public bool protocol_status { get; set; }
        public string partner_name { get; set; }
        public string partner_pass { get; set; }
    }
}