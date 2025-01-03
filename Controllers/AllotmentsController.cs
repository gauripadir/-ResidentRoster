using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SocietyManagementMVC.Data;
using SocietyManagementMVC.Models;

namespace SocietyManagementMVC.Controllers
{
    public class AllotmentsController : BaseController
    {
        public AllotmentsController(ApplicationDbContext db) : base(db) 
        {
            
        }
        public IActionResult Index()
        {
            var data = db.Allotments.ToList();
            return View(data);
        }

        public IActionResult Create()
        {
            // Get all users that are not already allotted a flat (where AllotedTo != Email)
            var availableUsers = db.Users
                .Where(u => u.Email != "Admin@gmail.com" && !db.Allotments.Any(a => a.AllotedTo == u.Email))  // Users not allotted a flat
                .Select(u => new { u.Id, u.UserName, u.Email })
                .ToList();

            // Populate ViewBag.Users with email as value and username as text
            ViewBag.Users = new SelectList(availableUsers, "Email", "UserName");

            // Get all flats that are not already allotted (where FlatNumber not in Allotments)
            var availableFlats = db.Flats
                .Where(f => !db.Allotments.Any(a => a.FlatNumber == f.BlockNumber + "-" + f.FlatNumber))  // Flats not allotted
                .Select(f => new
                {
                    f.FlatNumber,
                    DisplayText = f.BlockNumber + "-" + f.FlatNumber  // Concatenate BlockNumber and FlatNumber
                })
                .ToList();

            // Populate ViewBag.Flats with concatenated BlockNumber and FlatNumber
            ViewBag.Flats = new SelectList(availableFlats, "DisplayText", "DisplayText");

            return View();
        }

        [HttpPost]
        public IActionResult Create(Allotment a)
        {
            var flatdetails = a.FlatNumber.Split("-");
            var flattype = db.Flats.Where(x => x.FlatNumber == flatdetails[1] && x.BlockNumber ==  flatdetails[0]).Select(x => x.Type).FirstOrDefault();
            a.Type = flattype;
            db.Allotments.Add(a);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var allotment = db.Allotments.Find(id);
            var username = db.Users.Where(x => x.Email == allotment.AllotedTo).Select(x => x.UserName).FirstOrDefault();
            // Get all users that are not already allotted a flat (where AllotedTo != Email)
            var availableUsers = db.Users
                .Where(u => u.Email != "Admin@gmail.com" && !db.Allotments.Any(a => a.AllotedTo == u.Email))  // Users not allotted a flat
                .Select(u => new { u.UserName, u.Email })
                .ToList();

            availableUsers.Add(new {UserName = username,Email = allotment.AllotedTo });
            // Populate ViewBag.Users with email as value and username as text
            ViewBag.Users = new SelectList(availableUsers, "Email", "UserName");

            // Get all flats that are not already allotted (where FlatNumber not in Allotments)
            var availableFlats = db.Flats
                .Where(f => !db.Allotments.Any(a => a.FlatNumber == f.BlockNumber + "-" + f.FlatNumber))  // Flats not allotted
                .Select(f => new
                {
                    f.FlatNumber,
                    DisplayText = f.BlockNumber + "-" + f.FlatNumber  // Concatenate BlockNumber and FlatNumber
                })
                .ToList();

            availableFlats.Add(new
            {
                FlatNumber = allotment.FlatNumber,
                DisplayText = allotment.FlatNumber
            });

            // Populate ViewBag.Flats with concatenated BlockNumber and FlatNumber
            ViewBag.Flats = new SelectList(availableFlats, "DisplayText", "DisplayText");

            var data = db.Allotments.Find(id);
            return View(data);
        }

        [HttpPost]
        public IActionResult Edit(Allotment a)
        {
            var oldFlatnumber = db.Allotments.Where(x => x.Id == a.Id).Select(x => x.FlatNumber).FirstOrDefault();
            var flatdetails = a.FlatNumber.Split("-");
            var flattype = db.Flats.Where(x => x.FlatNumber == flatdetails[1] && x.BlockNumber == flatdetails[0]).Select(x => x.Type).FirstOrDefault();
            a.Type = flattype;

            if (oldFlatnumber != null)
            {

                var billData = db.Bills.Where(x => x.FlatNumber == oldFlatnumber).ToList();
                foreach (var bill in billData)
                {
                    bill.FlatNumber = a.FlatNumber;
                }

                var complaintData = db.Complaints.Where(x => x.FlatNumber == oldFlatnumber).ToList();
                foreach (var complaint in complaintData)
                {
                    complaint.UserName = a.AllotedTo;
                    complaint.FlatNumber = a.FlatNumber;
                }

                var visitorData = db.Visitors.Where(x => x.FlatNumber == oldFlatnumber).ToList();

                foreach (var visitor in visitorData)
                {
                    visitor.FlatNumber = a.FlatNumber;
                }

                var notificationData = db.Notifications.Where(x => x.FlatNumber == oldFlatnumber).ToList();

                foreach (var notification in notificationData)
                {
                    if (notification.Status != "Pending")
                    {
                        notification.NotificationFor = a.AllotedTo;
                        notification.FlatNumber = a.FlatNumber;
                    }
                }

            }

            db.Allotments.Update(a);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var allotData = db.Allotments.Find(id);
            if (allotData != null)
            {
                var billData = db.Bills.Where(x => x.FlatNumber == allotData.FlatNumber).ToList();


                var complaintData = db.Complaints.Where(x => x.FlatNumber == allotData.FlatNumber).ToList();


                var visitorData = db.Visitors.Where(x => x.FlatNumber == allotData.FlatNumber).ToList();


                var notificationData = db.Notifications.Where(x => x.FlatNumber == allotData.FlatNumber).ToList();

                db.Bills.RemoveRange(billData);
                db.Complaints.RemoveRange(complaintData);
                db.Visitors.RemoveRange(visitorData);
                db.Notifications.RemoveRange(notificationData);

            }
            db.Allotments.Remove(allotData);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


    }
}
