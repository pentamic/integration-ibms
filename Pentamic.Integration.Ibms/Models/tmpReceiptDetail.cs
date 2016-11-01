using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    [DataContract(Name = "lstprd")]
    public class tmpReceiptDetail
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }
        [DataMember(Name = "productCode")]
        public string ProductCode { get; set; }
        [DataMember(Name = "productName")]
        public string ProductName { get; set; }
        [DataMember(Name = "price")]
        public decimal Price { get; set; }
        [DataMember(Name = "Quantity")]
        public double Quantity { get; set; }
        [DataMember(Name = "unit")]
        public string Unit { get; set; }
        [DataMember(Name = "amount")]
        public decimal Amount { get; set; }
        [DataMember(Name = "percentDiscount")]
        public double PercentDiscount { get; set; }
        [DataMember(Name = "moneyDiscount")]
        public decimal MoneyDisCount { get; set; }
        [DataMember(Name = "total")]
        public decimal Total { get; set; }

        [DataMember(Name = "totalPay")]
        public decimal TotalPay { get; set; }
        [DataMember(Name = "coffer")]
        public Coffer coffer { get; set; }
        [DataMember(Name = "type")]
        public int Type { get; set; }
        [DataMember(Name = "saleStatus")]
        public int SaleStatus { get; set; }
        [DataMember(Name = "ibms_code")]
        public int IBMSCode { get; set; }
    }
}