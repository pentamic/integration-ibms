﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    [DataContract(Name = "coffer")]
    public class Coffer
    {
        [Key]
        public int Id { get; set; }
        [DataMember(Name = "id")]
        public int IDs { get; set; }
        [DataMember(Name = "coffer_name")]
        public string Name { get; set; }
        [DataMember(Name = "branch_id")]
        public int? BranchId { get; set; }
        [DataMember(Name = "type")]
        public int Type { get; set; }
        public string LastSync { get; set; }
        public string BranchCode { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}