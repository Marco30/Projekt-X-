using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using OnlineVoting.Models;
using System.Data.SqlClient;
using System.Configuration;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OnlineVoting.Models.Repository;

namespace OnlineVoting.Controllers
{

    public class ElectionsController : Controller
    {


        private IElectionRepository _electionRepository;
        private IUserRepository _userRepository;
        private IStateRepository _stateRepository;

        public ElectionsController()
        {
            _electionRepository = new ElectionRepository();
            _userRepository = new UserRepository();
            _stateRepository = new StateRepository();
        }


        [Authorize(Roles = "Admin")]
        public ActionResult Close(int id)// används för att stänga vallet 
        {

            var voting = _electionRepository.GetElectionById(id);

            if (voting != null)
            {
                var candidate = _electionRepository.GetListOfAllElectionCandidates(voting.ElectionId);


                if (candidate != null)
                {
                    var state = this.GetState("Closed");
                    voting.StateId = state.StateId;
                    voting.CandidateWinId = candidate.User.UserId;

                    _electionRepository.UpdateElection(voting);// updaterar valet i DB

                    _electionRepository.Save();// sparar ändrignarna til DB 

                }
            }

            return RedirectToAction("Index");
        }


        public ActionResult ShowResults(int id)// visar resultat på valet med vinar och lista på alla deltagar och antal röster dem fåt
        {
            try
            {
                var Rank = _electionRepository.ShowResultsOfElectionById(id);

                ViewBag.NameOfElection = Rank[0].Electionname;
                ViewBag.StateOfElection = Rank[0].State;
                ViewBag.WinnerOfElection = Rank[0].Candidate;
                ViewBag.NumberOfVotes = Rank[0].QuantityVotes;

                return View(Rank);
            }
            catch// det körs när ShowResultsOfElectionById() misslyckats, vilket betyder att det är något fel i DB eller att det finns inga kandidater och där med finns ingen data
            {
                var election = _electionRepository.GetElectionById(id);
                TempData["Message"] = "There are no candidates in this election (" + election.Description + ")";
                return Redirect(ControllerContext.HttpContext.Request.UrlReferrer.T‌​oString());
            }
           
        }


        [Authorize(Roles = "User")]
        public ActionResult Results()// listar alla val som hålits
        {
            var state = this.GetState("Closed");

            var votings = _electionRepository.GetElectionByStateId(state);

            var views = new List<ElectionIndexView>();

            var Yearlist = new List<string>();


            foreach (var voting in votings)// går igenom listan man fåt från DB och skapar ny model 
            {
                User user = null;

                if (voting.CandidateWinId != 0)
                {
                    user = _userRepository.GetUserByUserId(voting.CandidateWinId);
                }

                views.Add(new ElectionIndexView
                {
                    CandidateWinId = voting.CandidateWinId,
                    DateTimeEnd = voting.DateTimeEnd,
                    DateTimeStart = voting.DateTimeStart,
                    Description = voting.Description,
                    IsEnableBlankVote = voting.IsEnableBlankVote,
                    IsForAllUsers = voting.IsForAllUsers,
                    QuantityBlankVotes = voting.QuantityBlankVotes,
                    QuantityVotes = voting.QuantityVotes,
                    Remarks = voting.Remarks,
                    StateId = voting.StateId,
                    State = voting.State,
                    ElectionId = voting.ElectionId,
                    Winner = user,

                });
                var year = voting.DateTimeStart.ToString("yyyy");

                Yearlist.Add(year);
            }


            Yearlist = Yearlist.Distinct().ToList();

            ViewBag.SelectedYear = new SelectList(Yearlist);

            var Monthslist = new List<MonthsList>();

            ViewBag.SelectedMonths = new SelectList(Monthslist, "MonthsID", "Months");

            ViewBag.empty = false;

            if (views.Count == 0)
            {
                ViewBag.empty = true;
            }

            return View(views);


        }

        //-----------------test index sök------------------------------------------------------------------------

        [Authorize(Roles = "User")]
        [HttpPost]
        public ActionResult ResultsIndexSearch(String SearchText)// gör sökning på val namn i index view och ger resultatet 
        {
            List<Election> ElectionList;

            var views = new List<ElectionIndexView>();

            var state = this.GetState("Closed");


            if (string.IsNullOrEmpty(SearchText))
            {
                ElectionList = _electionRepository.GetElectionByStateId(state);
            }
            else
            {
                ElectionList = _electionRepository.GetElectionByNameAndStateId(SearchText, state);// söker efter val namnet man sökt på i DB för att visas i viewn 
            }

            foreach (var Election in ElectionList)
            {
                User user = null;
                if (Election.CandidateWinId != 0)
                {
                    user = _userRepository.GetUserByUserId(Election.CandidateWinId);
                }

                views.Add(new ElectionIndexView
                {
                    CandidateWinId = Election.CandidateWinId,
                    DateTimeEnd = Election.DateTimeEnd,
                    DateTimeStart = Election.DateTimeStart,
                    Description = Election.Description,
                    IsEnableBlankVote = Election.IsEnableBlankVote,
                    IsForAllUsers = Election.IsForAllUsers,
                    QuantityBlankVotes = Election.QuantityBlankVotes,
                    QuantityVotes = Election.QuantityVotes,
                    Remarks = Election.Remarks,
                    StateId = Election.StateId,
                    State = Election.State,
                    ElectionId = Election.ElectionId,
                    Winner = user,

                });
            }


            return PartialView("_UserResultsInfo", views);
        }


