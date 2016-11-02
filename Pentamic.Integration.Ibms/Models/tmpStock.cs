using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    [DataContract(Name = "lstprd")]
    public class tmpStock
    {
        [Key]
        public int Id { get; set; }
        [DataMember(Name = "id")]
        public int ProductId { get; set; }
        [DataMember(Name = "productCode")]
        public string ProductCode { get; set; }
        [DataMember(Name = "productName")]
        public string ProductName { get; set; }
        [DataMember(Name = "price")]
        public decimal Price { get; set; }
        [DataMember(Name = "costPrice")]
        public decimal CostPrice { get; set; }
        [DataMember(Name = "createDate")]
        public DateTime CreateDate { get; set; }
        [DataMember(Name = "quantity")]
        public float Quantity { get; set; }
        [DataMember(Name = "coffer")]
        public Coffer Coffer { get; set; }
        [DataMember(Name = "type")]
        public int Type { get; set; }
        [DataMember(Name = "lstBranch")]
        public List<Branch> Branch { get; set; }
    }
}