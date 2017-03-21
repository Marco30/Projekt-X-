using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using OnlineVoting.Models;
using System.IO;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.SqlClient;
using System.Configuration;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.Schema;
using OnlineVoting.Models.Repository;
using System.Xml;

namespace OnlineVoting.Controllers
{

    public class UsersController : Controller
    {

        private IUserRepository _userRepository;
        private IElectionRepository _electionRepository;
        public UsersController()
        {
            _userRepository = new UserRepository();
            _electionRepository = new ElectionRepository();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult XML()// skapar XML fil 
        {

            var report = _userRepository.GetUserListByFirstName();//hämtar från db

            var doc = new XDocument();
            var xmlSerializer = new XmlSerializer(report.GetType());
            using (var writer = doc.CreateWriter())
            {
                xmlSerializer.Serialize(writer, report);
            }

            XElement el = doc.Root;

            var fil = "/UserList.xml";

            el.Save(Server.MapPath("~/Content/XML") + fil);// bör läga till bekreftelse att fil skapatas och filen borde visas i ny flick på webbläsaren 

            TempData["Message"] = "XML file has been created with all the users";

            return RedirectToAction("Index", "Users");
        }

        //-------------------------test ändra XML fil

        public ActionResult TransformXML()// sändrar XML fil
        {

            XDocument XMLfil = XDocument.Load(@"D:\valkod\Marco 2\OnlineVoting\OnlineVoting\Content\METS.66f54119-c80e-4bdf-b3c3-76a625a91400.xml");

            XNamespace mets = "http://www.loc.gov/METS/";

            var docs1 = XMLfil.Descendants(mets + "dmdSec")
               .Select(x => new {
                   //title = x.Element(dc + "title").Value,
                   ID = x.Attribute("ID").Value,
               })
               .ToList();


            XNamespace dcterms = "http://purl.org/dc/terms/";
            XNamespace dc = "http://purl.org/dc/elements/1.1/";

            var docs = XMLfil.Descendants(dcterms + "dublincore")
               .Select(x => new {
                   title = x.Element(dc + "title").Value,
                   subject = x.Element(dc + "subject").Value,
                   description = x.Element(dc + "description").Value,
                   creator = x.Element(dc + "creator").Value,
                   date = x.Element(dc + "date").Value
               })
               .ToList();

            TempData["Message"] = "XML file has " + docs + " " + docs1;

            return RedirectToAction("Index", "Users");
        }

        public ActionResult ValidationXML()// sändrar XML fil
        {
            var FGSModel = new List<FGSModel>();

            XmlSchemaSet schema = new XmlSchemaSet();
            schema.Add("http://www.loc.gov/METS/", @"http://xml.ra.se/e-arkiv/METS/CSPackageMETS.xsd");

            XDocument XMLfil = XDocument.Load(@"D:\valkod\Marco 2\OnlineVoting\OnlineVoting\Content\METS.66f54119-c80e-4bdf-b3c3-76a625a91400.xml", LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);

            Boolean result = false;

            int ID = 0; 

            XMLfil.Validate(schema, (sender, e) =>
            {
                //TempData["Message"] = TempData["Message"] + " XML file error " + e.Message;

                IXmlLineInfo info = sender as IXmlLineInfo;

                XElement reader = ((XElement)sender);

                var XMLNode = reader.Name.ToString();
                XMLNode = XMLNode.Replace("{", "");
                XMLNode = XMLNode.Replace("}", ":");

                var XMLattribute = reader.FirstAttribute.ToString();


                //-----------------model test---------------

                var NewAttribute = FilterWords(e.Message);

                FGSModel.Add(new FGSModel
                {
                    FGSId = ID++,
                    Node = XMLNode,
                    NewAttribute = NewAttribute,
                    Attribute = XMLattribute,
                    Message = info.LineNumber.ToString(),
                });
                //----------------------------------

                //string Message = string.Format("{0}: {1} Node: {2} - Attribute: {3} - Line: {4}", e.Severity, e.Message, XMLNode, XMLattribute, info != null ? info.LineNumber.ToString() : "not known");

                //TempData["Message"] = TempData["Message"] + Message;

                result = true;
            });

            if(result)
            {
                Console.WriteLine("Validation Failed");
            }
            else
            {
                Console.WriteLine("Validation succeeded");
            }


            //TempData["Message"] = "XML file has been created with all the users" + docs + " " + docs1;

            return RedirectToAction("index", "Users");
        }

        static string FilterWords(string str)
        {
            var ListOfWorlds = str.Split(' ').Where(w => w == w.ToUpper());

            var upperCaseWord = "";

            foreach (string s in ListOfWorlds)
            {
                upperCaseWord = s;
            }

                return upperCaseWord;

        }

        public ActionResult FGSEdit()// hämtar lista på alla användare som finns på DB
        {

            var FGSModel = new List<FGSModel>();

            XmlSchemaSet schema = new XmlSchemaSet();
            schema.Add("http://www.loc.gov/METS/", @"http://xml.ra.se/e-arkiv/METS/CSPackageMETS.xsd");

            XDocument XMLfil = XDocument.Load(@"D:\valkod\Marco 2\OnlineVoting\OnlineVoting\Content\METS.66f54119-c80e-4bdf-b3c3-76a625a91400.xml", LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);

            Boolean result = false;

            int ID = 0;

            XMLfil.Validate(schema, (sender, e) =>
            {
                //TempData["Message"] = TempData["Message"] + " XML file error " + e.Message;

                IXmlLineInfo info = sender as IXmlLineInfo;

                XElement reader = ((XElement)sender);

                var XMLNode = reader.Name.ToString();
                XMLNode = XMLNode.Replace("{", "");
                XMLNode = XMLNode.Replace("}", ":");

                var XMLattribute = reader.FirstAttribute.ToString();


                //-----------------model test---------------

                var NewAttribute = FilterWords(e.Message);

                FGSModel.Add(new FGSModel
                {
                    FGSId = ID++,
                    Node = XMLNode,
                    NewAttribute = NewAttribute,
                    Attribute = XMLattribute,
                    Line = info.LineNumber.ToString(),
                    Message = e.Message,
                });
                //----------------------------------

                //string Message = string.Format("{0}: {1} Node: {2} - Attribute: {3} - Line: {4}", e.Severity, e.Message, XMLNode, XMLattribute, info != null ? info.LineNumber.ToString() : "not known");

                //TempData["Message"] = TempData["Message"] + Message;

                result = true;
            });

            if (result)
            {
                Console.WriteLine("Validation Failed");
            }
            else
            {
                Console.WriteLine("Validation succeeded");
            }

            return View(FGSModel);
        }

        [HttpPost]
        public ActionResult FGSEditXML(List<FGSModel> FGS)// hämtar lista på alla användare som finns på DB
        {
            var test = FGS;
            return View(FGS);
        }

        public ActionResult AddToXML()// sändrar XML fil
        {

            XDocument XMLfil = XDocument.Load(@"D:\valkod\Marco 2\OnlineVoting\OnlineVoting\Content\METS.66f54119-c80e-4bdf-b3c3-76a625a91400.xml");

            XNamespace mets = "http://www.loc.gov/METS/";


            XMLfil.Element(mets + "mets").Add(new XAttribute("OBJID", "UUID:4129e475-4572-415d-a8aa-2424b7fdd16e"), new XAttribute("TYPE", "ERMS"), new XAttribute("PROFILE", "http://www.loc.gov/standards/mets/version18/mets.xsd"));// TYPE måste var minst 4 täcken

            XMLfil.Save(@"D:\valkod\Marco 2\OnlineVoting\OnlineVoting\Content\SIP.xml");

            /* var docs1 = XMLfil.Descendants(mets + "mets")
                .Select(x => new {
                    //title = x.Element(dc + "title").Value,
                    OBJID = x.Attribute("OBJID").Value,
                    TYPE = x.Attribute("TYPE").Value,
                    PROFILE = x.Attribute("PROFILE").Value,
                })
                .ToList();

             TempData["Message"] = "XML file has " + docs1;*/



            return RedirectToAction("Index", "Users");
        }

        //------------------------------------------

        [Authorize(Roles = "User")]
        public ActionResult MySettings()// visar view med användare info för att kunna ändra den
        {

            var UserSettingsView = _userRepository.GetUserByUserEmail(this.User.Identity.Name);// this.User.Identity.Name ger oss användar namnet på användare som är email 

            return View(UserSettingsView);
        }

        [HttpPost]
        public ActionResult MySettings(UserSettingsView view)// postar ändringarna man gjort i sin användar info 
        {
            if (ModelState.IsValid)
            {
                //Upload image
                string path = string.Empty;
                string pic = string.Empty;

                if (view.NewPhoto != null)
                {

                    pic = Path.GetFileName(view.NewPhoto.FileName);//hämtar anmnet på bilden  

                    path = Path.Combine(Server.MapPath("~/Content/Photos"), pic);   //sökvägen(~= relative rute)

                    view.NewPhoto.SaveAs(path);// sparar vägen 

                    using (MemoryStream ms = new MemoryStream())// laddar up bilden 
                    {
                        view.NewPhoto.InputStream.CopyTo(ms);
                        byte[] array = ms.GetBuffer();
                    }
                }

                var user = _userRepository.GetUserByUserId(view.UserId);// hämtar user från DB

                user.Adress = view.Adress;
                user.FirstName = view.FirstName;
                user.LastName = view.LastName;
                user.Phone = view.Phone;

                if (!string.IsNullOrEmpty(pic))
                {
                    user.Photo = string.Format("~/Content/Photos/{0}", pic);
                }

                _userRepository.Update(user);// uppdaterar DB
                _userRepository.Save();// sparar ändringar i DB

                TempData["Message"] = "You have update your profile!";

                return RedirectToAction("Index", "Home");
            }

            return View(view);
        }


        [Authorize(Roles = "Admin")]
        public ActionResult OnOffAdmin(int id)// funktion som kan göra användare till admin, On/Off Admin 
        {
            var user = _userRepository.GetUserByUserId(id);// får User från DB

            if (user != null)
            {

                var userASP = _userRepository.GetUserByUserEmailFromASPdb(user.UserName);// kontrolerare om användare finns i asp.net DB

                if (userASP != null)
                {
                    if (_userRepository.GetIfUserIsAdminFromASPdb(userASP.Id.ToString()))// kontrolerare om användare är admin
                    {
                        _userRepository.DisableAdmin(userASP.Id);// tar bort Admin status 
                    }
                    else
                    {
                        _userRepository.EnableAdmin(userASP.Id);// läger till Admin status 
                    }
                }
            }


            return RedirectToAction("Index");
        }


        [Authorize(Roles = "Admin")]
        public ActionResult Index()// hämtar lista på alla användare som finns på DB
        {


            var usersList = _userRepository.GetListOfAllUser();

            var usersView = new List<UserIndexView>();

            foreach (var user in usersList)
            {
                var userASP = _userRepository.GetUserByUserEmailFromASPdb(user.UserName);//UserName är email 

                usersView.Add(new UserIndexView
                {
                    Adress = user.Adress,
                    Candidates = user.Candidates,
                    FirstName = user.FirstName,
                    IsAdmin = userASP != null && _userRepository.GetIfUserIsAdminFromASPdb(userASP.Id.ToString()),//userManager.IsInRole(userASP.UserId, "Admin"),
                    LastName = user.LastName,
                    Phone = user.Phone,
                    Photo = user.Photo,
                    UserId = user.UserId,
                    UserName = user.UserName,
                });
            }

            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"].ToString();// visar medelande som tagit med fron Edit view
            }

            return View(usersView);
        }

