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
using System.Security.Claims;

namespace OnlineVoting.Models.Repository
{
    
    public interface IAccountRepository : IDisposable// min interface, IDisposable används för att rensa upp ohanterade resurser, Ohanterade resurser är till exempel, öppna filer, Öppna nätverksanslutningar, Datorstyrda minne
    {
        //Marco, bra att repetera 
        //klasser som implementerar interfacen funktionaliteten kan använda interfacens egenskaper, metoder och/eller händelser. 
        //interface i C# är ett sätt att komma runt bristen på multipelt arv i C #, vilket innebär att man inte inte kan ärva från flera klasser C# men du kan inplämnetar flera gränssnitt istället i C#.


        Task<ApplicationUser> GetUserByEmailAndPassword(string email, string password);
        Task<ClaimsIdentity> ControlIdentityInASPdb(ApplicationUser user);
        ApplicationUser CreatesUserInASPdb(AccountRegisterUserView userView);
        Task<IdentityResult> RemoveLoginInASPdb(string UserId, string loginProvider, string providerKey);
        Task<IdentityResult> ChangePasswordForUserInASPdb(string UserId, string OldPassword, string NewPassword);
        Task<IdentityResult> AddPasswordForUserToASPdb(string UserId, string NewPassword);
        IList<UserLoginInfo> GetUsersLoginsInfoFromASPdb(String UserId);
        ApplicationUser GetUserByIdInASPdb(String UserId);
        Task<bool> ControlIfUserIsLockedOutFromASPdb(string UserID);

        Task<bool> Update(ApplicationUser user);

        void Save();


    }
}
