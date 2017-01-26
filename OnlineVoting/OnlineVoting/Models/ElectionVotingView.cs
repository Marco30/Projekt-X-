using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVoting.Models
{


    [NotMapped]
    public class ElectionVotingView : Election
    {
        //lista på kandidater model används i vote view   
        public List<Candidate> MyCandidate { get; set; }
    }


}
