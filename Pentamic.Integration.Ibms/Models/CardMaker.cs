using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    [Table("CardMaker")]
    public class CardMaker
    {
        [Key]
        [Column("Id")]
        public int id { get; set; }
        [Column("Card_Maker_Name")]
        public string card_maker_name { get; set; }
    }
}