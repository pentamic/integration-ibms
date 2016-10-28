using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class CardPayment
    {
        [Key]
        public int Id { get; set; }
        [DataMember(Name = "id")]
        public int IDs { get; set; }
        [DataMember(Name = "bank_card_type")]
        public string BankCardType { get; set; }
        [DataMember(Name = "amount")]
        public decimal Amount { get; set; }
        [DataMember(Name = "total")]
        public decimal Total { get; set; }
        [DataMember(Name = "currency_code")]
        public string CurrencyCode { get; set; }
        [DataMember(Name = "exchange_rate")]
        public double ExchangeRate { get; set; }
        [DataMember(Name = "status")]
        public bool Status { get; set; }
        [DataMember(Name = "bank_account")]
        public Bank bank_account { get; set; }
    }
}