        public JsonResult GetElectionResultsSearch(String term)// funktion som används av autocomplete jquery i index view för sökning på val namn  
        {
            List<String> ElectionList;// skapar lista som kommer användas för att spara alla User från DB

            var state = this.GetState("Closed");

            ElectionList = _electionRepository.GetElectionByNameAndStateIdForAutocomplete(term, state);//  söker efter namnet man sökt på och state i DB för att visas på autocomplete

            return Json(ElectionList, JsonRequestBehavior.AllowGet);
        }

        //-------------testa indexorderby----------------------------

        [Authorize(Roles = "User")]
        [HttpPost]
        public ActionResult ResultsIndexOrderBy(string SelectedYear, string SelectedMonths)// visar index view meda alla valen för admin och om den är avslutad så visas vinare
        {
            if (SelectedMonths == "0")
            {
                SelectedMonths = "";
            }

            List<Election> ElectionList;

            int MonthsNum = 0;


            var SMonths = 0;

            if (SelectedMonths != "")
            {
                SMonths = Int32.Parse(SelectedMonths);

                foreach (var Months in TempData["List"] as List<MonthsList>)
                {
                    if (SMonths == Months.MonthsID)
                    {
                        MonthsNum = Months.Months;
                    }
                }
            }


            int Syear = 0;

            var state = this.GetState("Closed");

            if (SelectedYear != "" & SelectedMonths != "")
            {
                Syear = Int32.Parse(SelectedYear);

                ElectionList = _electionRepository.GetElectionByYearMonthsAndStateId(Syear, MonthsNum, state);
            }
            else if (SelectedYear != "" & SelectedMonths == "")
            {

                Syear = Int32.Parse(SelectedYear);
                ElectionList = _electionRepository.GetElectionByYearAndStateId(Syear, state);
            }
            else if (SelectedYear == "" & SelectedMonths != "")
            {
                ElectionList = _electionRepository.GetElectionByMonthsAndStateId(MonthsNum, state);
            }
            else
            {
                ElectionList = _electionRepository.GetElectionByStateId(state);
            }

            var views = new List<ElectionIndexView>();

            foreach (var Election in ElectionList)
            {
                User user = null;
                if (Election.CandidateWinId != 0)
                {
                    user = _userRepository.GetUserByUserId(Election.CandidateWinId);
                }

                views.Add(new ElectionIndexView
                {
                    CandidateWinId = Election.CandidateWinId,
                    DateTimeEnd = Election.DateTimeEnd,
                    DateTimeStart = Election.DateTimeStart,
                    Description = Election.Description,
                    IsEnableBlankVote = Election.IsEnableBlankVote,
                    IsForAllUsers = Election.IsForAllUsers,
                    QuantityBlankVotes = Election.QuantityBlankVotes,
                    QuantityVotes = Election.QuantityVotes,
                    Remarks = Election.Remarks,
                    StateId = Election.StateId,
                    State = Election.State,
                    ElectionId = Election.ElectionId,
                    Winner = user,

                });
            }

            TempData["List"] = TempData["List"];


            return PartialView("_UserResultsInfo", views);

        }


        //------------------------------------------ test månad 

        //[HttpPost]
        public JsonResult FetchMonthsResults(int selectedYear)// genrerar månaderna som har eller har haft vall ut i från året i DB
        {
            var state = this.GetState("Closed");

            int Year = selectedYear; //Int32.Parse(selectedYear);

            var votings = _electionRepository.GetElectionByYearAndStateId(Year, state);

            var MontsListsOfYear = new List<MonthsList>();

            var MontsList = new List<int>();

            //visar info om valet och visar också vinare om vallet är slut förd 
            foreach (var voting in votings)
            {


                var Months = voting.DateTimeStart.ToString("MM");
                MontsList.Add(Int32.Parse(Months));

            }

            MontsList = MontsList.Distinct().ToList();
            MontsList.Add(0);
            MontsList.Sort();
            int i = 0;

            foreach (var M in MontsList)
            {

                MontsListsOfYear.Add(new MonthsList
                {
                    MonthsID = i++,
                    Months = M,
                });



            }

            TempData["List"] = MontsListsOfYear.OrderBy(s => s.Months).ToList();

            return Json(MontsListsOfYear, JsonRequestBehavior.AllowGet);
        }

        //-------------------------------------------



