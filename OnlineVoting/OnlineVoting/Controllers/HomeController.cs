using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineVoting.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"].ToString();// visar medelande som tagit med fron Edit view
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}