using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using OnlineVoting.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.SqlClient;

namespace OnlineVoting.Models.Repository
{
    public class UserRepository : IUserRepository
    {
        private bool _disposed = false;// använda för att se om disposed metoden kallas på 

        private OnlineVotingContext db = new OnlineVotingContext();//egna teabeler

        //user managment ASP.net automat genererade tabeller 
        private ApplicationDbContext userContext;// ASP.net tabeler
        private UserManager<ApplicationUser> userManager;
        private RoleManager<IdentityRole> roleManager;

        public UserRepository()// konstruktor
        {
            //user managment ASP.net  automat genererade tabeller kopling
            userContext = new ApplicationDbContext();
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(userContext));
        }

        public void Add(User User)//läger till nu kontakt 
        {

            db.Users.Add(User);

        }

        public void AdminCreatesUserInASPdb(UserCreateEditView userView)// skapar en ny användare i ASP.net automat genererad DB 
        {

            string roleName = "User";// användarens rol i systemet  
            // string roleName = "Admin"; // skapar en admin 

            if (!roleManager.RoleExists(roleName))// kontrolerar om rolen existerar om den inte gör det så skapas den 
            {

                roleManager.Create(new IdentityRole(roleName));

            }



            var userASP = new ApplicationUser // skapar ASPNetUser

            {

                UserName = userView.UserName,
                Email = userView.UserName,
                PhoneNumber = userView.Phone,

            };

            userManager.Create(userASP, userASP.UserName);

            userASP = userManager.FindByName(userView.UserName);
            userManager.AddToRole(userASP.Id, "User");// läger till role i DB, byt till Admin för att skapa en admin User 



        }

        public List<User> GetUserListByFirstName()// används till att lad hem alla användare så att man kan skapa en XML fil
        {
            try
            {

                var users = db.Users.ToList().OrderBy(i => i.FirstName);

                var UsersList = new List<User>();

                foreach (var user in users)
                {

                    UsersList.Add(new User
                    {
                        UserId = user.UserId,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Phone = user.Phone,
                        Adress = user.Adress,
                        UserName = user.UserName,
                        Photo = user.Photo,

                    });
                }


                return UsersList;
            }
            catch
            {
                throw new ApplicationException("An error occured while getting members from the database.");
            }

        }

        public User GetUserByUserEmail(string UserEmail)// hämtar användar ut i från email 
        {
            //UserName är email 
            var user = db.Users.Where(u => u.UserName == UserEmail).FirstOrDefault();
       
            var UserInfo = new UserSettingsView
            {
                Adress = user.Adress,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                Photo = user.Photo,
                UserId = user.UserId,
                UserName = user.UserName,
            };

            return UserInfo;
        }

        public User GetUserByUserId(int UserID)// hämtar användar ut i från ID 
        {
            
            var user = db.Users.Find(UserID);

            return user;
        }

        public List<User> GetListOfAllUser()// hämtar användar ut i från ID 
        {

            var userList = db.Users.ToList();

            return userList;
        }

        public List<User> GetListOfAllUserByFirstNameAndLastName(string FirstNameText, string LastNameText)// söker efter förnamn och efternam i DB
        {
            var usersList = db.Users.Where(x => x.FirstName.StartsWith(FirstNameText) & x.LastName.StartsWith(LastNameText)).ToList();// söker efter förnamn och efternam man sökt på i DB för att visas i viewn 
            return usersList;
        }

        public List<User> GetListUserByFirstName(string FirstNameText)// söker efter förnamn i DB
        {
            var usersList = db.Users.Where(x => x.FirstName.StartsWith(FirstNameText)).ToList();// söker efter förnamn man sökt på i DB för att visas i viewn  

            return usersList;
        }

        public List<string> AutocompleteListByFirstNameAndLastName(string FirstNameText, string LastNameText)// användas vid Userindex för att Autocomplete när man ska söka på användare 
        {
            var UsersList = db.Users.Where(x => x.FirstName.StartsWith(FirstNameText) & x.LastName.StartsWith(LastNameText)).Select(y => y.FirstName + " " + y.LastName).ToList();// söker efter förnamn och efternam man sökt på i DB för att visas på autocomplete 

            return UsersList;
        }

        public List<string> AutocompleteListByFirstName(string FirstNameText)// användas vid Userindex för att Autocomplete när man ska söka på användare
        {
            var UsersList = db.Users.Where(x => x.FirstName.StartsWith(FirstNameText)).Select(y => y.FirstName + " " + y.LastName).ToList();// söker efter förnamnet man sökt på i DB för att visas på autocomplete

            return UsersList;
        }

        public ApplicationUser GetUserByUserEmailFromASPdb(string UserEmail)// hämtar användar med hjälp av email från ASP.net DB
        {

            var userASP = userManager.FindByEmail(UserEmail);

            return userASP;
        }

        public bool GetIfUserIsAdminFromASPdb(string usersID)// kontrolerar om användare är Admin i asp.net DB
        {

            var usersView = userManager.IsInRole(usersID, "Admin"); // kontrolerare om användare är admin 

            return usersView;
        }

        public void EnableAdmin(String UserID)// läger till admin status i ASP.net DB
        {
                userManager.AddToRole(UserID, "Admin");// gör till admin 
                userManager.RemoveFromRole(UserID, "User");// tar bort som användare 
        }

        public void DisableAdmin(String UserID)// tar bort Admin status i ASP.net DB
        {

            userManager.RemoveFromRole(UserID, "Admin");//tar bort som admin 
            userManager.AddToRole(UserID, "User");// gör till användare 
        }

        public void Update(User User)//Regdigerar kontakt 
        {
            if (db.Entry(User).State == EntityState.Detached)// kontrolerar om Entity är detached för att attacha den 
            {
                db.Users.Attach(User);// attachar data till DataContext 

            }

            db.Entry(User).State = EntityState.Modified;// meddelar att data som lagt till är regdigerar och därmed så kommer det sparras till DB 
            //_entities.SaveChanges();
        }

        public void Delete(User User)//tar bort kontkat 
        {
            if (db.Entry(User).State == EntityState.Detached)// kontrolerar om Entity är detached för att attacha den 
            {
                db.Users.Attach(User);// attachar data till DataContext 
            }
            


            db.Users.Remove(User);// kallar på fukntion i EntityFramework som tar bort kontakt

            //blockerare användare från ASP.net DB
            var UserASP = this.GetUserByUserEmailFromASPdb(User.UserName);// usernamn är eamil
            //userManager.SetLockoutEnabledAsync(UserASP.Id, true);// användar data sparas i ASPnet DB men användare kan inte längre logga in i systemet 
            //userManager.SetLockoutEndDateAsync(UserASP.Id, DateTimeOffset.MaxValue);
            userManager.Delete(UserASP);

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
