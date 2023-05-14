using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    public class EventsController : Controller
    {
        private InternalTrainingSystemEntities db = new InternalTrainingSystemEntities();

        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "74UiAQSl7LINPN1eV2TWj7cDReSioM5YkzWRfPog",
            BasePath = "https://mvc-project-9443e-default-rtdb.firebaseio.com/"
        };
        IFirebaseClient client;

        // GET: Event
        public ActionResult Schedule()
        {
            return View();
        }

        public JsonResult GetEvents()
        {
            //var events = db.Events.ToList();

            var events = (from a in db.Events
                                    join d in db.Courses on a.courseID equals d.courseID
                                    select new
                                    {
                                        eventID = a.eventID,
                                        room = a.room,
                                        description = a.event_descriptions,
                                        courseName = d.course_name,
                                        courseID = d.courseID,
                                        start = a.event_start,
                                        end = a.event_end
                                    }).ToList();

            return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult ScheduleByUser()
        {
            return View();
        }

        public JsonResult GetEventsByUser()
        {

            int user = Convert.ToInt32(Session["ID"]);

            var events = (from a in db.Events
                            join b in db.Courses on a.courseID equals b.courseID
                            join d in db.AssignCourse on a.courseID equals d.courseID
                            join c in db.Trainer on d.trainerID equals c.trainerID
                            where (c.trainerID == user)
                            select new {
                                eventID = a.eventID,
                                room = a.room,
                                description = a.event_descriptions,
                                courseName = b.course_name,
                                start = a.event_start,
                                end = a.event_end
                            }).ToList();



            return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult Index(int? page)
        {
            if (Session["Staff_UserName"] != null)
            {
                ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name");
                return View(db.Events.ToList().ToPagedList(page ?? 1, 20));
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
                    var events = db.Events.Include(a => a.Courses).Where(a => a.courseID == courseID);
                    return View(events.ToList().ToPagedList(page ?? 1, 20));
                }
                else
                {
                    ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name");
                    var events = db.Events.Include(a => a.Courses);
                    return View(events.ToList().ToPagedList(page ?? 1, 20));
                }
        }

        public ActionResult Create()
        {      
            if (Session["Staff_UserName"] != null)
            {
                ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name");
                var room = new List<string>() { "F.101", "F.102", "F.103", "F.201", "F.202", "F.203" };
                ViewBag.room = room;
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "eventID,event_descriptions,event_start,event_end, room, courseID")] Events events)
        {
            if (ModelState.IsValid)
            {
                var SearchData = db.Events.Where(x => x.room == events.room).Where
                                     (x => x.event_start < events.event_start && x.event_end > events.event_start 
                                     || x.event_end > events.event_end && x.event_start < events.event_end
                                     || x.event_start > events.event_start && x.event_start < events.event_end
                                     || x.event_end < events.event_end && x.event_end > events.event_start).SingleOrDefault();

                if (SearchData != null)
                {
                    ViewBag.Status = "Room not available";
                }
                else
                {
                    db.Events.Add(events);
                    db.SaveChanges();

                    try
                    {
                        AddEventToFirebase(events);
                        //ModelState.AddModelError(string.Empty, "Add successfully");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                    }

                    ViewBag.Message = "Create successfully";
                }
                

            }
            ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name");
            var room = new List<string>() { "F.101", "F.102", "F.103", "F.201", "F.202", "F.203" };
            ViewBag.room = room;
            return View(events);
        }

        public ActionResult Edit(int? id)
        {
            if (Session["Staff_UserName"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                Events events = db.Events.Find(id);


                var roomName = new List<string>() { "F.101", "F.102", "F.103", "F.201", "F.202", "F.203" };

                ViewBag.roomName = roomName;


                ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name", events.courseID);


                if (events == null)
                {
                    return HttpNotFound();
                }

                return View(events);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }     
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "eventID,event_descriptions,event_start,event_end, room, courseID")] Events events)
        {
            if (ModelState.IsValid)
            {
                var SearchData = db.Events.Where(x => x.eventID != events.eventID && x.room == events.room).Where
                                     (x => x.event_start < events.event_start && x.event_end > events.event_start
                                     || x.event_end > events.event_end && x.event_start < events.event_end
                                     || x.event_start > events.event_start && x.event_start < events.event_end
                                     || x.event_end < events.event_end && x.event_end > events.event_start).SingleOrDefault();

                if (SearchData != null)
                {
                    ViewBag.Status = "Room not available";
                }
                else
                {
                    db.Entry(events).State = EntityState.Modified;
                    db.SaveChanges();

                    client = new FireSharp.FirebaseClient(config);
                    var data = events;
                    SetResponse setResponse = client.Set("Events/" + events.eventID, data);

                    ViewBag.Message = "Edit successfully";
                }
            }
            
            ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name", events.courseID);
            var roomName = new List<string>() { "F.101", "F.102", "F.103", "F.201", "F.202", "F.203" };
            ViewBag.roomName = roomName;
            return View(events);
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
                Events events = db.Events.Find(id);
                if (events == null)
                {
                    return HttpNotFound();
                }
                return View(events);
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
            Events events = db.Events.Find(id);
            db.Events.Remove(events);
            db.SaveChanges();

            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Delete("Events/" + id);

            return RedirectToAction("Index");
        }

        private void AddEventToFirebase(Events events)
        {
            client = new FireSharp.FirebaseClient(config);
            var data = events;

            PushResponse response = client.Push("Events/" + events.eventID, data);
        }

    }
}