using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class VisitorType
    {
        [Key]
        public int id { get; set; }
        public string visitor_type_name { get; set; }
    }
}