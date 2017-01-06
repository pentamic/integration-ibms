using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    [DataContract(Name = "fee_port_contact")]
    public class tmpFeePort
    {
        [Key]
        public int Id { get; set; }
        [DataMember(Name = "id")]
        public int IDs { get; set; }
        [DataMember(Name = "fullName")]
        public string Name { get; set; }
        [DataMember(Name = "emailAddress")]
        public string Email { get; set; }
        [DataMember(Name = "phoneNumber")]
        public string Phone { get; set; }
        [DataMember(Name = "fee_port")]
        public decimal FeePort { get; set; }
    }
}