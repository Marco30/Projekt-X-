using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace OnlineVoting.Models
{
    public class ElectionRankView
    {
        // modelen används för att visa resultat info om valet och lista på vem som van och hur många röster alla deltagare fåt 
        // används av ShowResults viewn 
        [Required(ErrorMessage = "The field {0} is required")]
        public int ElectionId { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        public String Electionname { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        public String State { get; set; }

        [Display(Name = "All Candidates")]
        [Required(ErrorMessage = "The field {0} is required")]
        public String Candidate { get; set; }

        [Display(Name = "Number Of Votes")]
        [Required(ErrorMessage = "The field {0} is required")]
        public int QuantityVotes { get; set; }

    }
}