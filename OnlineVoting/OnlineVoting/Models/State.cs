using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineVoting.Models
{
    public class State
    {
        [Key]
        // hämtar och säter status på valet, om den är öpen eller stängde 
        public int StateId { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(50, ErrorMessage = "The field {0} can contain maximum {1} and minimum {2} characters", MinimumLength = 3)]

        // hämtar och säter status string
        [Display(Name = "State")]
        public String Descripcion { get; set; }

        public virtual ICollection<Election> Elections { get; set; }
    }
}