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

    [Serializable]
    public class User
    {
        // används på alla paltser i systmet där man ska presentera info om användaren 
        [Key]
        public int UserId { get; set; }

        [Display(Name = "E-Mail")]
        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(100, ErrorMessage = "The field {0} can contain maximum {1} and minimum {2} characters", MinimumLength = 7)]
        [DataType(DataType.EmailAddress)]
        [Index("UserNameIndex", IsUnique = true)]
        public String UserName { get; set; }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(50, ErrorMessage = "The field {0} can contain maximum {1} and minimum {2} characters", MinimumLength = 2)]
        public String FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(50, ErrorMessage = "The field {0} can contain maximum {1} and minimum {2} characters", MinimumLength = 2)]
        public String LastName { get; set; }

        [Display(Name = "Full Name")]

        public String FullName { get { return string.Format(" {0} {1} ", this.FirstName, this.LastName); } }

        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(20, ErrorMessage = "The field {0} can contain maximum {1} and minimum {2} characters", MinimumLength = 7)]
        public String Phone { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(100, ErrorMessage = "The field {0} can contain maximum {1} and minimum {2} characters", MinimumLength = 10)]
        public String Adress { get; set; }

        [StringLength(200, ErrorMessage = "The field {0} can contain maximum {1} and minimum {2} characters", MinimumLength = 5)]
        [DataType(DataType.ImageUrl)]
        public String Photo { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public virtual ICollection<Candidate> Candidates { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public virtual ICollection<ElectionVotingDetail> ElectionVotingDetails { get; set; }
    }
}