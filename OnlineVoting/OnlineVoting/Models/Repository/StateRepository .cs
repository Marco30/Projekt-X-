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
    public class StateRepository : IStateRepository
    {
        private bool _disposed = false;// använda för att se om disposed metoden kallas på 

        private OnlineVotingContext db = new OnlineVotingContext();//egna teabeler

        //user managment ASP.net automat genererade tabeller 
        private ApplicationDbContext userContext;// ASP.net tabeler
        private UserManager<ApplicationUser> userManager;
        private RoleManager<IdentityRole> roleManager;

        public StateRepository()// konstruktor
        {
            //user managment ASP.net  automat genererade tabeller kopling
            userContext = new ApplicationDbContext();
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(userContext));
        }
        //---

        public DbSet<State> GetStateTb()
        {
            var StateTb = db.States;
            return StateTb;
        }

        public void AddState(State state)
        {
            db.States.Add(state);

        }

        public List<State> GetAllState()
        {
            var StateList = db.States.ToList();
            return StateList;
        }

        public State GetStateByStateName(string stateName)
        {
            var state = db.States.Where(s => s.Descripcion == stateName).FirstOrDefault();

            return state;
        }

        public State GetStateById(int stateId)
        {
            var state = db.States.Find(stateId);
            return state;
        }

        public State GetStateByIdNoTracking(int stateId)// används för att inte Entity Framework ska binda data till en state model så att den kan användas utan att påvärka annat data av samma model typ som används i sammas metod 
        {
            var state = db.States.AsNoTracking().Where(p => p.StateId == stateId).FirstOrDefault();
            return state;
        }

        public void UpdateState(State state)//Regdigerar kontakt 
        {
            if (db.Entry(state).State == EntityState.Detached)// kontrolerar om Entity är detached för att attacha den 
            {
                db.States.Attach(state);// attachar data till DataContext 

            }

            db.Entry(state).State = EntityState.Modified;// meddelar att data som lagt till är regdigerar och därmed så kommer det sparras till DB 
            //_entities.SaveChanges();
        }

        public void DeleteState(State state)//tar bort state
        {
            if (db.Entry(state).State == EntityState.Detached)// kontrolerar om Entity är detached för att attacha den 
            {
                db.States.Attach(state);// attachar data till DataContext 
            }

            db.States.Remove(state);// kallar på fukntion i EntityFramework som tar bort state
        }

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
