using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Net;
using System.Web.Mvc;
using TrainingSystem.Models;
using FireSharp.Interfaces;
using FireSharp.Config;
using FireSharp.Response;

namespace TrainingSystem.Controllers
{
    public class TrainingStaffController : Controller
    {

        private InternalTrainingSystemEntities db = new InternalTrainingSystemEntities();

        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "74UiAQSl7LINPN1eV2TWj7cDReSioM5YkzWRfPog",
            BasePath = "https://mvc-project-9443e-default-rtdb.firebaseio.com/"
        };
        IFirebaseClient client;


        // GET: TrainingStaff
        public ActionResult Index()
        {
            if (Session["Admin_UserName"] != null)
            {
                return View(db.TrainingStaff.ToList());
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
        public ActionResult Login(TrainingStaff staff)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (ModelState.IsValid)
            {
                var obj = db.TrainingStaff.Where(a => a.staff_username.Equals(staff.staff_username) &&
                                     a.staff_password.Equals(staff.staff_password)).FirstOrDefault();
                if (obj != null)
                {
                    Session["StaffID"] = obj.staffID.ToString();
                    Session["Staff_UserName"] = obj.staff_username.ToString();
                    return RedirectToAction("StaffDashBoard");
                }
            }
            return View(staff);
        }

        public ActionResult StaffDashBoard()
        {
            if (Session["Staff_Username"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult ChangePassword()
        {
            if (Session["Staff_Username"] != null)
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
            string staff_username = Session["Staff_Username"].ToString();
            var obj = db.TrainingStaff.Where(a => a.staff_username.Equals(staff_username) &&
                                     a.staff_password.Equals(old_password)).FirstOrDefault();
            if (obj != null)
            {
                if (new_password.Equals(confirm_password))
                {
                    obj.staff_password = new_password;
                    db.SaveChanges();
                    return RedirectToAction("StaffDashBoard");
                }
            }
            ViewData["Message"] = "Old password is not correct or New password and " + "Confirm New Password are not match";
            return View();
        }


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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "staffID,staff_username,staff_password,staff_fullname,staff_address,staff_email")] TrainingStaff staff)
        {
            if (ModelState.IsValid)
            {
                var SearchData = db.TrainingStaff.Where(x => x.staff_username == staff.staff_username).SingleOrDefault();

                if (SearchData != null)
                {
                    ViewBag.Status = "Account not available";
                }

                else
                {
                    db.TrainingStaff.Add(staff);
                    db.SaveChanges();

                    try
                    {
                        AddTrainingStaffToFirebase(staff);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                    }

                    ViewBag.Message = "Create successfully";
                }
            }
            return View(staff);
        }

        public ActionResult Edit(int? id)
        {
            if (Session["Admin_UserName"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                TrainingStaff staff = db.TrainingStaff.Find(id);
                if (staff == null)
                {
                    return HttpNotFound();
                }
                return View(staff);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "staffID,staff_username,staff_password,staff_fullname,staff_address,staff_email")] TrainingStaff staff)
        {
            if (ModelState.IsValid)
            {
                db.Entry(staff).State = EntityState.Modified;
                db.SaveChanges();

                client = new FireSharp.FirebaseClient(config);
                var data = staff;
                SetResponse setResponse = client.Set("Training Staff/" + staff.staffID, data);

                ViewBag.Message = "Edit successfully";
            }
            return View(staff);
        }



        public ActionResult Delete(int? id)
        {
            if (Session["Admin_UserName"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                TrainingStaff staff = db.TrainingStaff.Find(id);
                if (staff == null)
                {
                    return HttpNotFound();
                }
                return View(staff);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TrainingStaff staff = db.TrainingStaff.Find(id);
            db.TrainingStaff.Remove(staff);
            db.SaveChanges();

            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Delete("Training Staff/" + id);

            return RedirectToAction("Index");
        }


        public ActionResult UpdateProfile()
        {
            int staffID = Convert.ToInt32(Session["ID"]);
            if (staffID == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            return View(db.TrainingStaff.Find(staffID));
              
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile([Bind(Include = "staffID,staff_username,staff_password,staff_fullname,staff_address,staff_email")] TrainingStaff staff)
        {
            if (ModelState.IsValid)
            {
                db.Entry(staff).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("StaffDashBoard");
            }
            return View(staff);
        }

        public ActionResult Logout()
        {
            Session.Clear();  //remove all sessions in your app
            return RedirectToAction("Login", "TrainingStaff");  //redirect to Login page
        }

        private void AddTrainingStaffToFirebase(TrainingStaff staff)
        {
            client = new FireSharp.FirebaseClient(config);
            var data = staff;

            PushResponse response = client.Push("Training Staff/" + staff.staffID, data);
        }
    }
}