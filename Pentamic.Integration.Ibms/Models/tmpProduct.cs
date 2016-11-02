using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    [DataContract(Name = "lstprd")]
    public class tmpProduct
    {
        [Key]
        public int Id { get; set; }
        [DataMember(Name = "id")]
        public int IDs { get; set; }
        [DataMember(Name = "productCode")]
        public string Code { get; set; }
        [DataMember(Name = "productName")]
        public string Name { get; set; }
        [DataMember(Name = "unit")]
        public string Unit { get; set; }
        [DataMember(Name = "shape")]
        public string Shape { get; set; }
        [DataMember(Name = "size")]
        public string Size { get; set; }
        [DataMember(Name = "color")]
        public string Color { get; set; }
        [DataMember(Name = "pearl")]
        public string Pearl { get; set; }
        [DataMember(Name = "metaType")]
        public string MetaType { get; set; }
        [DataMember(Name = "metalWeight")]
        public string MetalWeight { get; set; }
        [DataMember(Name = "diamond")]
        public string Diamond { get; set; }
        [DataMember(Name = "diamondWeight")]
        public string DiamondWeight { get; set; }
        [DataMember(Name = "gemstoneType")]
        public string GemStoneType { get; set; }
        [DataMember(Name = "gemstoneWeight")]
        public string GemStoneWeight { get; set; }
        [DataMember(Name = "pearlWeight")]
        public string PearlWeight { get; set; }
        [DataMember(Name = "totalWeight")]
        public string TotalWeight { get; set; }
        [DataMember(Name = "type")]
        public int? Type { get; set; }
        public string LastSync { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
    }
}