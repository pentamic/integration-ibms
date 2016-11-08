using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public int IDs { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string Shape { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string Pearl { get; set; }
        public string MetaType { get; set; }
        public string MetalWeight { get; set; }
        public string Diamond { get; set; }
        public string DiamondWeight { get; set; }
        public string GemStoneType { get; set; }
        public string GemStoneWeight { get; set; }
        public string PearlWeight { get; set; }
        public string TotalWeight { get; set; }
        public int? Type { get; set; }
        public string LastSync { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}