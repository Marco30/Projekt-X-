using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace OnlineVoting.Models
{
    public class MonthsList 
    {

        //[Required(ErrorMessage = "The field {0} is required")]
        public int MonthsID { get; set; }

        //[Required(ErrorMessage = "The field {0} is required")]
        public int Months { get; set; }


    }



}