﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class tblCheckIn
    {
        [Key]
        public int Id { get; set; }
        public int IDs { get; set; }
        public string CheckInCode { get; set; }
        public double FeePort { get; set; }
        public bool Status { get; set; }

        public int? CardTypeId { get; set; }
        public int? CardMakerId { get; set; }

        public int? DriverId { get; set; }
        public int? TourGuideId { get; set; }
        public int? VisitorTypeId { get; set; }
        public int? LocationId { get; set; }
        public string LastSync { get; set; }
        public int? PartnerId { get; set; }

    }
}