﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class tblPartner
    {
        [Key]
        public int Id { get; set; }
        public int IDs { get; set; }
        public string Name { get; set; }
        public int? PartnerGroupId { get; set; }
        public string LastSync { get; set; }
    }
}