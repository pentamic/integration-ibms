using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    [DataContract(Name = "product_history")]
    public class tmpInventory
    {
        [Key]
        public int id { get; set; }
        [DataMember(Name = "id")]
        public int IDs { get; set; }

        [DataMember(Name = "product_id")]
        public int ProductId { get; set; }
        [DataMember(Name = "product_type")]
        public int ProductType { get; set; }
        [DataMember(Name = "product_code")]
        public string ProductCode { get; set; }

        [DataMember(Name = "supplier")]
        public tmpSupplier SupplierId { get; set; }

        [DataMember(Name = "code")]
        public string Code { get; set; }

        [DataMember(Name = "create_date")]
        public DateTime CreatedDate { get; set; }

        [DataMember(Name = "quantity")]
        public float Quantity { get; set; }
        [DataMember(Name = "price")]
        public decimal Price { get; set; }
        [DataMember(Name = "amount")]
        public decimal Amount { get; set; }

        [DataMember(Name = "coffer_begin")]
        public Coffer StockFrom { get; set; }
        [DataMember(Name = "coffer_end")]
        public Coffer StockTo { get; set; }
        [DataMember(Name = "location")]
        public Branch Store { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }
        [DataMember(Name = "type")]
        public int Type { get; set; }

        [DataMember(Name = "code_class")]
        public int Code_Class { get; set; }
        
    }
}