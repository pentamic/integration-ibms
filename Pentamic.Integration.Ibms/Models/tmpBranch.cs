using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    [DataContract(Name = "lstBranch")]
    public class tmpBranch
    {
        [Key]
        public int Id { get; set; }
        [DataMember(Name = "branchId")]
        public int IDs { get; set; }
        [DataMember(Name = "maChiNhanh")]
        public string BranchCode { get; set; }
        [DataMember(Name = "branchName")]
        public string BranchName { get; set; }
        [DataMember(Name = "address")]
        public string Address { get; set; }
        [DataMember(Name = "phoneNumber")]
        public string Phone { get; set; }
        [DataMember(Name = "country")]
        public tmpCountry Country { get; set; }
    }
}