        //---------------------------------------search User--------------------------------------------------


        [HttpPost]
        public ActionResult _SearchUser(String SearchText)// visar den användare man sökt på 
        {

            var usersView = new List<UserIndexView>();
            var users = new List<User>();

            if (string.IsNullOrEmpty(SearchText))// om inget namn gets vid sökings så får man lista på alla 
            {

                users = _userRepository.GetListOfAllUser();
            }
            else
            {
                if (SearchText.Contains(" "))// kontrolerare om sökningen man gör har ett för namn och efternamn 
                {
                    string[] array = SearchText.Split(new char[] { ' ' }, 2);

                    var FirstNameText = array[0];
                    var LastNameText = array[1];

                    users = _userRepository.GetListOfAllUserByFirstNameAndLastName(FirstNameText, LastNameText);// söker efter förnamn och efternam man sökt på i DB för att visas i viewn 
                }
                else
                {
                    users = _userRepository.GetListUserByFirstName(SearchText);

                }
            }


            foreach (var user in users)
            {


                var userASP = _userRepository.GetUserByUserEmailFromASPdb(user.UserName);// hämtar användare från ASP.net DB

                usersView.Add(new UserIndexView
                {
                    Adress = user.Adress,
                    Candidates = user.Candidates,
                    FirstName = user.FirstName,
                    IsAdmin = userASP != null && _userRepository.GetIfUserIsAdminFromASPdb(userASP.Id.ToString()),// kontrolerare om User är Admin
                    LastName = user.LastName,
                    Phone = user.Phone,
                    Photo = user.Photo,
                    UserId = user.UserId,
                    UserName = user.UserName,
                });

            }

            return PartialView("_UserInfo", usersView);

        }


