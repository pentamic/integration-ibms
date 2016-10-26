using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class Location
    {
        [Key]
        public int id { get; set; }
        public string branchName { get; set; }
        public string address { get; set; }
    }
}