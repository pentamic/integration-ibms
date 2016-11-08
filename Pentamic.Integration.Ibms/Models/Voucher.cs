using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class Voucher
    {
        [Key]
        public int Id { get; set; }
        public int IDs { get; set; }
        public string Code { get; set; }
        public double Percent { get; set; }
        public int DiscountType { get; set; }
        public decimal MoneyDiscount { get; set; }
        public string LastSync { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}