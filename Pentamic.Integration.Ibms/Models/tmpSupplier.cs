using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    [DataContract(Name = "supplier")]
    public class tmpSupplier
    {
        [Key]
        public int Id { get; set; }
        [DataMember(Name = "id")]
        public int IDs { get; set; }
        [DataMember(Name = "supplier_code")]
        public string Code { get; set; }
        [DataMember(Name = "supplier_name")]
        public string Name { get; set; }
    }
}