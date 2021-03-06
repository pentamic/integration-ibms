﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class DataSync
    {
        [Key]
        public int Id { get; set; }
        public bool Status { get; set; }
        public string LastSync { get; set; }
        public string RecordIdFailure { get; set; }
        public int TotalRecord { get; set; }
        public int RecordSuccess { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
    }
}