using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    [DataContract(Name = "lst_sale_staff")]
    public class ReceiptSalesStaff
    {
        [Key]
        public int Id { get; set; }
        [DataMember(Name = "id")]
        public int SalemanId { get; set; }
        [DataMember(Name = "full_name")]
        public string SalemanName { get; set; }
        [DataMember(Name = "pecent")]
        public double Percent { get; set; }
        [DataMember(Name = "money")]
        public decimal Money { get; set; }
        [DataMember(Name = "status")]
        public bool Status { get; set; }

        public int ReceiptId { get; set; }
        public string LastSync { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
    }
}