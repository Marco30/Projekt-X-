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
    
    public interface IElectionRepository : IDisposable// min interface, IDisposable används för att rensa upp ohanterade resurser, Ohanterade resurser är till exempel, öppna filer, Öppna nätverksanslutningar, Datorstyrda minne
    {
        //Marco, bra att repetera 
        //klasser som implementerar interfacen funktionaliteten kan använda interfacens egenskaper, metoder och/eller händelser. 
        //interface i C# är ett sätt att komma runt bristen på multipelt arv i C #, vilket innebär att man inte inte kan ärva från flera klasser C# men du kan inplämnetar flera gränssnitt istället i C#.

        void Save();
        DbContextTransaction Transaction();
        void VotingDetailAdd(ElectionVotingDetail votingDetail);

        Election GetElectionById(int ElectionID);
        Candidate GetListOfAllElectionCandidates(int ElectionID);
        Candidate GetCandidateById(int ElectionID);
        void DeleteCandidate(Candidate candidate);
        void UpdateCandidate(Candidate candidate);
        void AddCandidate(Candidate candidate);
        Candidate GetCandidateByElectionIdAndUserId(int ElectionID, int UserId);

        List<ElectionRankView> ShowResultsOfElectionById(int ElectionID);
        Election GetElectionByIdNoTracking(int ElectionId);
        void AddElection(Election voting);
        List<Election> GetListOfAllElectionsById(int id);
        void DeleteElection(Election Election);
        List<Election> GetListOfAllElections();
        List<string> GetElectionByNameForAutocomplete(string SearchText);
        List<Election> GetElectionByName(string SearchText);
        List<Election> GetListOfElectionIfOpen(State state);
        ElectionVotingDetail GetIfUserAlreadyVotedInElection(int VotingId, int UserId);
        List<Election> GetElectionByStateId(State state);
        List<Election> GetElectionByYear(int year);
        List<Election> GetElectionByMonths(int MonthsNum);
        List<Election> GetElectionByYearandMonths(int year, int MonthsNum);
        List<Election> GetElectionByNameAndStateId(string SearchText, State state);
        List<string> GetElectionByNameAndStateIdForAutocomplete(string SearchText, State state);
        List<Election> GetElectionByYearAndStateId(int year, State state);
        List<Election> GetElectionByMonthsAndStateId(int MonthsNum, State state);
        List<Election> GetElectionByYearMonthsAndStateId(int year, int MonthsNum, State state);

        void UpdateElection(Election voting);
        
    }
}
