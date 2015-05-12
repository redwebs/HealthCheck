using System.Collections.Generic;
using System.Web.Mvc;
using WebMvc.Models;
using WebMvc.HC;

namespace WebMvc.Controllers
{
    public class HealthCheckController : Controller
    {
        // GET: HealthCheck
        public ActionResult Index()
        {
            // Simple health check to confirm connectivity and database

            return Database.RegularCheck() ? new HttpStatusCodeResult(200) : new HttpStatusCodeResult(500);
        }

        // GET: HealthCheck/Details
        public ActionResult Details()
        {
            ViewBag.Url = Request.Url.AbsoluteUri;

            var secList = new List<HealthCheckSection> {Database.CheckDatabasesEx()};

            IEnumerable<HealthCheckSection> ienum = secList;

            return View(ienum);
        }

    }
}
