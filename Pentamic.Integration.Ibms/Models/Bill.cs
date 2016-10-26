using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class Bill
    {
        public int id { get; set; }
        public string bill_code { get; set; }
        public double amount { get; set; }
    }
}