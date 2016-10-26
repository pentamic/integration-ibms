using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class Partner
    {
        [Key]
        public int id { get; set; }
        public string fullName { get; set; }
        public int? partner_group_id { get; set; }
    }
}