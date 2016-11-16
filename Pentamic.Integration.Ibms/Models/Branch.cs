using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    [DataContract(Name = "location")]
    public class Branch
    {
        [Key]
        public int Id { get; set; }
        [DataMember(Name = "branchId")]
        public int IDs { get; set; }
        [DataMember(Name = "branchName")]
        public string BranchName { get; set; }
        [DataMember(Name = "maChiNhanh")]
        public string BranchCode { get; set; }
        [DataMember(Name = "address")]
        public string Address { get; set; }
        public string LastSync { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}