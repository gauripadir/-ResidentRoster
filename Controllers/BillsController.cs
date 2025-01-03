using iTextSharp.text.pdf;
using iTextSharp.text;
using iTextSharp.tool.xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Razorpay.Api;
using SocietyManagementMVC.Data;
using SocietyManagementMVC.Models;
using System.Reflection.Metadata;
using Document = iTextSharp.text.Document;
using System.Net.Mail;
using System.Net;

namespace SocietyManagementMVC.Controllers
{
    public class BillsController : BaseController
    {
        //private readonly ApplicationDbContext db;

        public BillsController(ApplicationDbContext db) : base(db)
        {
            //this.db = db;
        }
        public IActionResult Index()
        {
            var email = HttpContext.Session.GetString("email");
            if (email == "Admin@gmail.com")
            {
                var data = db.Bills.ToList();
                return View(data);
            }
            var flatnumber = db.Allotments.Where(x => x.AllotedTo == email).Select(x => x.FlatNumber).FirstOrDefault();
            var UBdata = db.Bills.Where(x => x.FlatNumber == flatnumber && x.PaidAmount != null).ToList();
            //var UBdata = db.Bills.Where(x => x.FlatNumber == flatnumber).ToList();
            return View(UBdata);
        }

        public IActionResult Create()
        {
            ViewBag.flatnumber = new SelectList(db.Allotments, "FlatNumber" , "FlatNumber");
            return View();
        }

        [HttpPost]
        public IActionResult Create(Bill b)
        {
            db.Bills.Add(b);

            var allotedto = db.Allotments.Where(x => x.FlatNumber == b.FlatNumber).Select(x => x.AllotedTo).FirstOrDefault();

            Notification noti = new Notification()
            {
                NotificationName = "Bill",
                NotificationFor = allotedto,
                FlatNumber = b.FlatNumber,
                NotificationMessage = b.BillTitle,
                Status = "Not Paid",
                Seen = "Not Seen"
            };

            db.Notifications.Add(noti);
            db.SaveChanges();   
            return RedirectToAction("Index");
        }


        public IActionResult Edit(int id)
        {
            ViewBag.flatnumber = new SelectList(db.Allotments, "FlatNumber", "FlatNumber");
            var data = db.Bills.Find(id);
            return View(data);
        }

