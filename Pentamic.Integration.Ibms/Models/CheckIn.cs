using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class CheckIn
    {
        [Key]
        public int id { get; set; }
        public string checkin_code { get; set; }
        public double fee_port { get; set; }
        public bool status { get; set; }
        //public int card_type_id { get; set; }
        public CardType card_type { get; set; }
        //public int card_maker_id { get; set; }
        public CardMaker card_maker { get; set; }
        
        //public int partner_group_id { get; set; }
        public PartnerGroup partner_group { get; set; }
        //public int driver_id { get; set; }
        public Driver driver { get; set; }
        //public int tour_guide_id { get; set; }
        public TourGuide tour_guide { get; set; }
        //public int visitor_type_id { get; set; }
        public VisitorType visitor_type { get; set; }
        public List<CarNumber> car_number_list { get; set; }
        public List<Payee> payee_list { get; set; }
        public List<CheckIn_Info> checkin_info_list { get; set; }
        //public int location_id { get; set; }
        public Location location { get; set; }

    }
}