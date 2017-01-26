using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineVoting.Models
{

    public class Election
    {
        //model används när man ska visa info i Delete och ElectionsForUsers Viewn
        [Key]
        public int ElectionId { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(50, ErrorMessage = "The field {0} can contain maximun {1} and minimum {2} characters", MinimumLength = 3)]
        [Display(Name = "Election")]
        public string Description { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [Display(Name = "State")]
        public int StateId { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Description")]
        public string Remarks { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [Display(Name = "DateTime Start")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime DateTimeStart { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [Display(Name = "DateTime End")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime DateTimeEnd { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [Display(Name = "Is for all users?")]
        public bool IsForAllUsers { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [Display(Name = "Enabled blank vote")]
        public bool IsEnableBlankVote { get; set; }

        [Display(Name = "Total votes")]
        public int QuantityVotes { get; set; }

        [Display(Name = "Blank votes")]
        public int QuantityBlankVotes { get; set; }

        [Display(Name = "Winner")]
        public int CandidateWinId { get; set; }

        public virtual State State { get; set; }

        public virtual ICollection<Candidate> Candidates { get; set; }

        public virtual ICollection<ElectionVotingDetail> ElectionVotingDetails { get; set; }








    }
}