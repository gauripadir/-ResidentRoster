using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyManagementMVC.Data;
using SocietyManagementMVC.Models;

namespace SocietyManagementMVC.Controllers
{
    public class ReportsController : BaseController
    {

        public ReportsController(ApplicationDbContext db) : base(db) 
        {
             
        }
        public IActionResult Index()
        {

            return View();
        }


        [HttpPost]
        public IActionResult Index(string ReportType, DateTime StartDate, DateTime EndDate)
        {
            var model = new Report
            {
                Headers = new List<string>(),
                Rows = new List<List<string>>()
            };

            switch (ReportType)
            {
                case "Bills":
                    //var bills = db.Bills
                    //    .Where(b => Convert.ToDateTime(b.PaymentDate) >= StartDate && Convert.ToDateTime(b.PaymentDate) <= EndDate && b.PaidAmount != null)
                    //    //.Select(b => new { b.BillTitle, b.FlatNumber, b.BillAmount, b.PaidAmount, b.PaymentDate, b.BillMonth })
                    //    .ToList();

                    var bills = db.Bills
                               .AsEnumerable() // Switch to in-memory evaluation
                               .Where(b => DateTime.TryParse(b.PaymentDate, out var date) &&
                                           date >= StartDate && date <= EndDate &&
                                           b.PaidAmount != null)
                               .ToList();



                    model.Headers = new List<string> { "Bill Title", "Flat Number", "Bill Amount", "Paid Amount","Bill Month" , "Payment Date", "Payment Method" };
                    model.Rows = bills.Select(b => new List<string>
                {
                    b.BillTitle,
                    b.FlatNumber,
                    b.BillAmount,
                    b.PaidAmount,
                    b.BillMonth,
                    b.PaymentDate,
                    b.PaymentMethod
                }).ToList();
                    break;

                case "Complaints":
                    var complaints = db.Complaints
                        .AsEnumerable()
                        .Where(c => DateTime.TryParse(c.CreatedAtDate, out var date) && date >= StartDate && date <= EndDate)
                        .ToList();

                    model.Headers = new List<string> { "Complaint By", "Flat Number", "Description", "Comment", "Status" ,"Created Date" };
                    model.Rows = complaints.Select(c => new List<string>
                {
                    c.UserName,
                    c.FlatNumber,
                    c.ComplaintDescription,
                    c.Comment,
                    c.Status,
                    c.CreatedAtDate
                }).ToList();
                    break;

                case "Visitor":
                    var visitors = db.Visitors
                        .AsEnumerable()
                        .Where(v => DateTime.TryParse(v.CreatedAtDate, out var date) && date >= StartDate && date <= EndDate)
                        //.Select(v => new { v.VisitorName, v.FlatNumber, v.PersonToVisit, v.InTime, v.CreatedDate })
                        .ToList();

                    model.Headers = new List<string> { "Visitor Name", "Flat Number", "Visitor Phone", "Person To Visit", "In Time", "Reason To Visit", "Out Time", "Out Remark", "Status" , "Created Date" };
                    model.Rows = visitors.Select(v => new List<string>
                {
                    v.VisitorName,
                    v.FlatNumber,
                    v.VisitorPhone,
                    v.PersonToVisit,
                    v.InTime,
                    v.ReasonToVisit,
                    v.OutTime,
                    v.OutRemark,
                    v.Status,
                    v.CreatedAtDate
                }).ToList();
                    break;

                default:
                    ModelState.AddModelError("", "Invalid report type selected.");
                    break;
            }

            return View(model);
        }

    }
}
