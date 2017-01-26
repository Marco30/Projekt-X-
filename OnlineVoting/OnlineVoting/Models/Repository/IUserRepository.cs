using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OnlineVoting.Models.Repository
{
    
    public interface IUserRepository : IDisposable// min interface, IDisposable används för att rensa upp ohanterade resurser, Ohanterade resurser är till exempel, öppna filer, Öppna nätverksanslutningar, Datorstyrda minne
    {
        //Marco, bra att repetera 
        //klasser som implementerar interfacen funktionaliteten kan använda interfacens egenskaper, metoder och/eller händelser. 
        //interface i C# är ett sätt att komma runt bristen på multipelt arv i C #, vilket innebär att man inte inte kan ärva från flera klasser C# men du kan inplämnetar flera gränssnitt istället i C#.
        void Add(User user);
        void Save();
        void AdminCreatesUserInASPdb(UserCreateEditView userView);
        List<User> GetUserListByFirstName();
        User GetUserByUserEmail(string UserEmail);
        User GetUserByUserId(int UserID);
        List<User> GetListOfAllUser();
        List<User> GetListOfAllUserByFirstNameAndLastName(string FirstNameText, string LastNameText);
        List<User> GetListUserByFirstName(string FirstNameText);
        List<string> AutocompleteListByFirstNameAndLastName(string FirstNameText, string LastNameText);
        List<string> AutocompleteListByFirstName(string FirstNameText);

        ApplicationUser GetUserByUserEmailFromASPdb(string UserEmail);
        bool GetIfUserIsAdminFromASPdb(string usersID);
        void EnableAdmin(String UserID);
        void DisableAdmin(String UserID);
        void Delete(User user);
        void Update(User user);

    }
}
