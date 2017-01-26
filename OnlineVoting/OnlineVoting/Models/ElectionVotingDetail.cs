using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVoting.Models
{
    public class ElectionVotingDetail
    {
        // modelr för att visa detaljerade info om valet i Detail view
        [Key]
        public int ElectionVotingDetailId { get; set; }

        public DateTime DateTime { get; set; }

        public int ElectionId { get; set; }

        public int UserId { get; set; }

        public int CandidateId { get; set; }

        public virtual Election Election { get; set; }

        public virtual Candidate Candidate { get; set; }
    }
}
