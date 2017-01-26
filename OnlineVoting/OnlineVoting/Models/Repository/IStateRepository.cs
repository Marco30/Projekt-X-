using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data;
using System.Data.Entity;

namespace OnlineVoting.Models.Repository
{
    
    public interface IStateRepository : IDisposable// min interface, IDisposable används för att rensa upp ohanterade resurser, Ohanterade resurser är till exempel, öppna filer, Öppna nätverksanslutningar, Datorstyrda minne
    {
        //Marco, bra att repetera 
        //klasser som implementerar interfacen funktionaliteten kan använda interfacens egenskaper, metoder och/eller händelser. 
        //interface i C# är ett sätt att komma runt bristen på multipelt arv i C #, vilket innebär att man inte inte kan ärva från flera klasser C# men du kan inplämnetar flera gränssnitt istället i C#.
        void AddState(State state);
        State GetStateByIdNoTracking(int stateId);
        List<State> GetAllState();
        DbSet<State> GetStateTb();
        State GetStateByStateName(string stateName);
        State GetStateById(int stateId);
       void UpdateState(State state);
        void DeleteState(State state);
        void Save();


    }
}
