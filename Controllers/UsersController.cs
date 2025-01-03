using Microsoft.AspNetCore.Mvc;
using SocietyManagementMVC.Data;
using SocietyManagementMVC.Models;

namespace SocietyManagementMVC.Controllers
{
    public class UsersController : BaseController
    {

        public UsersController(ApplicationDbContext db) : base(db) 
        {
            
        }

        public IActionResult Index()
        {
            var data = db.Users.ToList();
            return View(data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(User u)
        {
            if (u != null)
            {
                u.Urole = "User";
                db.Users.Add(u);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Edit(int id)
        {
            var data = db.Users.Find(id);
            return View(data);
        }

        [HttpPost]
        public IActionResult Edit(User u)
        {
            if (u != null)
            {
                db.Users.Update(u);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }


        public IActionResult Delete(int id)
        {
            var data = db.Users.Find(id);
            if (data != null)
            {
                var FlatNumber = db.Allotments.Where(x => x.AllotedTo == data.Email).Select(x => x.FlatNumber).FirstOrDefault();
                if (FlatNumber != null)
                {
                    var DBills = db.Bills.Where(x => x.FlatNumber == FlatNumber).ToList();
                    db.Bills.RemoveRange(DBills);

                    var DCompliants = db.Complaints.Where(x => x.FlatNumber == FlatNumber).ToList();
                    db.Complaints.RemoveRange(DCompliants);

                    var DVisitors = db.Visitors.Where(x => x.FlatNumber == FlatNumber).ToList();
                    db.Visitors.RemoveRange(DVisitors);

                    var DNotifications = db.Notifications.Where(x => x.FlatNumber == FlatNumber).ToList();
                    db.Notifications.RemoveRange(DNotifications);

                    var DAllotments = db.Allotments.Where(x => x.FlatNumber == FlatNumber).ToList();
                    db.Allotments.RemoveRange(DAllotments);
                }
                db.Users.Remove(data);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index");
            }

        }
    }
}
