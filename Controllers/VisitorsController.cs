using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SocietyManagementMVC.Data;
using SocietyManagementMVC.Models;

namespace SocietyManagementMVC.Controllers
{
    public class VisitorsController : BaseController
    {

        public VisitorsController(ApplicationDbContext db) : base(db)
        {
             
        }
        public IActionResult Index()
        {
            var email = HttpContext.Session.GetString("email");
            if (email == "Admin@gmail.com")
            {
                var data = db.Visitors.ToList();
                return View(data);
            }
            var flatnumber = db.Allotments.Where(x => x.AllotedTo == email).Select(x => x.FlatNumber).FirstOrDefault();
            var UVdata = db.Visitors.Where(x => x.FlatNumber == flatnumber).ToList();
            return View(UVdata);
        }


        public IActionResult Create()
        {
            ViewBag.FlatNumbers = new SelectList(db.Allotments, "FlatNumber", "FlatNumber");
            return View();
        }

        [HttpPost]
        public IActionResult Create(Visitor v)
        {

            var email = db.Allotments.Where(x => x.FlatNumber == v.FlatNumber).Select(x => x.AllotedTo).FirstOrDefault();
            v.Status = "In";
            v.CreatedAtDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            db.Visitors.Add(v);

            Notification notif = new Notification()
            {
                NotificationName = "Visitor",
                NotificationFor = email,
                FlatNumber = v.FlatNumber,
                NotificationMessage = v.VisitorName + "; Wants to Meet",
                Status = "In",
                Seen = "Not Seen"
            };

            db.Notifications.Add(notif);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var data = db.Visitors.Find(id);
            ViewBag.FlatNumbers = new SelectList(db.Allotments, "FlatNumber", "FlatNumber");
            return View(data);
        }

        [HttpPost]
        public IActionResult Edit(Visitor v)
        {

            db.Visitors.Update(v);
            v.Status = "Out";
            var notifiData = db.Notifications.Where(x => x.FlatNumber == v.FlatNumber && x.NotificationMessage == v.VisitorName + "; Wants to Meet" && x.Status == "In").FirstOrDefault();
            notifiData.Status = "Out";
            notifiData.NotificationMessage = v.VisitorName + "; Leaved";
          
            db.Notifications.Update(notifiData);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var vdata = db.Visitors.Find(id);
            if (vdata != null)
            {
                var notidata = db.Notifications.Where(x => x.FlatNumber == vdata.FlatNumber && x.NotificationMessage == vdata.VisitorName + "; Wants to Meet" && x.Status == "In" && x.Seen == "Not Seen").FirstOrDefault();
                db.Notifications.Remove(notidata);
                db.Visitors.Remove(vdata);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index");
            }

        }

        public IActionResult Details(int id)
        {
            var data = db.Visitors.Find(id);
            if (data == null)
            {
                var notifdata = db.Notifications.Find(id);
                if (notifdata.Seen == "Not Seen" && notifdata.Status == "Out")
                {
                    notifdata.Seen = "Seen";
                    db.Notifications.Update(notifdata);
                    db.SaveChanges();
                }
                var MessageDetails = notifdata.NotificationMessage.Split(";");
                var visitorName = MessageDetails[0];
                var Vdata = db.Visitors.Where(x => x.FlatNumber == notifdata.FlatNumber && x.VisitorName == visitorName && x.Status == notifdata.Status).FirstOrDefault();
                return View(Vdata);
            }
            return View(data);
        }
    }
}
