using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class CheckIn_Info
    {
        [Key]
        public int id { get; set; }
        public int number_baby { get; set; }
        public int number_child { get; set; }
        public int number_adult { get; set; }
        public string nationality { get; set; }
        public int nationality_id { get; set; }
        public int checkin_id { get; set; }
    }
}