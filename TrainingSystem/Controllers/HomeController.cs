using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    public class HomeController : Controller
    {
        private InternalTrainingSystemEntities db = new InternalTrainingSystemEntities();

        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password, string roles)
        {
            switch (roles)
            {
                case "1":     //if role is administrator
                    var obj = db.Administrators.Where(a => a.admin_username.Equals(username) &&
                                     a.admin_password.Equals(password)).FirstOrDefault();
                    if (obj != null)
                    {
                        Session["ID"] = obj.adminID.ToString();
                        Session["Admin_UserName"] = obj.admin_username.ToString();
                        return RedirectToAction("Index");
                        //return RedirectToAction("AdminDashBoard");
                        //return Redirect("Cours/Index");
                    }
                    break;
                case "2":     //if role is staff
                    var obj2 = db.TrainingStaff.Where(a => a.staff_username.Equals(username) &&
                                     a.staff_password.Equals(password)).FirstOrDefault();
                    if (obj2 != null)
                    {
                        Session["ID"] = obj2.staffID.ToString();
                        Session["Staff_UserName"] = obj2.staff_username.ToString();
                        return RedirectToAction("Index");
                        //return RedirectToAction("StaffDashBoard");
                    }
                    break;
                case "3":     //if role is trainer
                    var obj3 = db.Trainer.Where(a => a.trainer_username.Equals(username) &&
                                     a.trainer_password.Equals(password)).FirstOrDefault();
                    if (obj3 != null)
                    {
                        Session["ID"] = obj3.trainerID.ToString();
                        Session["Trainer_UserName"] = obj3.trainer_username.ToString();
                        return RedirectToAction("Index");
                        //return RedirectToAction("TrainerDashBoard");
                    }
                    break;
                default:
                    break;
            }
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();  //remove all sessions in your app
            return RedirectToAction("Index", "Home");  //redirect to Login page
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
