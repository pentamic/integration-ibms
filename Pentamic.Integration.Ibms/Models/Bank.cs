﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class Bank
    {
        [Key]
        public int Id { get; set; }
        [DataMember(Name = "id")]
        public int IDs { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        public string LastSync { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
    }
}