using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace OnlineVoting.Models
{
    public class OnlineVotingContext : DbContext
    {
        //den här klassen koplar systemet till databasen och skapar också vår databas om den inte finns med entity framework
        public OnlineVotingContext()
            : base("DefaultConnection")
        {

        }

        //Denna metod används för att "inaktivera" cascade radera läge i DB
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }

        public DbSet<State> States { get; set; }

        public DbSet<Election> Elections { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Candidate> Candidates { get; set; }

        public DbSet<ElectionVotingDetail> ElectionVotingDetails { get; set; }


    }
}