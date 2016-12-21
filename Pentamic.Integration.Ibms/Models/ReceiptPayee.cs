using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    [DataContract(Name = "payee_list")]
    public class ReceiptPayee
    {
        [Key]
        public int Id { get; set; }
        [DataMember(Name = "id")]
        public int PayeeId { get; set; }
        [DataMember(Name = "payee_name")]
        public string PayeeName { get; set; }
        [DataMember(Name = "payee_commission")]
        public double Commission { get; set; }
        [DataMember(Name = "payee_money")]
        public decimal Money { get; set; }
        [DataMember(Name = "payee_type")]
        public int PayeeType { get; set; }
        public int? CheckInId { get; set; }
        public int? ReceiptId { get; set; }

        public string LastSync { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}