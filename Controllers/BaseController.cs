using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SocietyManagementMVC.Data;

namespace SocietyManagementMVC.Controllers
{
    public class BaseController : Controller
    {
        protected ApplicationDbContext db;

        public BaseController(ApplicationDbContext context)
        {
            db = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Get the logged-in user's email from HttpContext
            var email = HttpContext.Session.GetString("email");
            if (!string.IsNullOrEmpty(email))
            {
                // Fetch notifications for the logged-in user
                var notifications = db.Notifications
                    .Where(n => n.NotificationFor == email && n.Seen == "Not Seen")
                    .ToList();

                // Store notifications in ViewBag for layout usage
                ViewBag.Notifications = notifications;
            }

            base.OnActionExecuting(context);
        }
    }
}
