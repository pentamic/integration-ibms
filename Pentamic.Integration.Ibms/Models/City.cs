﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class City
    {
        [Key]
        public int Id { get; set; }
        public int IDs { get; set; }
        public string Name { get; set; }
        public int CountryId { get; set; }
        public string LastSync { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}