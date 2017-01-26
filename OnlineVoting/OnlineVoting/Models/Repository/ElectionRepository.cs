using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using OnlineVoting.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.SqlClient;
using System.Configuration;

namespace OnlineVoting.Models.Repository
{
    public class ElectionRepository : IElectionRepository
    {
        private bool _disposed = false;// använda för att se om disposed metoden kallas på 

        private OnlineVotingContext db = new OnlineVotingContext();//egna teabeler

        //user managment ASP.net automat genererade tabeller 
        private ApplicationDbContext userContext;// ASP.net tabeler
        private UserManager<ApplicationUser> userManager;
        private RoleManager<IdentityRole> roleManager;

        public ElectionRepository()// konstruktor
        {
            //user managment ASP.net  automat genererade tabeller kopling
            userContext = new ApplicationDbContext();
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(userContext));
        }
        //---
        public DbContextTransaction Transaction()// kontakt med DB och transaction anbvänds i MVC för att kunna läga till data i flera tabeler 
        {
            var transaction = db.Database.BeginTransaction();

            return transaction;
        }


        public Election GetElectionByIdNoTracking(int ElectionId)// används för att inte Entity Framework ska binda data till en state model så att den kan användas utan att påvärka annat data av samma model typ som används i sammas metod 
        {
            var Election = db.Elections.AsNoTracking().Where(p => p.ElectionId == ElectionId).FirstOrDefault();
            return Election;
        }
       

        public void VotingDetailAdd(ElectionVotingDetail votingDetail)// läger till votingDetails til DB
        {
            db.ElectionVotingDetails.Add(votingDetail);
        }

        public Election GetElectionById(int ElectionID)// hämtar val ut i från ID 
        {

            var Election = db.Elections.Find(ElectionID);

            return Election;
        }

        public List<Election> GetListOfAllElectionsById(int id)
        {
            var ElectionsList = db.Elections.Where(x => x.ElectionId == id).Include(v => v.State).ToList();
            return ElectionsList;

        }

        public void AddElection(Election voting)// hämtar val ut i från ID 
        {

            db.Elections.Add(voting);
        }
        //Candidate 

        public Candidate GetListOfAllElectionCandidates(int ElectionID)// hämtar hel kandidat lista ut i från ID 
        {

            var Candidate = db.Candidates.Where(c => c.ElectionId == ElectionID).OrderByDescending(c => c.QuantityVotes).FirstOrDefault();

            return Candidate;
        }

        public Candidate GetCandidateById(int ElectionID)// hämtar kandidat ut i från id 
        {
            var Candidate = db.Candidates.Find(ElectionID);

            return Candidate;
        }

        public Candidate GetCandidateByElectionIdAndUserId(int ElectionID, int UserId)// hämtar kandidat ut i från id 
        {
            var Candidate = db.Candidates
                .Where(c => c.ElectionId == ElectionID &&
                c.UserId == UserId)
                .FirstOrDefault();
                          

            return Candidate;
        }

        public void AddCandidate(Candidate candidate)
        {
            db.Candidates.Add(candidate);
        }

        public void UpdateCandidate(Candidate candidate)//Regdigerar kandidat tabelen 
        {
            if (db.Entry(candidate).State == EntityState.Detached)// kontrolerar om Entity är detached för att attacha den 
            {
                db.Candidates.Attach(candidate);// attachar data till DataContext 

            }

            db.Entry(candidate).State = EntityState.Modified;// meddelar att data som lagt till är regdigerar och därmed så kommer det sparras till DB 
            //_entities.SaveChanges();
        }

        public void DeleteCandidate(Candidate candidate)
        {
            db.Candidates.Remove(candidate);
        }

        //---
        public List<Election> GetListOfElectionIfOpen(State state)// hämtar valen som är öpan, där röstnings tiden gäller från DB 
        {
            var votings = db.Elections
                .Where(v => v.StateId == state.StateId &&
                v.DateTimeStart <= DateTime.Now &&
                v.DateTimeEnd >= DateTime.Now)
                .Include(v => v.Candidates)
                .Include(v => v.State)
                .ToList();

            return votings;
        }

        public ElectionVotingDetail GetIfUserAlreadyVotedInElection(int VotingId, int UserId)// använda för att kontrollera om en användare redan röstat 
        {
            var votingDetail = db.ElectionVotingDetails.
                Where(vd => vd.ElectionId == VotingId &&
                vd.UserId == UserId).FirstOrDefault();

            return votingDetail;
        }


        public List<ElectionRankView> ShowResultsOfElectionById(int ElectionID)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            //@ from not concatenate
            var sql = @"SELECT  Elections.ElectionId, Elections.Description AS Election, States.Descripcion AS State, 
                                Users.FirstName + ' ' + Users.LastName AS Candidate, Candidates.QuantityVotes
                         FROM   Candidates INNER JOIN
                                Users ON Candidates.UserId = Users.UserId INNER JOIN
                                Elections ON Candidates.ElectionId = Elections.ElectionId INNER JOIN
                                States ON Elections.StateId = States.StateId
                          WHERE Elections.ElectionId =" + ElectionID + " ORDER BY Candidates.QuantityVotes DESC";

            // Skapar och initierar ett anslutningsobjekt.
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // Skapar ett List-objekt med 100 platser.
                    var Rank = new List<ElectionRankView>(100);

                    // Skapar och initierar ett SqlCommand-objekt som används till att exekveras specifierad lagrad procedur.
                    var command = new SqlCommand(sql, connection);


