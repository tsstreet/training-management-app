using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    public class RequestController : Controller
    {
        private InternalTrainingSystemEntities db = new InternalTrainingSystemEntities();

        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "74UiAQSl7LINPN1eV2TWj7cDReSioM5YkzWRfPog",
            BasePath = "https://mvc-project-9443e-default-rtdb.firebaseio.com/"
        };
        IFirebaseClient client;

        // GET: Request
        public ActionResult Index(int? page)
        {
           
            if (Session["Staff_UserName"] != null)
            {

                var request = db.Request.Include(t => t.Trainer);
                return View(request.ToList().ToPagedList(page ?? 1, 20));
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult ShowRequest()
        {
       
            if (Session["Trainer_UserName"] != null)
            {

                //var request = db.Request.Include(t => t.Trainer);

                int user = Convert.ToInt32(Session["ID"]);

                var request = (from a in db.Request
                                   join c in db.Trainer on a.trainerID equals c.trainerID
                                   where (a.trainerID == user)
                                   select a).OrderByDescending(a=> a.request_date).DefaultIfEmpty();

                return View(request.ToList());
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Topic/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["Staff_Username"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Request request = db.Request.Find(id);
                if (request == null)
                {
                    return HttpNotFound();
                }
                return View(request);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }

        // GET: Topic/Create
        public ActionResult Create()
        {
            if (Session["Trainer_Username"] != null)
            {
                
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }

        // POST: Topic/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "requestID,request_subject,request_descriptions,trainerID,request_answer,request_date, request_status")] Request request)
        {
            if (ModelState.IsValid)
            {
                int trainerID = Convert.ToInt32(Session["ID"]);
                request.trainerID = trainerID;
                db.Request.Add(request);
                db.SaveChanges();

                try
                {
                    AddRequestToFirebase(request);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }

                ViewBag.Message = "Create successfully";
            }

            return View(request);
        }

        // GET: Topic/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["Staff_Username"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Request request = db.Request.Find(id);
                var status = new List<string>() { "Approved", "Denied", "Waiting" };
                ViewBag.status = status;
                if (request == null)
                {
                    return HttpNotFound();
                }
                return View(request);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }

        // POST: Topic/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "requestID,request_subject,request_descriptions,request_answer,trainerID,request_date, request_status")] Request request)
        {
            if (ModelState.IsValid)
            {
                
                db.Entry(request).State = EntityState.Modified;
                db.SaveChanges();

                client = new FireSharp.FirebaseClient(config);
                var data = request;
                SetResponse setResponse = client.Set("Request/" + request.requestID, data);

                ViewBag.Message = "Edit successfully";
            }

            var status = new List<string>() { "Approved", "Denied", "Waiting" };
            ViewBag.status = status;
            return View(request);
        }

        // GET: Topic/Delete/5
        public ActionResult Delete(int? id)
        {
            if (Session["Staff_Username"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Request request = db.Request.Find(id);
                if (request == null)
                {
                    return HttpNotFound();
                }
                return View(request);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Topic/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Request request = db.Request.Find(id);
                db.Request.Remove(request);
                db.SaveChanges();

                client = new FireSharp.FirebaseClient(config);
                FirebaseResponse response = client.Delete("Request/" + id);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("Error");
            }
        }

        private void AddRequestToFirebase(Request request)
        {
            client = new FireSharp.FirebaseClient(config);
            var data = request;

            PushResponse response = client.Push("Request/" + request.requestID, data);
        }
    }
}
