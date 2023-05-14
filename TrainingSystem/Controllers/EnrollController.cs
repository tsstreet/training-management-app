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
    public class EnrollController : Controller
    {
        private InternalTrainingSystemEntities db = new InternalTrainingSystemEntities();

        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "74UiAQSl7LINPN1eV2TWj7cDReSioM5YkzWRfPog",
            BasePath = "https://mvc-project-9443e-default-rtdb.firebaseio.com/"
        };
        IFirebaseClient client;


        public ActionResult Index(int? page)
        {
            
            if (Session["Staff_UserName"] != null)
            {
                ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name");
                var enroll = db.Enroll.Include(a => a.Courses).Include(a => a.Trainee);
                return View(enroll.ToList().ToPagedList(page ?? 1, 20));
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }

        [HttpPost]
        public ActionResult Index(int? courseID, int? page)
        {
                if (courseID != null)
                {
                    ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name");
                    var enroll = db.Enroll.Include(a => a.Courses).Include(a => a.Trainee).Where(a => a.courseID == courseID);
                    return View(enroll.ToList().ToPagedList(page ?? 1, 20));
                }
                else
                {
                    ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name");
                    var enroll = db.Enroll.Include(a => a.Courses).Include(a => a.Trainee);
                    return View(enroll.ToList().ToPagedList(page ?? 1, 20));
                }            

        }

   

        // GET: AssignCourse/Create
        public ActionResult Create()
        {
            if (Session["Staff_UserName"] != null)
            {
                ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name");       
                ViewBag.traineeID = new SelectList(db.Trainee, "traineeID", "trainee_username");
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }       

        }

        public JsonResult GetTrainee(int traineeID)
        {
            // Retrieve the course with the given ID from the database
            //var course = db.Courses.Where(c => c.courseID == courseID).ToList();

            var trainee = (from a in db.Trainee
                          where (traineeID == a.traineeID)
                          select new
                          {
                              trainee_fullname = a.trainee_fullname,
                              trainee_email = a.trainee_email
 
                          }).FirstOrDefault();

            return new JsonResult { Data = trainee, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            // Return the result as JSON
            //return Json(course);
        }

        // POST: AssignCourse/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]

        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "enrollID,enroll_descriptions,courseID,traineeID,enroll_date")] Enroll enroll)
        {
            if (ModelState.IsValid)
            {
                var SearchData = db.Enroll.Where(x => x.courseID == enroll.courseID).Where
                                         (x => x.traineeID == enroll.traineeID).SingleOrDefault();

                if (SearchData != null)
                {
                    ViewBag.Status = "Already have enrolled";
                }
                else
                {
                    db.Enroll.Add(enroll);
                    db.SaveChanges();

                    try
                    {
                        AddEnrollToFirebase(enroll);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                    }

                    ViewBag.Message = "Create successfully";
                }
            }

            ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name", enroll.courseID);
            ViewBag.traineeID = new SelectList(db.Trainee, "traineeID", "trainee_username", enroll.traineeID);
            return View(enroll);
        }

        // GET: AssignCourse/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["Staff_UserName"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Enroll enroll = db.Enroll.Find(id);
                if (enroll == null)
                {
                    return HttpNotFound();
                }
                ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name", enroll.courseID);
                ViewBag.traineeID = new SelectList(db.Trainee, "traineeID", "trainee_username", enroll.traineeID);
                return View(enroll);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            

        }

        // POST: AssignCourse/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "enrollID,enroll_descriptions,courseID,traineeID,enroll_date")] Enroll enroll)
        {
            if (ModelState.IsValid)
            {
                var SearchData = db.Enroll.Where(x => x.enrollID != enroll.enrollID && x.courseID == enroll.courseID).Where
                                         (x => x.traineeID == enroll.traineeID).SingleOrDefault();

                if (SearchData != null)
                {
                    ViewBag.Status = "Already have enrolled";
                }
                else
                {
                    db.Entry(enroll).State = EntityState.Modified;
                    db.SaveChanges();
                    client = new FireSharp.FirebaseClient(config);
                    var data = enroll;
                    SetResponse setResponse = client.Set("Enroll/" + enroll.enrollID, data);
                    ViewBag.Message = "Edit successfully";
                }


            }
            ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name", enroll.courseID);
            ViewBag.traineeID = new SelectList(db.Trainee, "traineeID", "trainee_username", enroll.traineeID);
            return View(enroll);
        }

        // GET: AssignCourse/Delete/5
        public ActionResult Delete(int? id)
        {
            if (Session["Staff_UserName"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Enroll enroll = db.Enroll.Find(id);
                if (enroll == null)
                {
                    return HttpNotFound();
                }
                return View(enroll);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            

        }

        // POST: AssignCourse/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Enroll enroll = db.Enroll.Find(id);
            db.Enroll.Remove(enroll);
            db.SaveChanges();
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Delete("Enroll/" + id);
            return RedirectToAction("Index");
        }



        private void AddEnrollToFirebase(Enroll enroll)
        {
            client = new FireSharp.FirebaseClient(config);
            var data = enroll;

            PushResponse response = client.Push("Enroll/" + enroll.enrollID, data);
            //SetResponse setResponse = client.Set("Courses/", data);
        }
    }
}
