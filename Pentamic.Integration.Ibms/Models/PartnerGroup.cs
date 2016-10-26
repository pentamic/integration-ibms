﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class PartnerGroup
    {
        [Key]
        public int id { get; set; }
        public string fullName { get; set; }
        public Partner partner { get; set; }
    }
}