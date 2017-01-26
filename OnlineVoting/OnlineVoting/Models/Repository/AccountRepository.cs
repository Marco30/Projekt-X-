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
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using System.Security.Claims;

namespace OnlineVoting.Models.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private bool _disposed = false;// använda för att se om disposed metoden kallas på 

        private OnlineVotingContext db = new OnlineVotingContext();//egna teabeler

        //user managment ASP.net automat genererade tabeller 
        private ApplicationDbContext userContext;// ASP.net tabeler
        private UserManager<ApplicationUser> userManager;
        private RoleManager<IdentityRole> roleManager;

        public AccountRepository()// konstruktor
        {
            //user managment ASP.net  automat genererade tabeller kopling
            userContext = new ApplicationDbContext();
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(userContext));
        }
        //---

        public async Task<ApplicationUser> GetUserByEmailAndPassword(string email, string password)// hämtar användare från ASP.net db asicront med hjälp av email och lösenord 
        {
            var user = await userManager.FindAsync(email, password);
            return user;
        }

        public async Task<ClaimsIdentity> ControlIdentityInASPdb(ApplicationUser user)// kontolerare om användare finns i ASP.net db asicront 
        {
             var identity = await userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            return identity;
        }

        public async Task<IdentityResult> RemoveLoginInASPdb(string UserId, string loginProvider, string providerKey)//tar bort login 
        {
            var identity = await userManager.RemoveLoginAsync(UserId, new UserLoginInfo(loginProvider, providerKey));
            return identity;
        }

        public async Task<IdentityResult> ChangePasswordForUserInASPdb(string UserId, string OldPassword, string NewPassword)// ändrar lösenord 
        {
            var identity = await userManager.ChangePasswordAsync(UserId,OldPassword, NewPassword);

            return identity;
        }

        public async Task<IdentityResult> AddPasswordForUserToASPdb(string UserId, string NewPassword)// läger till ny lösenord 
        {
            var identity = await userManager.AddPasswordAsync(UserId, NewPassword);

            return identity;
        }

        public ApplicationUser CreatesUserInASPdb(AccountRegisterUserView userView)// skpar ny anändare i ASP.net DB
        {

            // Create User role
            string roleName = "User";// ändra texten User till Admin för att skapa en Admin användare 

            // Check to see if Role Exists, if not create it
            if (!roleManager.RoleExists(roleName))
            {
                roleManager.Create(new IdentityRole(roleName));
            }

            // Create the ASP NET User
            var userASP = new ApplicationUser
            {
                UserName = userView.UserName,
                Email = userView.UserName,
                PhoneNumber = userView.Phone,
            };

            userManager.Create(userASP, userView.Password);

            // Add user to role
            userASP = userManager.FindByName(userView.UserName);
            userManager.AddToRole(userASP.Id, "User");// ändra texten User till Admin för att skapa en Admin användare 
            return userASP;
        }

        //ControlIfUserIsLockedOutFromASPdb användas inte just nu i appen 
        public async Task<bool> ControlIfUserIsLockedOutFromASPdb(string UserID)// gär os true om användare är blokerad från systemet
        {

            var userASP = await userManager.IsLockedOutAsync(UserID);

            return userASP;
        }

        public IList<UserLoginInfo> GetUsersLoginsInfoFromASPdb(String UserId)// hämtar logininfo på användare med hjälp av anändare id
        {
            var userList = userManager.GetLogins(UserId);

            return userList;
        }

        public ApplicationUser GetUserByIdInASPdb(String UserId)// hämtar anändare med hjälp av ID från ASP.net DB
        {
            var user = userManager.FindById(UserId);

            return user;
        }

        //-----Updat använda för Email verifiering
        public async Task<bool> Update(ApplicationUser user)//Regdigerar kontakt i ASP.net DB
        {
            try
            {
                await userManager.UpdateAsync(user);

                return true;
            }
            catch
            {
                return false;
            }
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
