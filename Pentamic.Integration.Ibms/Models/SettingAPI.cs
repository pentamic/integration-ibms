using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pentamic.Integration.Ibms.Models
{
    public class SettingAPI
    {
        [Key]
        public int Id { get; set; }
        public bool Enable { get; set; }
    }
}