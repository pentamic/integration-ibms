using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    [DataContract(Name = "checkin_list")]
    public class tmpCheckIn_receipt
    {
        [Key]
        public int Id { get; set; }
        [DataMember(Name = "id")]
        public int IDs { get; set; }
       
        [DataMember(Name = "payee_list")]
        public List<ReceiptPayee> payee_list { get; set; }
        
    }
}