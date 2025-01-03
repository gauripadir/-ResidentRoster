using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SocietyManagementMVC.Data;
using SocietyManagementMVC.Models;

namespace SocietyManagementMVC.Controllers
{
    public class FlatsController : BaseController
    {
        public FlatsController(ApplicationDbContext db) : base(db)
        {
            
        }

        public IActionResult Index()
        {
            var data = db.Flats.ToList();
            return View(data);
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Flat f)
        {
            db.Flats.Add(f);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var data = db.Flats.Find(id);
            return View(data);
        }

        [HttpPost]
        public IActionResult Edit(Flat f)
        {
            var oldFlatdetails = db.Flats.Where(x => x.Id  == f.Id).Select(x => new {x.FlatNumber, x.BlockNumber}).FirstOrDefault();
            var oldFlatnumber = oldFlatdetails.BlockNumber + "-" + oldFlatdetails.FlatNumber;
            var alllotedFlat = db.Allotments.Where(x => x.FlatNumber == oldFlatnumber).Select(x => x.FlatNumber).FirstOrDefault();
            if (alllotedFlat != null) {
                var allotData = db.Allotments.Where(x => x.FlatNumber == alllotedFlat).FirstOrDefault();
                allotData.FlatNumber = f.BlockNumber + "-" + f.FlatNumber;
                allotData.Type = f.Type;
                        
                var billData = db.Bills.Where(x => x.FlatNumber == alllotedFlat).ToList();
                foreach (var bill in billData)
                {
                    bill.FlatNumber = f.BlockNumber + "-" + f.FlatNumber;
                }

                var complaintData = db.Complaints.Where(x => x.FlatNumber == alllotedFlat).ToList();
                foreach (var complaint in complaintData)
                {
                    complaint.FlatNumber = f.BlockNumber + "-" + f.FlatNumber;
                }

                var visitorData = db.Visitors.Where(x => x.FlatNumber == alllotedFlat).ToList();

                foreach (var visitor in visitorData)
                {
                    visitor.FlatNumber = f.BlockNumber + "-" + f.FlatNumber;
                }

                var notificationData = db.Notifications.Where(x => x.FlatNumber == alllotedFlat).ToList();

                foreach (var notification in notificationData)
                {
                    notification.FlatNumber = f.BlockNumber + "-" + f.FlatNumber;
                }

            }

            db.Flats.Update(f);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var oldFlatdetails = db.Flats.Where(x => x.Id == id).Select(x => new { x.FlatNumber, x.BlockNumber }).FirstOrDefault();
            var oldFlatnumber = oldFlatdetails.BlockNumber + "-" + oldFlatdetails.FlatNumber;
            var alllotedFlat = db.Allotments.Where(x => x.FlatNumber == oldFlatnumber).Select(x => x.FlatNumber).FirstOrDefault();
            if (alllotedFlat != null)
            {
                var allotData = db.Allotments.Where(x => x.FlatNumber == alllotedFlat).FirstOrDefault();

                var billData = db.Bills.Where(x => x.FlatNumber == alllotedFlat).ToList();
              

                var complaintData = db.Complaints.Where(x => x.FlatNumber == alllotedFlat).ToList();
               

                var visitorData = db.Visitors.Where(x => x.FlatNumber == alllotedFlat).ToList();

             
                var notificationData = db.Notifications.Where(x => x.FlatNumber == alllotedFlat).ToList();

                db.Allotments.Remove(allotData);
                db.Bills.RemoveRange(billData);
                db.Complaints.RemoveRange(complaintData);
                db.Visitors.RemoveRange(visitorData);
                db.Notifications.RemoveRange(notificationData);

            }
            var flatData = db.Flats.Find(id);
            db.Flats.Remove(flatData);
            db.SaveChanges();
            return RedirectToAction("Index");
        }



    }
}
