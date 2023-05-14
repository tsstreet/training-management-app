using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TrainingSystem.Models;
using PagedList.Mvc;
using PagedList;
using System.Dynamic;
using FireSharp.Interfaces;
using FireSharp.Config;
using FireSharp.Response;


namespace TrainingSystem.Controllers
{
    public class TrainerController : Controller
    {
        private InternalTrainingSystemEntities db = new InternalTrainingSystemEntities();

        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "74UiAQSl7LINPN1eV2TWj7cDReSioM5YkzWRfPog",
            BasePath = "https://mvc-project-9443e-default-rtdb.firebaseio.com/"
        };
        IFirebaseClient client;

        // GET: Trainer
        public ActionResult Index(int? page)
        {
            if (Session["Admin_UserName"] != null)
            {
                return View(db.Trainer.ToList().ToPagedList(page ?? 1, 20));
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Trainer trainer)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (ModelState.IsValid)
            {
                var obj = db.Trainer.Where(a => a.trainer_username.Equals(trainer.trainer_username) &&
                                     a.trainer_password.Equals(trainer.trainer_password)).FirstOrDefault();
                if (obj != null)
                {
                    Session["TrainerID"] = obj.trainerID.ToString();
                    Session["Trainer_UserName"] = obj.trainer_username.ToString();
                    return RedirectToAction("TrainerDashBoard");
                }
            }
            return View(trainer);
        }

        public ActionResult TrainerDashBoard()
        {
            if (Session["Trainer_UserName"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            
        }

        // GET: Trainer/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["Staff_UserName"] != null || Session["Admin_UserName"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Trainer trainer = db.Trainer.Find(id);

                ViewBag.AssignCourse = (from a in db.AssignCourse
                               join c in db.Trainer on a.trainerID equals c.trainerID
                               join d in db.Courses on a.courseID equals d.courseID
                               where (c.trainerID == id)
                               select a).ToList();

                if (trainer == null)
                {
                    return HttpNotFound();
                }
                return View(trainer);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            


        }

        // GET: Trainer/Create
        public ActionResult Create()
        {

            if (Session["Admin_UserName"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            

        }

        // POST: Trainer/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "trainerID,trainer_username,trainer_password,trainer_fullname,trainer_address,trainer_workingplace,trainer_phone,trainer_email")] Trainer trainer)
        {
            if (ModelState.IsValid)
            {
                var SearchData = db.Trainer.Where(x => x.trainer_username == trainer.trainer_username).SingleOrDefault();

                if (SearchData != null)
                {
                    ViewBag.Status = "Account not available";
                }

                else
                {
                    db.Trainer.Add(trainer);
                    db.SaveChanges();

                    try
                    {
                        AddTrainerToFirebase(trainer);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                    }
                    ViewBag.Message = "Create successfully";
                }
            }

            return View(trainer);
        }

        // GET: Trainer/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["Admin_UserName"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Trainer trainer = db.Trainer.Find(id);
                if (trainer == null)
                {
                    return HttpNotFound();
                }
                return View(trainer);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }
        // POST: Trainer/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "trainerID,trainer_username,trainer_password,trainer_fullname,trainer_address,trainer_workingplace,trainer_phone,trainer_email")] Trainer trainer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(trainer).State = EntityState.Modified;
                db.SaveChanges();

                client = new FireSharp.FirebaseClient(config);
                var data = trainer;
                SetResponse setResponse = client.Set("Trainer/" + trainer.trainerID, data);

                ViewBag.Message = "Edit successfully";
            }
            return View(trainer);
        }


        // GET: Trainer/Delete/5
        public ActionResult Delete(int? id)
        {
            if (Session["Admin_UserName"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Trainer trainer = db.Trainer.Find(id);
                if (trainer == null)
                {
                    return HttpNotFound();
                }
                return View(trainer);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Trainer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Trainer trainer = db.Trainer.Find(id);
            db.Trainer.Remove(trainer);
            db.SaveChanges();

            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Delete("Trainer/" + id);

            return RedirectToAction("Index");
        }

        public ActionResult ChangePassword()
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

        [HttpPost]
        public ActionResult ChangePassword(string old_password, string new_password, string confirm_password)
        {
            string trainer_username = Session["Trainer_Username"].ToString();
            var obj = db.Trainer.Where(a => a.trainer_username.Equals(trainer_username) &&
                                     a.trainer_password.Equals(old_password)).FirstOrDefault();
            if (obj != null)
            {
                if (new_password.Equals(confirm_password))
                {
                    obj.trainer_password = new_password;
                    db.SaveChanges();

                    return RedirectToAction("TrainerDashBoard");
                }
            }
            ViewData["Message"] = "Old password is not correct or New password and " + "Confirm New Password are not match";
            return View();
        }

        public ActionResult UpdateProfile()
        {
            int trainerID = Convert.ToInt32(Session["ID"]);
            if (trainerID == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(db.Trainer.Find(trainerID));

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile([Bind(Include = "trainerID,trainer_username,trainer_password,trainer_fullname,trainer_address,trainer_workingplace,trainer_phone,trainer_email")] Trainer trainer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(trainer).State = EntityState.Modified;
                db.SaveChanges();

                client = new FireSharp.FirebaseClient(config);
                var data = trainer;
                SetResponse setResponse = client.Set("Trainer/" + trainer.trainerID, data);

                return RedirectToAction("TrainerDashBoard");
            }
            return View(trainer);
        }

        public ActionResult Logout()
        {
            Session.Clear();  //remove all sessions in your app
            return RedirectToAction("Login", "Trainer");  //redirect to 
        }

        private void AddTrainerToFirebase(Trainer trainer)
        {
            client = new FireSharp.FirebaseClient(config);
            var data = trainer;

            PushResponse response = client.Push("Trainer/" + trainer.trainerID, data);
        }

        
    }
}
