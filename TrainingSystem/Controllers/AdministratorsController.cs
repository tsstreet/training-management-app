using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    public class AdministratorsController : Controller
    {
        private InternalTrainingSystemEntities db = new InternalTrainingSystemEntities();

        // GET: Administrators
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Administrators admin)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (ModelState.IsValid)
            {
                var obj = db.Administrators.Where(a => a.admin_username.Equals(admin.admin_username) &&
                                     a.admin_password.Equals(admin.admin_password)).FirstOrDefault();
                if (obj != null)
                {
                    Session["AdminID"] = obj.adminID.ToString();
                    Session["Admin_UserName"] = obj.admin_username.ToString();
                    return RedirectToAction("AdminDashBoard");
                }
            }
            return View(admin);
        }

        public ActionResult AdminDashBoard()
        {
            if (Session["Admin_Username"] != null)
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
            if (Session["Admin_Username"] != null)
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
            string admin_username = Session["Admin_Username"].ToString();
            var obj = db.Administrators.Where(a => a.admin_username.Equals(admin_username) &&
                                     a.admin_password.Equals(old_password)).FirstOrDefault();
            if(obj!= null)
            {
                if(new_password.Equals(confirm_password))
                {
                    obj.admin_password = new_password;
                    db.SaveChanges();
                    return RedirectToAction("AdminDashBoard");
                }
            }
            ViewData["Message"] = "Old password is not correct or New password and" + "Confirm New Password are not match";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();  
            return RedirectToAction("Login", "Administrators");  
        }
    }
}