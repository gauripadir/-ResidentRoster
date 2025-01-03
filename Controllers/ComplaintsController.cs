using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyManagementMVC.Data;
using SocietyManagementMVC.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SocietyManagementMVC.Controllers
{
    public class ComplaintsController : BaseController
    {
        public ComplaintsController(ApplicationDbContext db) : base(db) 
        {
            
        }
        public IActionResult Index()
        {
            var email = HttpContext.Session.GetString("email");
            if (email == "Admin@gmail.com")
            {
                var data = db.Complaints.ToList();
                return View(data);
            }
            var flatnumber = db.Allotments.Where(x => x.AllotedTo == email).Select(x => x.FlatNumber).FirstOrDefault();
            var UCdata = db.Complaints.Where(x => x.FlatNumber == flatnumber).ToList();
            return View(UCdata);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Complaint c)
        {
            var email = HttpContext.Session.GetString("email");
            var flatnumber = db.Allotments.Where(x => x.AllotedTo == email).Select(x => x.FlatNumber).FirstOrDefault();
            c.UserName = email;
            c.FlatNumber = flatnumber;
            c.Status = "Pending";
            c.CreatedAtDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            db.Complaints.Add(c);

            Notification notif = new Notification()
            {
                NotificationName = "Complaint",
                NotificationFor = "Admin@gmail.com",
                FlatNumber = flatnumber,
                NotificationMessage = c.ComplaintDescription,
                Status = "Pending",
                Seen = "Not Seen"
            };

            db.Notifications.Add(notif);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        public IActionResult Edit(int cid)
        {
            
            if (HttpContext.Session.GetString("Role") == "User")
            {
                var data = db.Complaints.Find(cid);
                //ViewBag.ComplaintDescription = data.ComplaintDescription;
                return View(data);
            }
            if (HttpContext.Session.GetString("Role") == "Admin")
            {
                var complaintdata = db.Complaints.Find(cid);
                if (complaintdata != null)
                {
                    return View(complaintdata);
                }

                var notifdata = db.Notifications.Find(cid);
                var cdata = db.Complaints.Where(x => x.FlatNumber == notifdata.FlatNumber && x.ComplaintDescription == notifdata.NotificationMessage && x.Status == notifdata.Status).FirstOrDefault();
               
                return View(cdata);
            }
            return View();
        }

        [HttpPost]
        public IActionResult Edit(Complaint c)
        {
            if (HttpContext.Session.GetString("Role") == "User")
            {
                var oldDescription  = db.Complaints.Where(x => x.Id == c.Id).Select(x => x.ComplaintDescription).FirstOrDefault();
                var notificationId = db.Notifications.Where(x => x.FlatNumber == c.FlatNumber && x.NotificationMessage == oldDescription && x.Status == "Pending").Select(x => x.Id).FirstOrDefault();
                var notificationData = db.Notifications.Find(notificationId);
                notificationData.NotificationMessage = c.ComplaintDescription;

                db.Notifications.Update(notificationData);
                db.Complaints.Update(c);
                db.SaveChanges();

            }
            if (HttpContext.Session.GetString("Role") == "Admin")
            {
                if (c != null)
                {
                    var oldStatus = db.Complaints.Where(x => x.Id == c.Id).Select(x => x.Status).FirstOrDefault();
                    if (c.Status == "In Process")
                    {
                        var notif = db.Notifications.Where(x => x.NotificationFor == "Admin@gmail.com" && x.FlatNumber == c.FlatNumber && x.Status == oldStatus).FirstOrDefault();
                        notif.NotificationFor = c.UserName;
                        notif.Status = c.Status;
                        db.Notifications.Update(notif);
                        db.Complaints.Update(c);
                        db.SaveChanges();
                    }
                    if (c.Status == "Resolved")
                    {
                        Notification notif = new Notification();
                        if (oldStatus == "Pending")
                        {
                            notif = db.Notifications.Where(x => x.NotificationFor == "Admin@gmail.com" && x.FlatNumber == c.FlatNumber && x.Status == oldStatus).FirstOrDefault();
                        }
                        if (oldStatus == "In Process")
                        {
                            notif = db.Notifications.Where(x => x.NotificationFor == c.UserName && x.FlatNumber == c.FlatNumber && x.Status == oldStatus).FirstOrDefault();
                        }
                        notif.NotificationFor = c.UserName;
                        notif.Status = c.Status;
                        db.Notifications.Update(notif);
                        db.Complaints.Update(c);
                        db.SaveChanges();
                    }
                    

                }
            }
                return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var cdata = db.Complaints.Find(id);
            if (cdata != null)
            {
                var notidata = db.Notifications.Where(x => x.FlatNumber == cdata.FlatNumber && x.NotificationMessage == cdata.ComplaintDescription && x.Seen == "Not Seen").FirstOrDefault();
                db.Notifications.Remove(notidata);
                db.Complaints.Remove(cdata);
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
            var data = db.Complaints.Find(id);
            if (data == null)
            {
                var notifdata = db.Notifications.Find(id);
                if (notifdata.Seen == "Not Seen" && notifdata.Status == "Resolved")
                {
                    notifdata.Seen = "Seen";
                    db.Notifications.Update(notifdata);
                    db.SaveChanges();
                }
                var cdata = db.Complaints.Where(x => x.FlatNumber == notifdata.FlatNumber && x.ComplaintDescription == notifdata.NotificationMessage && x.Status == notifdata.Status).FirstOrDefault();
                return View(cdata);
            }
            return View(data);
        }
    }
}
