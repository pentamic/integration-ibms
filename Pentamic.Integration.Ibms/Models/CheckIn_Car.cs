﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class CheckIn_Car
    {
        [Key]
        public int Id { get; set; }
        public int CheckInId { get; set; }
        public string CarNumber { get; set; }
        public string LastSync { get; set; }
    }
}