        [Authorize(Roles = "User")]
        public ActionResult VoteForCandidate(int candidateId, int ElectionId)// röstnings funktion, används för att rösta 
        {
            // validering av anvädnare 
            var user = _userRepository.GetUserByUserEmail(this.User.Identity.Name);


            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var candidate = _electionRepository.GetCandidateById(candidateId);

            if (candidate == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var election = _electionRepository.GetElectionById(ElectionId);


            if (election == null)
            {
                return RedirectToAction("Index", "Home");
            }

            //kör röstnings funktionen 
            if (this.VoteCandidate(user, candidate, election))
            {
                TempData["Message"] = "Success,You have voted!";

                return RedirectToAction("ElectionsForUsers");
            }

            return RedirectToAction("Index", "Home");
        }

        private bool VoteCandidate(Models.User user, Candidate candidate, Election election) // röstnings funktion 
        {

            using (var transaction = _electionRepository.Transaction())// kontakt med DB och transaction anbvänds i MVC för att kunna läga till data i flera tabeler 
            {
                var votingDetail = new ElectionVotingDetail
                {
                    CandidateId = candidate.CandidateId,
                    DateTime = DateTime.Now,
                    UserId = user.UserId,
                    ElectionId = election.ElectionId,
                };


                _electionRepository.VotingDetailAdd(votingDetail);


                candidate.QuantityVotes++;// läger till en röst 

                _electionRepository.UpdateCandidate(candidate);// läger till röst i DB

                election.QuantityVotes++;

                _electionRepository.UpdateElection(election);

                //sparar data i DB
                try
                {
                    _electionRepository.Save();

                    transaction.Commit();
                    return true;
                }
                catch (Exception)// om någto går fel så går DB till baks till inna något ändrats 
                {
                    transaction.Rollback();
                }
            }
            return false;
        }

        //---------------------------------------------------- Testar Blankröst  

        [Authorize(Roles = "User")]
        public ActionResult VoteForBlankCandidate(int candidateId, int ElectionId)// röstnings funktion, validerign av användare för få möjlighet att rösta blankt 
        {

            // validering av anvädnare 
            var user = _userRepository.GetUserByUserEmail(this.User.Identity.Name);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var candidate = _electionRepository.GetCandidateById(candidateId);


            if (candidate == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var election = _electionRepository.GetElectionById(ElectionId);


            if (election == null)
            {
                return RedirectToAction("Index", "Home");
            }

            //validering om man inte röstat så kommer man hitt 
            if (this.VoteBlank(user, candidate, election))
            {
                TempData["Message"] = "Success,You have voted!";// meddelar att man röstat 

                return RedirectToAction("ElectionsForUsers");
            }

            return RedirectToAction("Index", "Home");
        }

        private bool VoteBlank(Models.User user, Candidate candidate, Election election) // röstnings funktion för att rösta blankt 
        {

            using (var transaction = _electionRepository.Transaction())// kontakt med DB och transaction anbvänds i MVC för att kunna läga till data i flera tabeler 
            {
                var votingDetail = new ElectionVotingDetail
                {
                    CandidateId = candidate.CandidateId,
                    DateTime = DateTime.Now,
                    UserId = user.UserId,
                    ElectionId = election.ElectionId,
                };

                _electionRepository.VotingDetailAdd(votingDetail);

                election.QuantityVotes++;

                election.QuantityBlankVotes++;

                _electionRepository.UpdateElection(election);

                //sparar data i DB
                try
                {
                    _electionRepository.Save();

                    transaction.Commit();
                    return true;
                }
                catch (Exception)// om någto går fel så går DB till baks till inna något ändrats 
                {
                    transaction.Rollback();
                }
            }
            return false;
        }

        //----------------------------------------------------  

        [Authorize(Roles = "User")]
        public ActionResult Vote(int ElectionId)// visar röstnigns Viewn för användar som är inlogad som User
        {
            var voting = _electionRepository.GetElectionById(ElectionId);

            var view = new ElectionVotingView
            {

                DateTimeEnd = voting.DateTimeEnd,
                DateTimeStart = voting.DateTimeStart,
                Description = voting.Description,
                IsEnableBlankVote = voting.IsEnableBlankVote,
                IsForAllUsers = voting.IsForAllUsers,
                MyCandidate = voting.Candidates.ToList(),
                Remarks = voting.Remarks,
                ElectionId = voting.ElectionId,
            };

            ViewBag.IsEnableBlankVote = voting.IsEnableBlankVote;

            var state = _stateRepository.GetStateById(voting.StateId);


            ViewBag.StateDescripcion = state.Descripcion;

            return View(view);
        }


        [Authorize(Roles = "User")]
        public ActionResult ElectionsForUsers()// visar valen som är pågång 
        {
            // söker login
            var user = _userRepository.GetUserByUserEmail(this.User.Identity.Name);


            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "There an error with the current user, call the support");
                return View();
            }


            var state = this.GetState("Open");// hämtar valen som är öpan/ där röstnings tiden gäller 

            var votings = _electionRepository.GetListOfElectionIfOpen(state);// hämtar valen som är öpan, där röstnings tiden gäller 


            //tar bort val där användaren redan röstat  
            foreach (var voting in votings.ToList())
            {


                var votingDetail = _electionRepository.GetIfUserAlreadyVotedInElection(voting.ElectionId, user.UserId);



                if (votingDetail != null)
                {
                    votings.Remove(voting);
                }

            }

            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"].ToString();// visar medelande som tagit med fron myvoting view
            }

            //konroll om val listan är tom 

            ViewBag.empty = false;

                if (votings.Count == 0)
                {
                    ViewBag.empty = true;
                }
   

                return View(votings);
        }



        private State GetState(string stateName)// konrolerar State om valet är öppet eller stängt 
        {
            var state = _stateRepository.GetStateByStateName(stateName);


            if (state == null)
            {
                state = new State
                {
                    Descripcion = stateName,
                };

                _stateRepository.AddState(state);

                _stateRepository.Save();
            }

            return state;
        }

        //----------------------------------------------------  Testar ny AddCandidate

