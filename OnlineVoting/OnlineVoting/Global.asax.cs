using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OnlineVoting.Models;

namespace OnlineVoting
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            this.CreateSuperuser();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }


        private void CreateSuperuser()
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var db = new OnlineVotingContext();
            this.CheckRole("Admin", userContext);
            this.CheckRole("User", userContext);

            var user = db.Users
                .Where(u => u.UserName.ToLower().Equals("Admin@live.se")).FirstOrDefault();

            if (user == null)
            {

                user = new User
                {
                    Adress = "Terapivägen 16E",
                    FirstName = "Marco",
                    LastName = "Villegas",
                    Phone = "0706880589",
                    UserName = "Admin@live.se",
                    Photo = "~/Content/Photos/MeAndMyself2.png",

                };

                db.Users.Add(user);
                db.SaveChanges();
            }

            var userASP = userManager.FindByName(user.UserName);

            if (userASP == null)
            {

                userASP = new ApplicationUser
                {
                    UserName = user.UserName,
                    Email = user.UserName,
                    PhoneNumber = user.Phone,
                };

                userManager.Create(userASP, "admin123");

            }

            userManager.AddToRole(userASP.Id, "Admin");

        }

        private void CheckRole(string roleName, ApplicationDbContext userContext)
        {
            // User management

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(userContext));

            // Check to see if Role Exists, if not create it
            if (!roleManager.RoleExists(roleName))
            {
                roleManager.Create(new IdentityRole(roleName));
            }


        }

    }
}
