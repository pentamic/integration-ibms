using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class Receipt
    {
        [Key]
        public int id { get; set; }
        public int IDs { get; set; }
        public string Code { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CheckinId { get; set; }
        public int? CustomerId { get; set; }
        public int? BranchId { get; set; }
        public decimal TotalPay { get; set; }
        public int BillType { get; set; }
        public bool Status { get; set; }
        public string LastSync { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}