        [Authorize(Roles = "Admin")]
        public ActionResult _SearchAndAddCandidate(int id)// visar en lista på alla användare man kan läga till i ett val 
        {
            List<int> RemoveID = new List<int>();

            var users = _userRepository.GetListOfAllUser();


            for (var i = 0; i < users.Count; i++)
            {

                var UserId = users[i].UserId;

                var candidate = _electionRepository.GetCandidateByElectionIdAndUserId(id, UserId);

                var userASP = _userRepository.GetUserByUserEmailFromASPdb(users[i].UserName);


                if (_userRepository.GetIfUserIsAdminFromASPdb(userASP.Id))
                {
                    RemoveID.Add(i);
                }
                else if (candidate != null)//om canditaten redan fins i valet startat fi statsen som kommer spara index för att sen ta bort den användaren från user listan på Users som kan lägas till i valet 
                {
                    RemoveID.Add(i);
                }
            }

            for (int i = RemoveID.Count - 1; i >= 0; i--)
            {
                users.RemoveAt(RemoveID[i]);
            }



            ViewBag.ElectionId = id;

            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"].ToString();// visar medelande som tagit med fron Edit eller delit view
            }

            return PartialView(users);

        }

        //Post AddCandidate
        [HttpPost]
        public ActionResult _SearchAndAddCandidate(String SearchText, int id)// visar den användare man sökt på 
        {
            List<User> UsersList;

            if (string.IsNullOrEmpty(SearchText))
            {
                UsersList = _userRepository.GetListOfAllUser();

            }
            else
            {
                if (SearchText.Contains(" "))
                {
                    string[] array = SearchText.Split(new char[] { ' ' }, 2);

                    var FirstNameText = array[0];
                    var LastNameText = array[1];

                    UsersList = _userRepository.GetListOfAllUserByFirstNameAndLastName(FirstNameText, LastNameText);

                }
                else
                {
                    UsersList = _userRepository.GetListUserByFirstName(SearchText);// söker efter förnamn man sökt på i DB för att visas i viewn 
                }
            }


            for (var i = 0; i < UsersList.Count; i++)
            {

                var userASP = _userRepository.GetUserByUserEmailFromASPdb(UsersList[i].UserName);


                if (_userRepository.GetIfUserIsAdminFromASPdb(userASP.Id))
                {

                    ViewBag.Admin = 1;
                }
                else
                {
                    ViewBag.Admin = 0;
                }

            }


            ViewBag.VotingId = id.ToString();

            return PartialView("_SearchAndAddCandidate", UsersList);

        }

        public JsonResult GetNameSearch(String term)// funktion som används av autocomplete jquery
        {
            List<String> UsersList;// skapar lista som kommer användas för att spara alla User från DB


            if (term.Contains(" "))
            {
                string[] array = term.Split(new char[] { ' ' }, 2);

                var FirstNameText = array[0];
                var LastNameText = array[1];

                UsersList = _userRepository.AutocompleteListByFirstNameAndLastName(FirstNameText, LastNameText);// söker efter förnamn och efternam man sökt på i DB för att visas på autocomplete 

            }
            else
            {
                UsersList = _userRepository.AutocompleteListByFirstName(term);// söker efter förnamnet man sökt på i DB för att visas på autocomplete
            }


            return Json(UsersList, JsonRequestBehavior.AllowGet);
        }


        //Post AddCandidate
        [HttpPost]
        public ActionResult MakeUserToCandidate(int UserID, int ElectionId, string UserFullName)// postar användare man valt till kandidat
        {


            //så man inte lägger inte samma kandidat två gånger 
            var candidate = _electionRepository.GetCandidateByElectionIdAndUserId(ElectionId, UserID);



            if (candidate != null)// användas för att kontrollera att användare inte läger till samma användare två gånger
            {

                TempData["Message"] = "(" + UserFullName + ") is already a candidate in this election";

                return Json(new { url = Url.Action("Details", new { id = ElectionId }) });


            }

            candidate = new Candidate
            {
                UserId = UserID,
                ElectionId = ElectionId,
            };

            _electionRepository.AddCandidate(candidate);

            _electionRepository.Save();


            TempData["Message"] = "(" + UserFullName + ") is add to this election";

            return Json(new { url = Url.Action("Details", new { id = ElectionId }) });


        }

        //------------------------------------------------------------------------------------------------------


        [Authorize(Roles = "Admin")]
        public ActionResult DeleteCandidate(int id)// tar bort kandidat från valet 
        {
            var candidate = _electionRepository.GetCandidateById(id);


            if (candidate != null)
            {
                _electionRepository.DeleteCandidate(candidate);

                _electionRepository.Save();

            }

            return RedirectToAction(string.Format("Details/{0}", candidate.ElectionId));
        }

        //-----------------test index sök------------------------------------------------------------------------

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult IndexSearch(String SearchText)// gör sökning på val namn i index view och ger resultatet 
        {
            List<Election> ElectionList;

            var views = new List<ElectionIndexView>();

            if (string.IsNullOrEmpty(SearchText))
            {
                ElectionList = _electionRepository.GetListOfAllElections();

            }
            else
            {
                ElectionList = _electionRepository.GetElectionByName(SearchText);// söker efter val namnet man sökt på i DB för att visas i viewn 
            }

            foreach (var Election in ElectionList)
            {
                User user = null;
                if (Election.CandidateWinId != 0)
                {
                    user = _userRepository.GetUserByUserId(Election.CandidateWinId);
                }

                views.Add(new ElectionIndexView
                {
                    CandidateWinId = Election.CandidateWinId,
                    DateTimeEnd = Election.DateTimeEnd,
                    DateTimeStart = Election.DateTimeStart,
                    Description = Election.Description,
                    IsEnableBlankVote = Election.IsEnableBlankVote,
                    IsForAllUsers = Election.IsForAllUsers,
                    QuantityBlankVotes = Election.QuantityBlankVotes,
                    QuantityVotes = Election.QuantityVotes,
                    Remarks = Election.Remarks,
                    StateId = Election.StateId,
                    State = Election.State,
                    ElectionId = Election.ElectionId,
                    Winner = user,

                });
            }


            return PartialView("_ElectionInfo", views);
        }

        public JsonResult GetElectionSearch(String term)// funktion som används av autocomplete jquery i index view för sökning på val namn  
        {
            List<String> ElectionList;// skapar lista som kommer användas för att spara alla User från DB

            ElectionList = _electionRepository.GetElectionByNameForAutocomplete(term);// söker efter val namnet man sökt på i DB för att visas på autocomplete


            return Json(ElectionList, JsonRequestBehavior.AllowGet);
        }

        //-------------testa indexorderby----------------------------

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult IndexOrderBy(String StateId, string SelectedYear, string SelectedMonths)// visar index view meda alla valen för admin och om den är avslutad så visas vinare
        {
            if (SelectedMonths == "0")
            {
                SelectedMonths = "";
            }

            List<Election> ElectionList;

            int MonthsNum = 0;


            var SMonths = 0;

            if (SelectedMonths != "")
            {
                SMonths = Int32.Parse(SelectedMonths);

                foreach (var Months in TempData["List"] as List<MonthsList>)
                {
                    if (SMonths == Months.MonthsID)
                    {
                        MonthsNum = Months.Months;
                    }
                }
            }

            int SID = 0;
            int Syear = 0;
            State state = new State();


            if (StateId != "" & SelectedYear != "" & SelectedMonths != "")
            {
                SID = Int32.Parse(StateId);
                Syear = Int32.Parse(SelectedYear);

                state.StateId = SID;

                ElectionList = _electionRepository.GetElectionByYearMonthsAndStateId(Syear, MonthsNum, state);// söker efter år, månade och status på valet i DB för att visas i viewn 
            }
            else if (StateId != "" & SelectedYear == "" & SelectedMonths == "")
            {
                SID = Int32.Parse(StateId);

                state.StateId = SID;
                ElectionList = _electionRepository.GetElectionByStateId(state);

            }
            else if (StateId == "" & SelectedYear != "" & SelectedMonths != "")
            {
                Syear = Int32.Parse(SelectedYear);
                ElectionList = _electionRepository.GetElectionByYearandMonths(Syear, MonthsNum);

            }
            else if (StateId != "" & SelectedYear != "" & SelectedMonths == "")
            {
                SID = Int32.Parse(StateId);
                Syear = Int32.Parse(SelectedYear);
                state.StateId = SID;
                ElectionList = _electionRepository.GetElectionByYearAndStateId(Syear, state);

            }
            else if (StateId == "" & SelectedYear != "" & SelectedMonths == "")
            {
                Syear = Int32.Parse(SelectedYear);
                ElectionList = _electionRepository.GetElectionByYear(Syear);

            }
            else if (StateId == "" & SelectedYear == "" & SelectedMonths != "")
            {
                ElectionList = _electionRepository.GetElectionByMonths(MonthsNum);

            }
            else
            {
                ElectionList = _electionRepository.GetListOfAllElections();

            }

            var views = new List<ElectionIndexView>();

            foreach (var Election in ElectionList)
            {
                User user = null;
                if (Election.CandidateWinId != 0)
                {
                    user = _userRepository.GetUserByUserId(Election.CandidateWinId);
                }

                views.Add(new ElectionIndexView
                {
                    CandidateWinId = Election.CandidateWinId,
                    DateTimeEnd = Election.DateTimeEnd,
                    DateTimeStart = Election.DateTimeStart,
                    Description = Election.Description,
                    IsEnableBlankVote = Election.IsEnableBlankVote,
                    IsForAllUsers = Election.IsForAllUsers,
                    QuantityBlankVotes = Election.QuantityBlankVotes,
                    QuantityVotes = Election.QuantityVotes,
                    Remarks = Election.Remarks,
                    StateId = Election.StateId,
                    State = Election.State,
                    ElectionId = Election.ElectionId,
                    Winner = user,

                });
            }

            TempData["List"] = TempData["List"];


            return PartialView("_ElectionInfo", views);

        }

        //-----------------------------------------------------------------------------------------

        [Authorize(Roles = "Admin")]
        public ActionResult Index()// visar index view meda alla valen för admin och om den är avslutad så visas vinare
        {
            var votings = _electionRepository.GetListOfAllElections();

            var views = new List<ElectionIndexView>();



            var Yearlist = new List<string>();

            //visar info om valet och visar också vinare om vallet är slut förd 
            foreach (var voting in votings)
            {
                User user = null;
                if (voting.CandidateWinId != 0)
                {
                    user = _userRepository.GetUserByUserId(voting.CandidateWinId);

                }

                views.Add(new ElectionIndexView
                {
                    CandidateWinId = voting.CandidateWinId,
                    DateTimeEnd = voting.DateTimeEnd,
                    DateTimeStart = voting.DateTimeStart,
                    Description = voting.Description,
                    IsEnableBlankVote = voting.IsEnableBlankVote,
                    IsForAllUsers = voting.IsForAllUsers,
                    QuantityBlankVotes = voting.QuantityBlankVotes,
                    QuantityVotes = voting.QuantityVotes,
                    Remarks = voting.Remarks,
                    StateId = voting.StateId,
                    State = voting.State,
                    ElectionId = voting.ElectionId,
                    Winner = user,

                });

                var year = voting.DateTimeStart.ToString("yyyy");

                Yearlist.Add(year);
            }

            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"].ToString();// visar medelande som tagit med fron Edit view
            }

            //-----------------------------dropdown list

            Yearlist = Yearlist.Distinct().ToList();

            ViewBag.SelectedYear = new SelectList(Yearlist);

            ViewBag.StateId = new SelectList(_stateRepository.GetStateTb(), "StateId", "Descripcion");


            var Monthslist = new List<MonthsList>();

            ViewBag.SelectedMonths = new SelectList(Monthslist, "MonthsID", "Months");

            //-----------------------------------------

            return View(views);
        }

        //------------------------------------------ test månad 


        //[HttpPost]
        public JsonResult FetchMonths(int selectedYear)
        {

            int Year = selectedYear; //Int32.Parse(selectedYear);

            var votings = _electionRepository.GetElectionByYear(Year);


            var MontsListsOfYear = new List<MonthsList>();

            var MontsList = new List<int>();

            //visar info om valet och visar också vinare om vallet är slut förd 
            foreach (var voting in votings)
            {


                var Months = voting.DateTimeStart.ToString("MM");
                MontsList.Add(Int32.Parse(Months));

            }

            MontsList = MontsList.Distinct().ToList();
            MontsList.Add(0);
            MontsList.Sort();
            int i = 0;

            foreach (var M in MontsList)
            {

                MontsListsOfYear.Add(new MonthsList
                {
                    MonthsID = i++,
                    Months = M,
                });



            }

            TempData["List"] = MontsListsOfYear.OrderBy(s => s.Months).ToList();


            return Json(MontsListsOfYear, JsonRequestBehavior.AllowGet);
        }

        //-------------------------------------------


        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)// visar detaljerad info om valet
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Election voting = _electionRepository.GetElectionById(id.GetValueOrDefault());

            if (voting == null)
            {
                return HttpNotFound();
            }

            var state = _stateRepository.GetStateById(voting.StateId);


            if ("Closed" != state.Descripcion)
            {

                var view = new ElectionDetailsView
                {
                    Candidates = voting.Candidates.ToList(),
                    CandidateWinId = voting.CandidateWinId,
                    DateTimeEnd = voting.DateTimeEnd,
                    DateTimeStart = voting.DateTimeStart,
                    Description = voting.Description,
                    IsEnableBlankVote = voting.IsEnableBlankVote,
                    IsForAllUsers = voting.IsForAllUsers,
                    QuantityBlankVotes = voting.QuantityBlankVotes,
                    QuantityVotes = voting.QuantityVotes,
                    Remarks = voting.Remarks,
                    StateId = voting.StateId,
                    ElectionId = voting.ElectionId,
                };




                ViewBag.StateDescripcion = state.Descripcion;

                return View(view);
            }
            else
            {
                TempData["Message"] = "This election(" + voting.Description + ") is finished,  you can not add candidate anymore!  ";
                return RedirectToAction("Index");
            }
        }



        [Authorize(Roles = "Admin")]
        public ActionResult Create()// visar view där man skapar valet 
        {
            var S1 = _stateRepository.GetAllState();


            foreach (var item in S1)// gör så att drop down listan som visas i skapar viewn får Open state vald 
            {

                if ("Open" == item.Descripcion)
                {
                    ViewBag.StateId = new SelectList(_stateRepository.GetStateTb(), "StateId", "Descripcion", item.StateId);

                }

            }

            //DateTime
            var view = new ElectionCreateEditView
            {
                DateStart = DateTime.Now,
                DateEnd = DateTime.Now,
            };



            return View(view);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ElectionCreateEditView view)// postar valet man skapat 
        {
            if (ModelState.IsValid)
            {
                //DateTime
                var voting = new Election
                {
                    DateTimeEnd = view.DateEnd
                                  .AddHours(view.TimeEnd.Hour)
                                  .AddMinutes(view.TimeEnd.Minute),
                    DateTimeStart = view.DateStart
                                  .AddHours(view.TimeStart.Hour)
                                  .AddMinutes(view.TimeStart.Minute),
                    Description = view.Description,
                    IsEnableBlankVote = view.IsEnabledBlankVote,
                    IsForAllUsers = view.IsForAllUsers,
                    Remarks = view.Remarks,
                    StateId = view.StateId,
                };


                _electionRepository.AddElection(voting);

                _electionRepository.Save();



                return RedirectToAction("Details", new { id = voting.ElectionId });
            }

            ViewBag.StateId = new SelectList(_stateRepository.GetStateTb(), "StateId", "Descripcion", view.StateId);


            return View(view);
        }



        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)// visar view för att ändra info om valet 
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var voting = _electionRepository.GetElectionById(id.GetValueOrDefault());


            if (voting == null)
            {
                return HttpNotFound();
            }

            var state = _stateRepository.GetStateById(voting.StateId);


            if ("Open" == state.Descripcion)// används för att kontrollera om ett val är på gong eller avslutad, är ett val avslutat så ska man inte kunna ändra något i valt, detta är en funktion som lags till för att bevara valets integritet
            {
                //DateTime
                var FixTimeStart = voting.DateTimeStart.ToString("HH:mm");//får tiden från datetime objekt 
                var FixTimeEnd = voting.DateTimeEnd.ToString("HH:mm");// får tiden från datetime objekt 

                var V1 = _electionRepository.GetElectionByIdNoTracking(voting.ElectionId);


                var view = new ElectionCreateEditView
                {


                    CandidateWinId = voting.CandidateWinId,
                    DateEnd = voting.DateTimeEnd.Date,
                    DateStart = voting.DateTimeStart.Date,
                    TimeStart = DateTime.Parse(FixTimeStart, System.Globalization.CultureInfo.CurrentCulture),// start tid
                    TimeEnd = DateTime.Parse(FixTimeEnd, System.Globalization.CultureInfo.CurrentCulture),//slut tid 
                    Description = voting.Description,
                    IsEnabledBlankVote = voting.IsEnableBlankVote,
                    IsForAllUsers = voting.IsForAllUsers,
                    QuantityBlankVotes = V1.QuantityBlankVotes,
                    QuantityVotes = V1.QuantityVotes,
                    Remarks = voting.Remarks,
                    StateId = voting.StateId,
                    ElectionId = voting.ElectionId,

                };


                ViewBag.StateId = new SelectList(_stateRepository.GetStateTb(), "StateId", "Descripcion", voting.StateId);

                return View(view);
            }
            else
            {
                TempData["Message"] = "You tried to Edit (" + voting.Description + "), This election is finished and can not be edited anymore!";
                return RedirectToAction("Index", "votings");
            }


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ElectionCreateEditView view)// postar ändringarna man gjort i valets info
        {


            var V1 = _electionRepository.GetElectionByIdNoTracking(view.ElectionId);


            var state = _stateRepository.GetStateById(V1.StateId);


            // används här i den här post funktionen för att hindra post attacker som kan göras genom URL, man postar ändrignar som int ska gå att göras
            if ("Open" == state.Descripcion & V1.QuantityVotes == view.QuantityVotes & V1.QuantityBlankVotes == view.QuantityBlankVotes & V1.CandidateWinId == view.CandidateWinId)// används för att kontrollera om ett val är på gong eller avslutad, är ett val avslutat så ska man inte kunna ändra något i valt, detta är en funktion som lags till för att bevara valets integritet
            {
                if (ModelState.IsValid)
                {

                    TimeSpan timeOfEnd = view.TimeEnd.TimeOfDay;// kommer användas för att längre ner slå ihop tid och datume 
                    TimeSpan timeOfStart = view.TimeStart.TimeOfDay;// kommer användas för att längre ner slå ihop tid och datume 

                    //DateTime
                    var voting = new Election
                    {


                        CandidateWinId = view.CandidateWinId,
                        DateTimeEnd = view.DateEnd.Add(timeOfEnd),// slåtr ihop tid i datetime objekt 
                        DateTimeStart = view.DateStart.Add(timeOfStart),// slåtr ihop tid i datetime objekt 
                        Description = view.Description,
                        IsEnableBlankVote = view.IsEnabledBlankVote,
                        IsForAllUsers = view.IsForAllUsers,
                        QuantityBlankVotes = view.QuantityBlankVotes,
                        QuantityVotes = view.QuantityVotes,
                        Remarks = view.Remarks,
                        StateId = view.StateId,
                        ElectionId = view.ElectionId,

                    };

                    _electionRepository.UpdateElection(voting);
                    _electionRepository.Save();

                    return RedirectToAction("Details", new { id = view.ElectionId });
                }

                ViewBag.StateId = new SelectList(_stateRepository.GetStateTb(), "StateId", "Descripcion", view.StateId);

                return View(view);

            }
            else
            {
                TempData["Message"] = "You tried to use the URL to post Edit (" + V1.Description + "), This election is finished and can not be edited anymore!";
                return RedirectToAction("Index", "votings");
            }

        }

        //--------------testar edit----------------------------------------------
        [Authorize(Roles = "Admin")]
        public ActionResult _EditElectionInfoOnline(int? id)// visar view för att ändra info om valet 
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var voting = _electionRepository.GetElectionById(id.GetValueOrDefault());


            if (voting == null)
            {
                return HttpNotFound();
            }

            var state = _stateRepository.GetStateById(voting.StateId);


            if ("Open" == state.Descripcion)// används för att kontrollera om ett val är på gong eller avslutad, är ett val avslutat så ska man inte kunna ändra något i valt, detta är en funktion som lags till för att bevara valets integritet
            {
                //DateTime
                var FixTimeStart = voting.DateTimeStart.ToString("HH:mm");//får tiden från datetime objekt 
                var FixTimeEnd = voting.DateTimeEnd.ToString("HH:mm");// får tiden från datetime objekt 

                var V1 = _electionRepository.GetElectionByIdNoTracking(voting.ElectionId);


                var view = new ElectionCreateEditView
                {

                    CandidateWinId = voting.CandidateWinId,
                    DateEnd = voting.DateTimeEnd.Date,
                    DateStart = voting.DateTimeStart.Date,
                    TimeStart = DateTime.Parse(FixTimeStart, System.Globalization.CultureInfo.CurrentCulture),// start tid
                    TimeEnd = DateTime.Parse(FixTimeEnd, System.Globalization.CultureInfo.CurrentCulture),//slut tid 
                    Description = voting.Description,
                    IsEnabledBlankVote = voting.IsEnableBlankVote,
                    IsForAllUsers = voting.IsForAllUsers,
                    QuantityBlankVotes = V1.QuantityBlankVotes,
                    QuantityVotes = V1.QuantityVotes,
                    Remarks = voting.Remarks,
                    StateId = voting.StateId,
                    ElectionId = voting.ElectionId,

                };

                ViewBag.StateId = new SelectList(_stateRepository.GetStateTb(), "StateId", "Descripcion", voting.StateId);

                return PartialView("_EditElectionInfo", view);
            }
            else
            {
                TempData["Message"] = "You tried to Edit (" + voting.Description + "), This election is finished and can not be edited anymore!";
                return Json(new { url = Url.Action("Index", "Elections") });
            }


        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult _EditElectionInfo(ElectionCreateEditView view)// postar ändringarna man gjort i valets info // VotingView view
        {
            ViewBag.VotingId = view.ElectionId;

            // NoTracking används för att inte Entity Framework inte ska binda sig till state modelen så att den längre ner kan updateras utan problem
            var V1 = _electionRepository.GetElectionByIdNoTracking(view.ElectionId);

            var state = _stateRepository.GetStateById(V1.StateId);


            // används här i den här post funktionen för att hindra post attacker som kan göras genom URL, man postar ändrignar som int ska gå att göras
            if ("Open" == state.Descripcion & V1.QuantityVotes == view.QuantityVotes & V1.QuantityBlankVotes == view.QuantityBlankVotes & V1.CandidateWinId == view.CandidateWinId)// används för att kontrollera om ett val är på gong eller avslutad, är ett val avslutat så ska man inte kunna ändra något i valt, detta är en funktion som lags till för att bevara valets integritet
            {
                if (ModelState.IsValid)
                {

                    TimeSpan timeOfEnd = view.TimeEnd.TimeOfDay;// kommer användas för att längre ner slå ihop tid och datume 
                    TimeSpan timeOfStart = view.TimeStart.TimeOfDay;// kommer användas för att längre ner slå ihop tid och datume 

                    //DateTime
                    var voting = new Election
                    {

                        CandidateWinId = view.CandidateWinId,
                        DateTimeEnd = view.DateEnd.Add(timeOfEnd),// slåtr ihop tid i datetime objekt 
                        DateTimeStart = view.DateStart.Add(timeOfStart),// slåtr ihop tid i datetime objekt 
                        Description = view.Description,
                        IsEnableBlankVote = view.IsEnabledBlankVote,
                        IsForAllUsers = view.IsForAllUsers,
                        QuantityBlankVotes = view.QuantityBlankVotes,
                        QuantityVotes = view.QuantityVotes,
                        Remarks = view.Remarks,
                        StateId = view.StateId,
                        ElectionId = view.ElectionId,

                    };

                    _electionRepository.UpdateElection(voting);
                    _electionRepository.Save();

                    //--------------------------------- det ska flytas ut till egen ActionResult metod


                    List<Election> ElectionList;

                    var views = new List<ElectionIndexView>();

                    int ID = view.ElectionId;

                    ElectionList = _electionRepository.GetListOfAllElectionsById(ID);


                    foreach (var Election in ElectionList)
                    {
                        User user = null;
                        if (Election.CandidateWinId != 0)
                        {
                            user = _userRepository.GetUserByUserId(Election.CandidateWinId);

                        }

                        views.Add(new ElectionIndexView
                        {
                            CandidateWinId = Election.CandidateWinId,
                            DateTimeEnd = Election.DateTimeEnd,
                            DateTimeStart = Election.DateTimeStart,
                            Description = Election.Description,
                            IsEnableBlankVote = Election.IsEnableBlankVote,
                            IsForAllUsers = Election.IsForAllUsers,
                            QuantityBlankVotes = Election.QuantityBlankVotes,
                            QuantityVotes = Election.QuantityVotes,
                            Remarks = Election.Remarks,
                            StateId = Election.StateId,
                            State = Election.State,
                            ElectionId = Election.ElectionId,
                            Winner = user,

                        });
                    }


                    return PartialView("_ElectionAfterEdit", views);

                    //-------------------------------------------------------------------------

                }

                ViewBag.StateId = new SelectList(_stateRepository.GetStateTb(), "StateId", "Descripcion", view.StateId);

                return PartialView("_EditElectionInfo", view);

            }
            else
            {
                TempData["Message"] = "You tried to use the URL to post Edit (" + V1.Description + "), This election is finished and can not be edited anymore!";

                return Json(new { url = Url.Action("Index", "Votings") });

            }

        }


        //-----------------------------------------------------------------------

        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)// visar view där man kan ta bort valet 
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Election voting = _electionRepository.GetElectionById(id.GetValueOrDefault());


            if (voting == null)
            {
                return HttpNotFound();
            }
            return View(voting);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)// postar att man tar bort valet 
        {
            Election voting = _electionRepository.GetElectionById(id);

            _electionRepository.DeleteElection(voting);



            try
            {
                _electionRepository.Save();

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

                return View(voting);
            }


            return RedirectToAction("Index");
        }

    }
}
