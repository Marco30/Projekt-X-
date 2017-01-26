using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace OnlineVoting.Models
{
    [NotMapped]
    public class UserSettingsView : User
    {
        [Display(Name = "New photo")]
        //model som används för att spara bild sökvägen 
        public HttpPostedFileBase NewPhoto { get; set; }

    }
}