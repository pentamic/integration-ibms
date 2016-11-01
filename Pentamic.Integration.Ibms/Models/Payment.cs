using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class Payment
    {
        [DataMember(Name = "cash_payment_list")]
        public List<ReceiptPayment> cash_payment_list { get; set; }
        [DataMember(Name = "card_payment_list")]
        public List<ReceiptPayment> card_payment_list { get; set; }
    }
}