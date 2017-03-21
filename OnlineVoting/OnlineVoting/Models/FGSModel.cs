using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace OnlineVoting.Models
{
    public class FGSModel
    {
        [Key]
        public int FGSId { get; set; }
        public String Node { get; set; }
        public String Attribute { get; set; }
        public String NewAttribute { get; set; }
        public String NewAttributeValue { get; set; }
        public String Line { get; set; }
        public String Message { get; set; }
    }
}