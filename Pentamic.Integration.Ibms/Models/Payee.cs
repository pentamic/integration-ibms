using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class Payee
    {
        [Key]
        public int id { get; set; }
        public string payee_name { get; set; }
        public int payee_commission { get; set; }
        public string payee_phone { get; set; }
    }
}