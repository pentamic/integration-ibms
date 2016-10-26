using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class Nationality
    {
        [Key]
        public int id { get; set; }
        public string nationality_name { get; set; }
    }
}