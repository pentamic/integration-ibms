﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    [DataContract(Name = "lsreceipt")]
    public class Bill
    {
        [Key]
        public int id { get; set; }
        [DataMember(Name = "idReceipt")]
        public int IDs { get; set; }
        [DataMember(Name = "receiptCode")]
        public string Code { get; set; }
        [DataMember(Name = "createdDate")]
        public DateTime CreatedDate { get; set; }
        [DataMember(Name = "totalPay")]
        public decimal TotalPay { get; set; }
        [DataMember(Name = "bill_type")]
        public int BillType { get; set; }
        [DataMember(Name = "status")]
        public bool Status { get; set; }

        [DataMember(Name = "checkin")]
        public CheckIn checkin { get; set; }
        [DataMember(Name = "customer")]
        public Customer customer { get; set; }
        [DataMember(Name = "lstprd")]
        public List<Bill_Product> lstprd { get; set; }
        [DataMember(Name = "lstDiscount")]
        public List<Discount> lstDiscount { get; set; }
        [DataMember(Name = "lst_sale_staff")]
        public List<Bill_Sale> lst_sale_staff { get; set; }
        [DataMember(Name = "payment")]
        public Payment payment { get; set; }
        [DataMember(Name = "branch")]
        public Branch branch { get; set; }
        [DataMember(Name = "paymentDeposit")]
        public Payment paymentDeposit { get; set; }
    }
}