        public JsonResult GetNameSearch(String term)// funktion som används av autocomplete jquery
        {
            List<String> UsersList;// skapar lista som kommer användas för att spara alla User från DB

            if (term.Contains(" "))
            {
                string[] array = term.Split(new char[] { ' ' }, 2);

                var FirstNameText = array[0];
                var LastNameText = array[1];

                UsersList = _userRepository.AutocompleteListByFirstNameAndLastName(FirstNameText, LastNameText);


            }
            else
            {
                UsersList = _userRepository.AutocompleteListByFirstName(term);

            }


            return Json(UsersList, JsonRequestBehavior.AllowGet);
        }

        //-----------------------------------------------------------------------------------------

        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)// hämtar specifik användares info
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = _userRepository.GetUserByUserId(id.GetValueOrDefault());// .GetValueOrDefault(); omvandlar från en itne som kan ha värdet null till vanlig int genom att ge den värdet 0 om den är null
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }


        [Authorize(Roles = "Admin")]
        public ActionResult Create()// användas av admin för att skapa ny anändare 
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserCreateEditView userView)// postar den nya anändaren som admin skapat 
        {
            if (!ModelState.IsValid)
            {

                return View(userView);
            }


            //ladar upp bild 

            String path = string.Empty;
            String pic = string.Empty;

            if (userView.Photo != null)
            {

                pic = Path.GetFileName(userView.Photo.FileName);
                path = Path.Combine(Server.MapPath("~/Content/Photos"), pic);
                userView.Photo.SaveAs(path);
                using (MemoryStream ms = new MemoryStream())
                {
                    userView.Photo.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();
                }
            }

            //sparar användar info

            var user = new User
            {

                Adress = userView.Adress,
                FirstName = userView.FirstName,
                LastName = userView.LastName,
                Phone = userView.Phone,
                Photo = pic == string.Empty ? string.Empty : string.Format("~/Content/Photos/{0}", pic),
                UserName = userView.UserName,


            };

            _userRepository.Add(user);

            try
            {
                _userRepository.Save(); // skpar användare i user DB

                _userRepository.AdminCreatesUserInASPdb(userView);// skapar användare i asp.net automat genererad DB
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.InnerException != null && ex.InnerException.InnerException.Message.Contains("UserNameIndex"))
                {

                    ViewBag.Error = "The email has already been used by another user";

                }
                else
                {
                    ViewBag.Error = ex.Message;

                }

                return View(userView);
            }

            return RedirectToAction("Index");
        }


        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)// används för att kuna ändra användares info
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = _userRepository.GetUserByUserId(id.GetValueOrDefault());

            if (user == null)
            {
                return HttpNotFound();

            }

            var userView = new UserSettingsView
            {

                Adress = user.Adress,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                UserId = user.UserId,
                UserName = user.UserName,
                Photo = user.Photo,

            };

            return View(userView);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserSettingsView userView)// postar det ändrade infot om användaren 
        {
            if (!ModelState.IsValid)
            {
                return View(userView);
            }

            //Upload image

            String path = string.Empty;
            String pic = string.Empty;

            if (userView.NewPhoto != null)
            {

                pic = Path.GetFileName(userView.NewPhoto.FileName);

                path = Path.Combine(Server.MapPath("~/Content/Photos"), pic);

                userView.NewPhoto.SaveAs(path);

                using (MemoryStream ms = new MemoryStream())
                {
                    userView.NewPhoto.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();
                }
            }

            var user = _userRepository.GetUserByUserId(userView.UserId);

            user.Adress = userView.Adress;
            user.FirstName = userView.FirstName;
            user.LastName = userView.LastName;
            user.Phone = userView.Phone;

            if (!string.IsNullOrEmpty(pic))
            {

                user.Photo = string.Format("~/Content/Photos/{0}", pic);

            }

            _userRepository.Update(user);
            _userRepository.Save();

            TempData["Message"] = "You have update " + userView.FirstName + " " + userView.LastName + " profile!";

            return RedirectToAction("Index");
        }

        //----------------testar ny delit funkntion------------------------------

        [Authorize(Roles = "Admin")]
        public ActionResult UserDeleteEnabled(int? id)// visar view där man kan ta bort valet 
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User user = _userRepository.GetUserByUserId(id.GetValueOrDefault());


            if (user == null)
            {
                return HttpNotFound();
            }

            user.DeleteEnabled = true;

            _userRepository.Update(user);

            _userRepository.Delete(user);// tar bort från ASP.net user DB

            _userRepository.Save();


            //---- aktiverarer Delete i candidate DB

            var candidate = _electionRepository.GetListOfAllElectionsOneCandidateIsIn(id.GetValueOrDefault());

            if (candidate != null)
            {

                for (var i = 0; i < candidate.Count; i++)//loopar alla val en kandidat varit med om och aktiverare delitare användaren 
                {


                    candidate[i].DeleteEnabled = true;

                    _electionRepository.UpdateCandidate(candidate[i]);



                }

                _electionRepository.Save();

            }

            return RedirectToAction("Index");
        }

        //-----------------------------------------------------------------------

        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)// hämtar anvädnare som ska tas bort
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = _userRepository.GetUserByUserId(id.GetValueOrDefault());
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)// postar användare som ska tas bort 
        {
            User user = _userRepository.GetUserByUserId(id);

            _userRepository.Delete(user);

            try
            {
                _userRepository.Save();
            }

            catch (Exception ex)
            {
                if (ex.InnerException != null &&
                      ex.InnerException.InnerException != null &&
                      ex.InnerException.InnerException.Message.Contains("REFERENCE"))
                {
                    ModelState.AddModelError(string.Empty, "Can't delete the register because it has related records to it");

                }
                else
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }

                return View(user);
            }
            return RedirectToAction("Index");

        }

    }
}
