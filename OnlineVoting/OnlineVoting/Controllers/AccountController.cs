using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using OnlineVoting.Models;
using System.IO;

using System.Net.Mail;
using System.Net;

using OnlineVoting.Models.Repository;

namespace OnlineVoting.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private IUserRepository _userRepository;
        private IAccountRepository _accountRepository;

        public AccountController()
        {
            _accountRepository = new AccountRepository();
            _userRepository = new UserRepository();

        }


        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        // public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {

            //----------------- test email validering 

            if (ModelState.IsValid)
            {
                var user = await _accountRepository.GetUserByEmailAndPassword(model.UserName, model.Password);

                if (user != null)
                {
                    if (user.EmailConfirmed == true)
                    {
                        await SignInAsync(user, model.RememberMe); return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Confirm Email Address.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }

            //-------------------------------------


            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        //public async Task<ActionResult> Register(AccountRegisterUserView userView)
        public ActionResult Register(AccountRegisterUserView userView)
        {
            if (ModelState.IsValid)
            {
                //Upload image
                string path = string.Empty;
                string pic = string.Empty;

                if (userView.Photo != null)
                {
                    //pic=  photo name  
                    pic = Path.GetFileName(userView.Photo.FileName);
                    //path= folder to save photos         ~= relative rute
                    path = Path.Combine(Server.MapPath("~/Content/Photos"), pic);
                    // local save 
                    userView.Photo.SaveAs(path);
                    //up photo
                    using (MemoryStream ms = new MemoryStream())
                    {
                        userView.Photo.InputStream.CopyTo(ms);
                        byte[] array = ms.GetBuffer();
                    }
                }

                //Save record
                var user = new User
                {
                    Adress = userView.Adress,
                    FirstName = userView.FirstName,
                    LastName = userView.LastName,
                    Phone = userView.Phone,
                    Photo = string.IsNullOrEmpty(pic) ? string.Empty : string.Format("~/Content/Photos/{0}", pic),
                    UserName = userView.UserName,

                };


                // try catch for validation of error    

                try
                {

                    var userASP = _accountRepository.CreatesUserInASPdb(userView);// skpar användar i Asp.net DB

                    _userRepository.Add(user); // läger till anändar i DB

                    _userRepository.Save();// sparar till DB

                    //--------------Eamil test

                    if (userASP != null)
                    {

                        string TextMessage = string.Format("Dear {0}<BR/>Thank you for your registration, please click on the below link to comlete your registration: <a href=\"{1}\" title=\"User Email Confirm\">{1}</a>", user.UserName, Url.Action("ConfirmEmail", "Account", new { Token = userASP.Id, Email = userASP.Email }, Request.Url.Scheme));
                        string Email = userASP.Email;
                        string EmailSubject = "Email confirmation";

                        try
                        {
                            SendEmail(Email, EmailSubject, TextMessage);
                        }
                        catch (Exception ex)// fixar bug med att användar skapades fast email misslyckats att skickas 
                        {

                            User userDB = _userRepository.GetUserByUserEmail(userASP.Email);

                            User userDelit = _userRepository.GetUserByUserId(userDB.UserId);

                            _userRepository.Delete(userDelit);// tar bort den skapade användaren 

                            _userRepository.Save();//sparat att man tagit bort användaren 

                            ModelState.AddModelError(string.Empty, ex.Message);

                            return View(userView);
                        }

                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "finns ingen användare att skicka mail till");
                    }

                    //--------------------


                    // auto login   -   isPersistent=remember in the sesion
                    //await SignInAsync(userASP, isPersistent: false);
                    //return RedirectToAction("Index", "Home");
                    return RedirectToAction("Confirm", "Account", new { Email = userASP.Email });

                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null &&
                        ex.InnerException.InnerException != null &&
                        ex.InnerException.InnerException.Message.Contains("UserNameIndex"))
                    {
                        ModelState.AddModelError(string.Empty, "The email has already used for another user");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);

                    }

                    return View(userView);
                }
            }

            // If we reach this point , it is that it was an error and re- display the form
            return View(userView);
        }


        //--------Email test-----------------

        [AllowAnonymous]
        public void SendEmail(string Email, string EmailSubject, string TextMessage)// funktion som skickar mail
        {
            MailMessage mailMessage = new MailMessage(
    new MailAddress("Marco.villegas@live.se", "Web Registration"),
    new MailAddress(Email));
            mailMessage.Subject = EmailSubject;
            mailMessage.Body = TextMessage;
            mailMessage.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient("smtp.live.com", 587); // 587 är för lokal bruk men 25 på don webb server
            smtp.Credentials = new NetworkCredential("Marco.villegas@live.se", "darkrign3030");
            smtp.EnableSsl =  true;
            smtp.Send(mailMessage);
        }


        [AllowAnonymous]
        public ActionResult Confirm(string Email)// visar confirm view
        {
            ViewBag.Email = Email;
            return View();
        }

        // GET: /Account/ConfirmEmail 
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string Token, string Email)// används för att bekräfta användare genom email 
        {
            ApplicationUser user = _accountRepository.GetUserByIdInASPdb(Token);
            //ApplicationUser user = this.UserManager.FindById(Token);
            if (user != null)
            {
                if (user.Email == Email)
                {
                    user.EmailConfirmed = true;

                    bool updated = await _accountRepository.Update(user);

                    if (updated == true)
                    {
                        _accountRepository.Save();
                    }
                    //await UserManager.UpdateAsync(user);
                    await SignInAsync(user, isPersistent: false);

                    return RedirectToAction("Index", "Home", new { ConfirmedEmail = user.Email });
                }
                else
                {
                    return RedirectToAction("Confirm", "Account", new { Email = user.Email });
                }
            }
            else
            {
                return RedirectToAction("Confirm", "Account", new { Email = "" });
            }

        }

        //------------------------------

        //------Lost password------------------------

        [AllowAnonymous]
        public ActionResult RecoverPassword()// visar confirm view
        {
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"].ToString();// visar medelande som tagit med fron Edit view
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RecoverPassword(User user)// visar confirm view
        {
            ApplicationUser userASP = _userRepository.GetUserByUserEmailFromASPdb(user.UserName);
            //ApplicationUser user = this.UserManager.FindById(Token);
            if (userASP != null)
            {
                if (userASP.Email == user.UserName)
                {

                    string TextMessage = string.Format("{0}, you are trying to reset your password if it is not you then just ignore this email, to reset your password klick on this linke: <BR/> <a href=\"{1}\" title=\"User Email Reset\">{1}</a>", userASP.UserName, Url.Action("SetNewPassword", "Account", new { Token = userASP.Id, Email = userASP.Email }, Request.Url.Scheme));
                    string Email = userASP.Email;
                    string EmailSubject = "Password Recovery";
                    //jätte bra, det kan bli något av detta
                    SendEmail(Email, EmailSubject, TextMessage);

                    TempData["Message"] = "A email that will help you recover you password has been sent to you!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["Message"] = "The user exist but the recovery email does not match users, could be errors in the DB!";
                    return RedirectToAction("RecoverPassword", "Account");
                }
            }
            else
            {
                TempData["Message"] = "The email does not exist!";
                return RedirectToAction("RecoverPassword", "Account");
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> SetNewPassword(string Token, string Email)// används för att bekräfta användare genom email 
        {
            ApplicationUser user = _accountRepository.GetUserByIdInASPdb(Token);
            //ApplicationUser user = this.UserManager.FindById(Token);
            if (user != null)
            {
                if (user.Email == Email)
                {
                    //await UserManager.UpdateAsync(user);
                    await SignInAsync(user, isPersistent: false);

                    return RedirectToAction("Manage", "Account", routeValues: null);// not fel här tita och fixa
                }
                else
                {
                    TempData["Message"] = "The user exist but the recovery link email does not match users, could be errors in the DB!";
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                TempData["Message"] = "The email does not exist!";
                return RedirectToAction("Index", "Home");
            }

        }

        //-------------------------------


        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult _LoginPartial()// används i partail view för att visa namn på dem som är inlogad 
        {
            // ASP.NET Identity
            if (User.Identity.GetUserId() != null)// kontroller om man är inlogad 
            {
                var ASPuser = _accountRepository.GetUserByIdInASPdb(User.Identity.GetUserId());
                var user = _userRepository.GetUserByUserEmail(ASPuser.Email);
                // Membership
                // var user = db.UserProfiles.SingleOrDefault(m => m.UserName == User.Identity.Name);

                return PartialView("_LoginPartial", user);
            }
            else
            {
                var user = new UserSettingsView();

                return PartialView("_LoginPartial", user);
            }

        }


        //
        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;

            IdentityResult result = await _accountRepository.RemoveLoginInASPdb(User.Identity.GetUserId(), loginProvider, providerKey);


            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");

            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await _accountRepository.ChangePasswordForUserInASPdb(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);// ändrar lösen i asp.net DB 
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await _accountRepository.AddPasswordForUserToASPdb(User.Identity.GetUserId(), model.NewPassword);// Läger till ny password til en användare i DB 
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }


        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = _accountRepository.GetUsersLoginsInfoFromASPdb(User.Identity.GetUserId());

            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            var identity = await _accountRepository.ControlIdentityInASPdb(user);
            //await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);

            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = _accountRepository.GetUserByIdInASPdb(User.Identity.GetUserId());// hittar användar i ASP.net Db ut i från id

            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri) : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}