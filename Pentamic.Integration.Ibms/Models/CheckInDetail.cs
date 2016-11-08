using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    [DataContract(Name = "checkin_info_list")]
    public class CheckInDetail
    {
        [Key]
        public int Id { get; set; }
        [DataMember(Name = "number_baby")]
        public int NumberBaby { get; set; }

        [DataMember(Name = "number_child")]
        public int NumberChild { get; set; }

        [DataMember(Name = "number_adult")]
        public int NumberAdult { get; set; }

        [DataMember(Name = "nationality")]
        public string Nationality { get; set; }

        [DataMember(Name = "nationality_id")]
        public int NationalityId { get; set; }

        public int CheckinId { get; set; }
        public string LastSync { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}