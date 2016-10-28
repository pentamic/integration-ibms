using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    [DataContract(Name = "payment")]
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        [DataMember(Name = "cash_payment_list")]
        public List<CashPayment> cash_payment_list { get; set; }
        [DataMember(Name = "card_payment_list")]
        public List<CardPayment> card_payment_list { get; set; }
    }
}