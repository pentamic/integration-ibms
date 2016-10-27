using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class Driver
    {
        [Key]
        public int id { get; set; }
        public string fullName { get; set; }
    }
}