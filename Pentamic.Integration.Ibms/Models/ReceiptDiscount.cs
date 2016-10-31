using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class ReceiptDiscount
    {
        [Key]
        public int Id { get; set; }
        public int ReceiptId { get; set; }
        public double Percent { get; set; }
        public int DiscountType { get; set; }
        public decimal MoneyDiscount { get; set; }
        public bool Status { get; set; }

        public string LastSync { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
    }
}