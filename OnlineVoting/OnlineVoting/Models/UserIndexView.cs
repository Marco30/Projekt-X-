using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace OnlineVoting.Models
{

    [NotMapped]
    public class UserIndexView : User
    {
        //admin
        [Display(Name = "Is Admin?")]
        public bool IsAdmin { get; set; }

    }
}