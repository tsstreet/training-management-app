using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
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
    public class CourseCategoriesController : Controller
    {
        private InternalTrainingSystemEntities db = new InternalTrainingSystemEntities();

        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "74UiAQSl7LINPN1eV2TWj7cDReSioM5YkzWRfPog",
            BasePath = "https://mvc-project-9443e-default-rtdb.firebaseio.com/"
        };
        IFirebaseClient client;

        // GET: CourseCategories
        public ActionResult Index()
        {
            if (Session["Staff_UserName"] != null)
            {
                return View(db.CourseCategories.ToList());
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            
        }

        // GET: CourseCategories/Create
        public ActionResult Create()
        {
            if (Session["Staff_UserName"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            

        }

        // POST: CourseCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "categoryID,category_name,category_descriptions")] CourseCategories courseCategories)
        {
            if (ModelState.IsValid)
            {
                db.CourseCategories.Add(courseCategories);
                db.SaveChanges();

                try
                {
                    AddCourseCategoryToFirebase(courseCategories);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }

                ViewBag.Message = "Create successfully";
            }

            return View(courseCategories);
        }

        // GET: CourseCategories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["Staff_UserName"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                CourseCategories courseCategories = db.CourseCategories.Find(id);
                if (courseCategories == null)
                {
                    return HttpNotFound();
                }
                return View(courseCategories);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            
        }

        // POST: CourseCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "categoryID,category_name,category_descriptions")] CourseCategories courseCategories)
        {
            if (ModelState.IsValid)
            {
                db.Entry(courseCategories).State = EntityState.Modified;
                db.SaveChanges();

                client = new FireSharp.FirebaseClient(config);
                var data = courseCategories;
                SetResponse setResponse = client.Set("Course Category/" + courseCategories.categoryID, data);

                ViewBag.Message = "Edit successfully";
            }
            return View(courseCategories);
        }

        // GET: CourseCategories/Delete/5
        public ActionResult Delete(int? id)
        {
                if (Session["Staff_UserName"] != null)
                {
                    if (id == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    CourseCategories courseCategories = db.CourseCategories.Find(id);
                    if (courseCategories == null)
                    {
                        return HttpNotFound();
                    }
                    return View(courseCategories);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
        }

        // POST: CourseCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                CourseCategories courseCategories = db.CourseCategories.Find(id);
                db.CourseCategories.Remove(courseCategories);
                db.SaveChanges();

                client = new FireSharp.FirebaseClient(config);
                FirebaseResponse response = client.Delete("Course Category/" + id);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("Error");
            }
           
        }

        private void AddCourseCategoryToFirebase(CourseCategories courseCategories)
        {
            client = new FireSharp.FirebaseClient(config);
            var data = courseCategories;

            PushResponse response = client.Push("Course Category/" + courseCategories.categoryID, data);
        }
    }
}
