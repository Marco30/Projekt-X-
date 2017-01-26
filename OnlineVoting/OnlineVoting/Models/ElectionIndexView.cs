using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVoting.Models
{
    [NotMapped]
    public class ElectionIndexView : Election
    {
        // används av index,_ElectionAfterEdit,_ElectionInfo,_UserResultsInfo och Results View ärver från Election model
        public User Winner { get; set; }
    }
}
