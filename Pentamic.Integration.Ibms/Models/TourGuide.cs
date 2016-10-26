using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class TourGuide
    {
        [Key]
        public int id { get; set; }
        public string fullName { get; set; }
        public bool sex { get; set; }
        public int relationship { get; set; }
        public int shoppingPurpose { get; set; }
        public int shoppingType { get; set; }
        public int salary { get; set; }
        public int partner_group { get; set; }
    }
}