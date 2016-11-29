using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }
        public int IDs { get; set; }
        public int ProductId { get; set; }
        public int ProductType { get; set; }

        public int? SupplierId { get; set; }

        public string Code { get; set; }

        public DateTime CreatedDate { get; set; }

        public double Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }

        public int? StockFromId { get; set; }
        public int? StockToId { get; set; }
        public int? StoreId { get; set; }

        public string Description { get; set; }
        public int? Type { get; set; }
        public int Status { get; set; }

        public int? Code_Class { get; set; }
        public string LastSync { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

    }
}