                    // Öppnar anslutningen till databasen.
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        // Tar reda på vilket index de olika kolumnerna har.
                        var ElectionId = reader.GetOrdinal("ElectionId");
                        var Electionname = reader.GetOrdinal("Election");
                        var State = reader.GetOrdinal("State");
                        var Candidate = reader.GetOrdinal("Candidate");
                        var QuantityVotest = reader.GetOrdinal("QuantityVotes");


                        // Så länge som det finns poster att läsa returnerar Read true och läsningen fortsätter.
                        while (reader.Read())
                        {
                            // Hämtar ut datat för en post.
                            Rank.Add(new ElectionRankView
                            {
                                ElectionId = reader.GetInt32(ElectionId),
                                Electionname = reader.GetString(Electionname),
                                State = reader.GetString(State),
                                Candidate = reader.GetString(Candidate),
                                QuantityVotes = reader.GetInt32(QuantityVotest)

                            });
                        }
                    }

                    // Avallokerar minne som inte används och skickar tillbaks listan med aktiviteter.
                    Rank.TrimExcess();
                    return Rank;
                }
                catch
                {
                    throw new ApplicationException("An error occured while getting members from the database.");
                }
            }
        }

        public List<Election> GetElectionByStateId(State state)
        {
            var votings = db.Elections.Where(v => v.StateId == state.StateId).Include(v => v.State).ToList();              
            return votings;
        }

        public List<Election> GetListOfAllElections()
        {
            var votings = db.Elections.Include(v => v.State).ToList();  
            return votings;

        }

        public List<Election> GetElectionByName(string SearchText)// söker val namn man sökt på i DB för att visas i viewn 
        {
            var votings = db.Elections.Where(x => x.Description.StartsWith(SearchText)).ToList();

            return votings;
        }

        public List<Election> GetElectionByNameAndStateId(string SearchText, State state)
        {
            var votings = db.Elections.Where(x => x.Description.StartsWith(SearchText) & x.StateId == state.StateId).ToList();

            return votings;
        }

        public List<string> GetElectionByNameForAutocomplete(string SearchText)
        {
            var ElectionList = db.Elections.Where(x => x.Description.StartsWith(SearchText)).Select(y => y.Description).ToList();

            return ElectionList;
        }

        public List<string> GetElectionByNameAndStateIdForAutocomplete(string SearchText, State state)
        {
            var votings = db.Elections.Where(x => x.Description.StartsWith(SearchText) & x.StateId == state.StateId).Select(y => y.Description).ToList();

            return votings;
        }

        public List<Election> GetElectionByYearandMonths(int year, int MonthsNum)
        {
            var ElectionList = db.Elections.Where(x => x.DateTimeStart.Year == year & x.DateTimeStart.Month == MonthsNum).ToList();

            return ElectionList;
        }

        public List<Election> GetElectionByYear(int year)
        {
            var ElectionList = db.Elections.Where(x => x.DateTimeStart.Year == year).ToList();

            return ElectionList;
        }

        public List<Election> GetElectionByMonths(int MonthsNum)
        {
            var ElectionList = db.Elections.Where(x => x.DateTimeStart.Month == MonthsNum).ToList();

            return ElectionList;
        }

        public List<Election> GetElectionByYearAndStateId(int year, State state)
        {
            var votings = db.Elections.Where(x => x.DateTimeStart.Year == year & x.StateId == state.StateId).ToList();
                         
            return votings;
        }

        public List<Election> GetElectionByMonthsAndStateId(int MonthsNum, State state)
        {
            var votings = db.Elections.Where(x => x.DateTimeStart.Month == MonthsNum & x.StateId == state.StateId).ToList();

            return votings;
        }

        public List<Election> GetElectionByYearMonthsAndStateId(int year, int MonthsNum, State state)
        {
            var votings = db.Elections.Where(x => x.DateTimeStart.Year == year & x.DateTimeStart.Month == MonthsNum & x.StateId == state.StateId).ToList();
          
            return votings;
        }

        public void UpdateElection(Election voting)//Regdigerar kontakt 
        {
            if (db.Entry(voting).State == EntityState.Detached)// kontrolerar om Entity är detached för att attacha den 
            {
                db.Elections.Attach(voting);// attachar data till DataContext 

            }

            db.Entry(voting).State = EntityState.Modified;// meddelar att data som lagt till är regdigerar och därmed så kommer det sparras till DB 
            //_entities.SaveChanges();
        }

        public void DeleteElection(Election Election)
        {
            if (db.Entry(Election).State == EntityState.Detached)// kontrolerar om Entity är detached för att attacha den 
            {
                db.Elections.Attach(Election);// attachar data till DataContext 
            }

            db.Elections.Remove(Election);
        }

        //---


        public void Dispose(bool disposing)
        {
            if (!_disposed)// kontrolerare om Dispos redan körts 
            {
                if (disposing)
                {
                    db.Dispose();// används för att frigöra Ohanterade resurser 
                }
                _disposed = true;
            }

        }

        public void Dispose()// används för att frigöra Ohanterade resurser 
        {
            Dispose(true);// används för att frigöra Ohanterade resurser 
            GC.SuppressFinalize(this);// vi har redan rensat resurser så man använder GC så att den inte kallas igen 
        }

        public void Save()// spara
        {
            db.SaveChanges();// kallar på fukntion i EntityFramework som sparar ändringar till DB 
        }


    }
}
