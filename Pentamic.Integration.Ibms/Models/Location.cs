﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    [DataContract(Name = "location")]
    public class Location
    {
        [Key]
        public int Id { get; set; }
        [DataMember(Name = "branchId")]
        public int IDs { get; set; }
        [DataMember(Name = "branchName")]
        public string BranchName { get; set; }
        [DataMember(Name = "address")]
        public string Address { get; set; }
        public string LastSync { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
    }
}