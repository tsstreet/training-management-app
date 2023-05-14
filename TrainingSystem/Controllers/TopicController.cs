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
    public class TopicController : Controller
    {
        private InternalTrainingSystemEntities db = new InternalTrainingSystemEntities();

        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "74UiAQSl7LINPN1eV2TWj7cDReSioM5YkzWRfPog",
            BasePath = "https://mvc-project-9443e-default-rtdb.firebaseio.com/"
        };
        IFirebaseClient client;

        // GET: Topic
        public ActionResult Index(int? page)
        {
            var topic = db.Topic.Include(t => t.Courses);
            
            return View(topic.ToList().ToPagedList(page ?? 1, 20));

        }


        // GET: Topic/Create
        public ActionResult Create()
        {
            if (Session["Staff_Username"] != null)
            {
                ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name");
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
        public ActionResult Create([Bind(Include = "topicID,topic_name,topic_descriptions,courseID,course_time, topic_status")] Topic topic)
        {
            if (ModelState.IsValid)
            {
                db.Topic.Add(topic);
                db.SaveChanges();

                try
                {
                    AddTopicToFirebase(topic);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                ViewBag.Message = "Create successfully";
            }

            ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name", topic.courseID);
            return View(topic);
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
                Topic topic = db.Topic.Find(id);
                if (topic == null)
                {
                    return HttpNotFound();
                }
                ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name", topic.courseID);
                return View(topic);
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
        public ActionResult Edit([Bind(Include = "topicID,topic_name,topic_descriptions,courseID,course_time, topic_status")] Topic topic)
        {
            if (ModelState.IsValid)
            {
                db.Entry(topic).State = EntityState.Modified;
                db.SaveChanges();

                client = new FireSharp.FirebaseClient(config);
                var data = topic;
                SetResponse setResponse = client.Set("Topic/" + topic.topicID, data);

                ViewBag.Message = "Edit successfully";
            }
            ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name", topic.courseID);
            return View(topic);
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
                Topic topic = db.Topic.Find(id);
                if (topic == null)
                {
                    return HttpNotFound();
                }
                return View(topic);
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
                Topic topic = db.Topic.Find(id);
                db.Topic.Remove(topic);
                db.SaveChanges();

                client = new FireSharp.FirebaseClient(config);
                FirebaseResponse response = client.Delete("Topic/" + id);

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        private void AddTopicToFirebase(Topic topic)
        {
            client = new FireSharp.FirebaseClient(config);
            var data = topic;

            PushResponse response = client.Push("Topic/" + topic.topicID, data);
        }
    }
}
