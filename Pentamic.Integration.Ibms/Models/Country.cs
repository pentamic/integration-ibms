using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    [DataContract(Name = "country")]
    public class Country
    {
        [Key]
        public int Id { get; set; }
        [DataMember(Name = "id")]
        public int IDs { get; set; }
        [DataMember(Name = "countryCode")]
        public string Code { get; set; }
        [DataMember(Name = "countryName")]
        public string Name { get; set; }
        [DataMember(Name = "cityList")]
        public List<City> cityList { get; set; }
    }
}