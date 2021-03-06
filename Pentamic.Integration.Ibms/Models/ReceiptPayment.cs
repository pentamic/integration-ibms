﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    [DataContract(Name = "card_payment_list")]
    public class ReceiptPayment
    {
        [Key]
        public int Id { get; set; }
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
        public int Status { get; set; }
        [DataMember(Name = "bank_account")]
        public BankAccount BankAccount { get; set; }
        public int? PaymentType { get; set; }
        public int ReceiptId { get; set; }
        public int? BankId { get; set; }
        public bool IsDeposit { get; set; }
        [DataMember(Name = "create_date")]
        public DateTime? DateDeposit { get; set; }
        public string LastSync { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}