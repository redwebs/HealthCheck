using System.Collections.Generic;
using System.Web.Mvc;
using WebMvc.Models;
using WebMvc.CsLib;
using WebMvc.HC;

namespace WebMvc.Controllers
{
    public class HealthCheckController : Controller
    {
        // GET: HealthCheck
        public ActionResult Index()
        {
            // Simple health check to confirm connectivity and database

            ViewBag.Url = "The URL here";

            if (Database.RegularCheck())
            {
                return new HttpStatusCodeResult(200);
            }
            return new HttpStatusCodeResult(500);;
        }

        // GET: HealthCheck/Details
        public ActionResult Details()
        {
            ViewBag.Url = "The URL here";

            var oddRow = false;

            var entry1 = new HealthCheckEntry
            {
                ItemName = "Database check 1",
                Result = true,
                ResultDescription = "Flying colors",
                OddRow = oddRow
            };
            oddRow = !oddRow;

            var entry2 = new HealthCheckEntry
            {
                ItemName = "Database check 2",
                Result = false,
                ResultDescription = "Dirty Flying colors",
                OddRow = oddRow
            };

            var section = new HealthCheckSection
            {
                Title = "Database",
                Entries = new List<HealthCheckEntry>()
            };
            section.Entries.Add(entry1);
            section.Entries.Add(entry2);


            var secList = new List<HealthCheckSection>();
            secList.Add(section);

            IEnumerable<HealthCheckSection> ienum = secList;

            //var sectionList = new HealthCheckList
            //{
            //    HcList = new List<HealthCheckSection>()
            //};
            //sectionList.HcList.Add(section);

            return View(ienum);
        }

        // GET: HealthCheck/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: HealthCheck/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: HealthCheck/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: HealthCheck/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: HealthCheck/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: HealthCheck/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
