using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SocietyManagementMVC.Data;
using SocietyManagementMVC.Models;
using System.Diagnostics;

namespace SocietyManagementMVC.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        //private readonly ApplicationDbContext db;

        public HomeController(ILogger<HomeController> logger,ApplicationDbContext db) : base(db)
        {
            _logger = logger;
            //this.db = db;
        }

        public IActionResult Index()
        {
            ViewBag.TotalFlats = db.Flats.Count();
            ViewBag.TotalBills = db.Bills.Count();
            ViewBag.TotalAllotments = db.Allotments.Count();
            ViewBag.TotalVisitors = db.Visitors.Count();
            ViewBag.TotalComplaints = db.Complaints.Count();
            ViewBag.TotalResolvedComplaints = db.Complaints.Where(x => x.Status == "Resolved").Count();
            ViewBag.UnResolvedComplaints = db.Complaints.Where(x => x.Status == "Pending").Count();
            ViewBag.InProgressComplaints = db.Complaints.Where(x => x.Status == "In Process").Count();
            return View();
        }


        public IActionResult login()
        {

            return View();   
        }

        [HttpPost]
        public IActionResult login(string email, string password)
        {
            var role = db.Users.Where(x => x.Email.Equals(email) && x.Password.Equals(password)).Select(x => x.Urole).FirstOrDefault();
            var userName = db.Users.Where(x => x.Email.Equals(email) && x.Password.Equals(password)).Select(x => x.UserName).FirstOrDefault();

            if (role != null)
            {
                //ViewBag.Notifications = notifications;
                //HttpContext.Session.SetString("Notifications", JsonConvert.SerializeObject(notifications));
                HttpContext.Session.SetString("Role", role);
                HttpContext.Session.SetString("email", email);
                HttpContext.Session.SetString("UserName", userName);
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("login");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
