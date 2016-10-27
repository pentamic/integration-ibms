using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    [DataContract(Name = "partner_group")]
    public class PartnerGroup
    {
        [Key]
        public int Id { get; set; }
        [DataMember(Name = "id")]
        public int IDs { get; set; }
        [DataMember(Name = "fullName")]
        public string Name { get; set; }

        [DataMember(Name = "partner")]
        public Partner partner { get; set; }
    }
}