using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    [DataContract(Name = "lstDiscount")]
    public class Discount
    {
        [Key]
        public int Id { get; set; }
        [DataMember(Name = "id")]
        public int VoucherId { get; set; }
        [DataMember(Name = "code")]
        public string  VoucherName{ get; set; }
        [DataMember(Name = "percent")]
        public double Percent { get; set; }
        [DataMember(Name = "discount_type")]
        public int DiscountType { get; set; }
        [DataMember(Name = "money_discount")]
        public decimal MoneyDiscount { get; set; }
        [DataMember(Name = "status")]
        public bool Status { get; set; }

        public string LastSync { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
    }
}