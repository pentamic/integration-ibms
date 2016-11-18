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
    public class tmpCheckIn
    {
        [Key]
        public int Id { get; set; }
        [DataMember(Name = "id")]
        public int IDs { get; set; }
        [DataMember(Name = "checkin_code")]
        public string CheckInCode { get; set; }
        [DataMember(Name = "checkin_date")]
        public DateTime? CreatedDate { get; set; }
        [DataMember(Name = "fee_port")]
        public decimal FeePort { get; set; }
        [DataMember(Name = "status")]
        public int Status { get; set; }

        [DataMember(Name = "card_type")]
        public CardType card_type { get; set; }

        [DataMember(Name = "card_maker")]
        public CardMaker card_maker { get; set; }

        [DataMember(Name = "partner_group")]
        public tmpPartnerGroup partner_group { get; set; }

        [DataMember(Name = "driver")]
        public Driver driver { get; set; }

        [DataMember(Name = "tour_guide")]
        public TourGuide tour_guide { get; set; }

        [DataMember(Name = "visitor_type")]
        public VisitorType visitor_type { get; set; }

        [DataMember(Name = "car_number_list")]
        public List<CarNumber> car_number_list { get; set; }

        [DataMember(Name = "payee_list")]
        public List<Payee> payee_list { get; set; }

        [DataMember(Name = "checkin_info_list")]
        public List<CheckInDetail> checkin_info_list { get; set; }

        [DataMember(Name = "location")]
        public Branch location { get; set; }
        

    }
}