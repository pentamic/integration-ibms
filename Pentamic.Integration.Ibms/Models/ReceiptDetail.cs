using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class ReceiptDetail
    {
        [Key]
        public int Id { get; set; }
        public int ReceiptId { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public int CofferId { get; set; }
        public decimal Price { get; set; }
        public double Quantity { get; set; }
        public string Unit { get; set; }
        public decimal Amount { get; set; }
        public double PercentDiscount { get; set; }
        public decimal MoneyDisCount { get; set; }
        public decimal Total { get; set; }
        public decimal TotalPay { get; set; }
        public int Type { get; set; }
        public int SaleStatus { get; set; }
        public int IBMSCode { get; set; }
        public string LastSync { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}