using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class BalanceStock
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public DateTime CreateDate { get; set; }
        public double Quantity { get; set; }
        public int BranchId { get; set; }
        public int CofferId { get; set; }
        public int Type { get; set; }
        public bool Status { get; set; }
        public string LastSync { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}