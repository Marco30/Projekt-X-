using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace OnlineVoting.Models
{
    public class UserCreateEditView
    {
        // användas vi skapande av användare 
        // används av creat och edit view 
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

        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(20, ErrorMessage = "The field {0} can contain maximum {1} and minimum {2} characters", MinimumLength = 7)]
        public String Phone { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(100, ErrorMessage = "The field {0} can contain maximum {1} and minimum {2} characters", MinimumLength = 10)]
        public String Adress { get; set; }

       [Required(ErrorMessage = "img {0} is required")]
        public HttpPostedFileBase Photo { get; set; }

    }
}