        [HttpPost]
        public IActionResult Edit(Bill b)
        {
            var allotedto = db.Allotments.Where(x => x.FlatNumber == b.FlatNumber).Select(x => x.AllotedTo).FirstOrDefault();

            //var oldBdata = db.Bills.Find(b.Id);
            var oldBdata = db.Bills.AsNoTracking().FirstOrDefault(x => x.Id == b.Id);

            var Ndata = db.Notifications.Where(x => x.NotificationMessage == oldBdata.BillTitle && x.FlatNumber == oldBdata.FlatNumber).FirstOrDefault();
            Ndata.NotificationFor = allotedto;
            Ndata.FlatNumber = b.FlatNumber;
            Ndata.NotificationMessage = b.BillTitle;
            
            db.Notifications.Update(Ndata);
            db.Bills.Update(b);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var Bdata = db.Bills.Find(id);
            if (Bdata != null)
            {
                var notifcationId = db.Notifications.Where(x => x.NotificationMessage == Bdata.BillTitle && x.FlatNumber == Bdata.FlatNumber).Select(x => x.Id).FirstOrDefault();
                var Ndata = db.Notifications.Find(notifcationId);
                db.Notifications.Remove(Ndata);
                db.Bills.Remove(Bdata);
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
            var data = db.Bills.Find(id);
            if (data == null)
            {
                var notifdata = db.Notifications.Find(id);
                if (notifdata.Seen == "Not Seen")
                {
                    notifdata.Seen = "Seen";
                    db.Notifications.Update(notifdata);
                    db.SaveChanges();
                }
                var cdata = db.Bills.Where(x => x.FlatNumber == notifdata.FlatNumber && x.BillTitle == notifdata.NotificationMessage && x.PaidAmount != null).FirstOrDefault();
                return View(cdata);
            }
            return View(data);
        }

        public IActionResult BillPay(int id)
        {
            var notidata = db.Notifications.Find(id);
            ViewBag.notificationId = id;
            var billdata = db.Bills.Where(x => x.BillTitle == notidata.NotificationMessage && x.FlatNumber == notidata.FlatNumber && x.PaidAmount == null).FirstOrDefault();
            ViewBag.BillId = billdata.Id;
            return View(billdata);
        }

        [HttpPost]
        public IActionResult BillPay(Bill b,string notificationId)
        {
            if (b.PaymentMethod == "Cash")
            {
                return RedirectToAction("Invoice", new
                {
                    bid = b.Id.ToString(),
                    notifid = notificationId,
                    amount = b.PaidAmount,
                    pdate = b.PaymentDate,
                    pmethod = b.PaymentMethod
                });

            }
            if (b.PaymentMethod == "Online") {
                string keyId = "rzp_test_Kl7588Yie2yJTV";
                string keySecret = "6dN9Nqs7M6HPFMlL45AhaTgp";

                RazorpayClient razorpayClient = new RazorpayClient(keyId, keySecret);
                //int userId = 10;
                double amount = int.Parse(b.PaidAmount);

                //string email = b.;
                // Create an order
                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("amount", amount * 100); // Amount should be in paisa (multiply by 100 for rupees)
                options.Add("currency", "INR");
                options.Add("receipt", "order_receipt_123");
                options.Add("payment_capture", 1); // Auto capture payment

                Razorpay.Api.Order order = razorpayClient.Order.Create(options);

                string orderId = order["id"].ToString();

                // Generate checkout form and redirect user to Razorpay payment page
                string razorpayScript = $@"
    var options = {{
        'key': '{keyId}',
        'amount': {amount * 100},
        'currency': 'INR',
        'name': 'Housing Society',
        'description': 'Checkout Payment',
        'order_id': '{orderId}',
        'handler': function(response) {{
           
     
            alert('Payment successful. Payment ID: ' + response.razorpay_payment_id);
        
window.location.href = '/Bills/Invoice?bid=' + '{b.Id}' + '&notifid=' + '{notificationId}' + '&amount=' + '{b.PaidAmount}' + '&pdate=' + '{b.PaymentDate}' + '&pmethod=' + '{b.PaymentMethod}';
        }},
        'prefill': {{
            'name': 'Krish Kheloji',
            'email': 'khelojikrish@gmail.com',
            'contact': '7208921898'
        }},
        'theme': {{
            'color': '#F37254'
        }}
    }};
    var rzp1 = new Razorpay(options);
    rzp1.open();";


                //ClientScript.RegisterStartupScript(this.GetType(), "razorpayScript", razorpayScript, true);

                ViewBag.RazorpayScript = razorpayScript;
                return View(b);
            }
            
            return View();
        }

        public IActionResult Invoice(string bid, string notifid,string amount,string pdate,string pmethod)
        {
            var billdata = db.Bills.Find(Convert.ToInt32(bid));
            billdata.PaidAmount = amount;
            billdata.PaymentDate = pdate;
            billdata.PaymentMethod = pmethod;
            var notifdata = db.Notifications.Find(Convert.ToInt32(notifid));
            notifdata.NotificationFor = "Admin@gmail.com";
            notifdata.Status = "Paid";
            db.Notifications.Update(notifdata);
            db.Bills.Update(billdata);
            db.SaveChanges();

            Random random = new Random();
            int invoice_no = random.Next(1000, 9999);
            string s = @"
<style>
 body {
   font-family: Arial, sans-serif;
   background-color: #f4f4f4;
   padding: 20px;
 }

 .invoice {
   background-color: #fff;
   border-radius: 8px;
   box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
   padding: 30px;
   max-width: 600px;
   margin: 0 auto;
 }

 .invoice-header {
   border-bottom: 2px solid #f1f1f1;
   padding-bottom: 20px;
   margin-bottom: 20px;
   text-align: center;
 }

 .invoice-header h1 {
   font-size: 32px;
   margin: 0;
   color: #333;
 }

 .invoice-info {
   display: flex;
   justify-content: space-between;
   margin-bottom: 15px;
 }

 .invoice-info p {
   margin: 5px 0;
   color: #555;
 }

 .invoice-table {
   width: 100%;
   border-collapse: collapse;
   margin-bottom: 20px;
 }

 .invoice-table th, .invoice-table td {
   padding: 12px;
   border-bottom: 1px solid #f1f1f1;
   text-align: left;
 }

 .invoice-table th {
   background-color: blue;
   color: #fff;
   font-weight: bold;
 }

 .invoice-table td {
   color: #666;
 }

 .invoice-total {
   display: flex;
   justify-content: space-between;
   font-weight: bold;
   font-size: 18px;
   margin-top: 10px;
 }

 .footer {
   margin-top: 20px;
   text-align: center;
   color: #888;
   font-size: 14px;
 }
</style>

<body>
 <div class='invoice'>
   <div class='invoice-header'>
     <h1>Bill Invoice</h1>
   </div>
   <div class='invoice-info'>
     <p><strong>Invoice Number:</strong> BILL-" + $"{invoice_no}" + @"</p>
     <p><strong>Flat Number:</strong> " + $"{billdata.FlatNumber}" + @"</p>
     <p><strong>Bill Date:</strong> " + $"{billdata.BillMonth}" + @"</p>
   </div>
   <div class='invoice-info'>
     <p><strong>Bill Title:</strong> " + $"{billdata.BillTitle}" + @"</p>
     <p><strong>Payment Date:</strong> " + $"{billdata.PaymentDate}" + @"</p>
   </div>
   <table class='invoice-table'>
     <thead>
       <tr>
         <th>Bill Amount</th>
         <th>Paid Amount</th>
         <th>Payment Method</th>
       </tr>
     </thead>
     <tbody>
       <tr>
         <td>" + $"{billdata.BillAmount}" + @"</td>
         <td>" + $"{billdata.PaidAmount}" + @"</td>
         <td>" + $"{billdata.PaymentMethod}" + @"</td>
       </tr>
     </tbody>
   </table>

   <div class='invoice-total'>
     <p><strong>Total Paid Amount:</strong></p>
     <p>" + $"{billdata.PaidAmount}" + @"</p>
   </div>
 </div>
</body>
";


            MemoryStream memoryStream = new MemoryStream();
            
            Document document = new Document(PageSize.A4, 10f, 10f, 10f, 10f);
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            using (StringReader stringReader = new StringReader(s))
            {
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, stringReader);
            }

            document.Close();
            byte[] pdfData =  memoryStream.ToArray();

            string email = HttpContext.Session.GetString("email");
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("kawleprasad03@gmail.com");
            mailMessage.To.Add(email);
            mailMessage.Subject = "Bill Receipt";
            mailMessage.Body = "Please find the Bill Receipt attachment.";
            mailMessage.IsBodyHtml = true;

            // Attach PDF
            MemoryStream stream = new MemoryStream(pdfData);
            mailMessage.Attachments.Add(new Attachment(stream, "Receipt.pdf", "application/pdf"));

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.Credentials = new NetworkCredential("kawleprasad03@gmail.com", "fzdo rrmf uhce iptx");
            smtpClient.EnableSsl = true;

            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                stream.Dispose();
                mailMessage.Dispose();
            }


            return View(billdata);
        }

    }
}
