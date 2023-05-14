using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    public class AssignCourseController : Controller
    {
        private InternalTrainingSystemEntities db = new InternalTrainingSystemEntities();

        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "74UiAQSl7LINPN1eV2TWj7cDReSioM5YkzWRfPog",
            BasePath = "https://mvc-project-9443e-default-rtdb.firebaseio.com/"
        };
        IFirebaseClient client;

        // GET: AssignCourse
        public ActionResult Index(int? page)
        {
            
            if (Session["Staff_UserName"] != null)
            {
                ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name");
                var assignCourse = db.AssignCourse.Include(a => a.Courses).Include(a => a.Trainer);
                return View(assignCourse.ToList().ToPagedList(page ?? 1, 20));
                
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }

        [HttpPost]
        public ActionResult Index(int? courseID, int? page)
        {

                ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name");

                if (courseID != null)
                {
                    var assignCourse = db.AssignCourse.Include(a => a.Courses).Include(a => a.Trainer).Where(a => a.courseID == courseID);
                    return View(assignCourse.ToList().ToPagedList(page ?? 1, 20));
                }
                else
                {
                    var assignCourse = db.AssignCourse.Include(a => a.Courses).Include(a => a.Trainer);
                    return View(assignCourse.ToList().ToPagedList(page ?? 1, 20));
                }
        }

        public ActionResult ShowAssignCourse()
        {

            if (Session["Trainer_UserName"] != null)
            {

                int user = Convert.ToInt32(Session["ID"]);
                //var assignCourse = db.AssignCourse.Include(a => a.Courses).Include(a => a.Trainer).Where(a => a.trainerID == user).SingleOrDefault();

                var assignCourse = (from a in db.AssignCourse
                               join c in db.Trainer on a.trainerID equals c.trainerID
                               where (a.trainerID == user)
                               select a).ToList();


                return View(assignCourse);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }

        // GET: AssignCourse/Create
        public ActionResult Create()
        {
            if (Session["Staff_UserName"] != null)
            {
                ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name");       
                ViewBag.trainerID = new SelectList(db.Trainer, "trainerID", "trainer_username");
                return View();
                
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }       

        }

        // POST: AssignCourse/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "asscourseID,asscourse_descriptions,courseID,trainerID")] AssignCourse assignCourse)
        {
                if (ModelState.IsValid)
                {
                    var SearchData = db.AssignCourse.Where(x => x.courseID == assignCourse.courseID).Where
                                         (x=> x.trainerID == assignCourse.trainerID).SingleOrDefault();

                if (SearchData != null)
                {
                    ViewBag.Status = "Already have assigned";
                }
                else
                {
                    db.AssignCourse.Add(assignCourse);
                    db.SaveChanges();
                    ViewBag.Message = "Assign successfully";

                    try
                    {
                        AddAssignToFirebase(assignCourse);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                    }

                    var trainerEmail = (from a in db.Trainer
                                        where (a.trainerID == assignCourse.trainerID)
                                        select new
                                        {
                                            trainerEmail = a.trainer_email
                                        }).SingleOrDefault();

                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(ConfigurationManager.AppSettings["Email"].ToString());
                    mailMessage.To.Add(trainerEmail.trainerEmail);
                    mailMessage.Subject = "Assigned Work";
                    mailMessage.Body = "You have an assigned work please check it out. If you have any problems with the assigned work don't hesitate to contact us.";
                    SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", Convert.ToInt32(587));
                    System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["Email"].ToString(), ConfigurationManager.AppSettings["Password"].ToString());
                    smtpClient.Credentials = credentials;
                    smtpClient.EnableSsl = true;
                    smtpClient.Send(mailMessage);
                }
                 
                }
                    
            ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name", assignCourse.courseID);
            ViewBag.trainerID = new SelectList(db.Trainer, "trainerID", "trainer_username", assignCourse.trainerID);
            return View(assignCourse);
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
                AssignCourse assignCourse = db.AssignCourse.Find(id);
                if (assignCourse == null)
                {
                    return HttpNotFound();
                }
                ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name", assignCourse.courseID);
                ViewBag.trainerID = new SelectList(db.Trainer, "trainerID", "trainer_username", assignCourse.trainerID);
                return View(assignCourse);
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
        public ActionResult Edit([Bind(Include = "asscourseID,asscourse_descriptions,courseID,trainerID")] AssignCourse assignCourse)
        {
            if (ModelState.IsValid)
            {
                var SearchData = db.AssignCourse.Where(x => x.asscourseID != assignCourse.asscourseID && x.courseID == assignCourse.courseID).Where
                                         (x => x.trainerID == assignCourse.trainerID).SingleOrDefault();

                if (SearchData != null)
                {
                    ViewBag.Status = "Already have assigned";
                }
                else
                {
                    db.Entry(assignCourse).State = EntityState.Modified;
                    db.SaveChanges();


                    client = new FireSharp.FirebaseClient(config);
                    var data = assignCourse;
                    SetResponse setResponse = client.Set("Assign/" + assignCourse.asscourseID, data);
                    ViewBag.Message = "Edit successfully";
                }
                
            }
            ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name", assignCourse.courseID);
            ViewBag.trainerID = new SelectList(db.Trainer, "trainerID", "trainer_username", assignCourse.trainerID);
            return View(assignCourse);
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
                AssignCourse assignCourse = db.AssignCourse.Find(id);
                if (assignCourse == null)
                {
                    return HttpNotFound();
                }
                return View(assignCourse);
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
            AssignCourse assignCourse = db.AssignCourse.Find(id);
            db.AssignCourse.Remove(assignCourse);
            db.SaveChanges();

            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Delete("Assign/" + id);
            return RedirectToAction("Index");
        }

        public FileResult Download(string courseMaterial)
        {
            var FileVirtualPath = "~/Materials/" + courseMaterial;
          
            if (FileVirtualPath ==null)
            {
                ViewBag.Message = "No file to download";
            }

            return File(FileVirtualPath, "application/force- download", Path.GetFileName(FileVirtualPath));   
        }


        private void AddAssignToFirebase(AssignCourse assignCourse)
        {
            client = new FireSharp.FirebaseClient(config);
            var data = assignCourse;

            PushResponse response = client.Push("Assign/" + assignCourse.asscourseID, data);
        }
    }
}
