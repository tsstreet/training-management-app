using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TrainingSystem.Models;
using PagedList.Mvc;
using PagedList;
using FireSharp.Interfaces;
using FireSharp.Config;
using FireSharp.Response;
using OfficeOpenXml;
using System.Windows.Input;

namespace TrainingSystem.Controllers
{
    public class CoursesController : Controller
    {
        private InternalTrainingSystemEntities db = new InternalTrainingSystemEntities();

        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "74UiAQSl7LINPN1eV2TWj7cDReSioM5YkzWRfPog",
            BasePath = "https://mvc-project-9443e-default-rtdb.firebaseio.com/"
        };
        IFirebaseClient client;

        // GET: Courses
        public ActionResult Index(int? page)
        {
            var courses = db.Courses.Include(c => c.CourseCategories);
            return View(courses.ToList().ToPagedList(page ?? 1, 5));
        }

        public ActionResult ShowCourses(int? page)
        {
            var courses = db.Courses.Include(c => c.CourseCategories);
       
            return View(courses.ToList().ToPagedList(page ?? 1, 4)); 
        }

        // GET: Courses/Details/5
        public ActionResult Details(int? id)
        {
            Courses courses = db.Courses.Find(id);
            if (courses == null)
            {
                return HttpNotFound();
            }

            ViewBag.Topic = db.Topic.ToList();
            return View(courses);

        }

        public ActionResult ViewCourseStatics()
        {
            // Populate the dropdown list with all available courses
            ViewBag.courseID = new SelectList(db.Courses, "courseID", "course_name");
            var courses = db.Courses.Include(c => c.CourseCategories);
            return View();
        }

        public JsonResult GetCourse(int courseID)
        {
            // Retrieve the course with the given ID from the database
            //var course = db.Courses.Where(c => c.courseID == courseID).ToList();

            var course = (from a in db.Courses
                          where (courseID == a.courseID)
                          select new
                          {
        
                              course_name = a.course_name,
                              startDate = a.startDate,
                              endDate = a.endDate,
                              course_duration = a.course_duration
 
                          }).ToList();

            return new JsonResult { Data = course, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            // Return the result as JSON
            //return Json(course);
        }

        // GET: Courses/Details/5
        public ActionResult ViewDetails(int? id)
        {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Courses courses = db.Courses.Find(id);

                ViewBag.AssignCourse = (from a in db.AssignCourse
                                        join c in db.Trainer on a.trainerID equals c.trainerID
                                        join d in db.Courses on a.courseID equals d.courseID
                                        where (a.courseID == id)
                                        select a).ToList();

                ViewBag.Enroll = (from a in db.Enroll
                                        join c in db.Trainee on a.traineeID equals c.traineeID
                                        join d in db.Courses on a.courseID equals d.courseID
                                        where (a.courseID == id)
                                        select a).ToList();

                if (courses == null)
                {
                    return HttpNotFound();
                }
                return View(courses);
            
        }





        [HttpGet]
        // GET: Courses/Create
        public ActionResult Create()
        {
            if (Session["Staff_Username"] != null)
            {
                ViewBag.categoryID = new SelectList(db.CourseCategories, "categoryID", "category_name");
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HttpPostedFileBase course_image, HttpPostedFileBase course_materials, [Bind(Include = "courseID,course_name,course_image,course_descriptions, course_materials, startDate, endDate, course_duration, numStd, course_status, categoryID")] Courses courses)
        {
            if (course_image != null)
            {
                //Save file
                string rootFolder = Server.MapPath("/Images/");
                string pathImage = rootFolder + course_image.FileName;
                course_image.SaveAs(pathImage);
                //Save url
                courses.course_image = course_image.FileName;
            }

            if (course_materials !=null)
            {
                //Save file
                string rootFolder = Server.MapPath("/Materials/");
                string pathMaterial = rootFolder + course_materials.FileName;
                course_materials.SaveAs(pathMaterial);
                //Save url
                courses.course_materials = course_materials.FileName;
            }
          
            if (ModelState.IsValid)
            {
                var SearchData = db.Courses.Where(x => x.course_name == courses.course_name).SingleOrDefault();

                if (SearchData != null)
                {
                    ViewBag.Status = "Course not available";
                }

                else
                {
                    
                    db.Courses.Add(courses);
                    db.SaveChanges();

                    try
                    {
                        AddCourseToFirebase(courses);                      
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                    }

                    ViewBag.Message = "Create successfully";
                }
            }

            ViewBag.categoryID = new SelectList(db.CourseCategories, "categoryID", "category_name", courses.categoryID);
            return View(courses);         
        }

        // GET: Courses/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["Staff_Username"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Courses courses = db.Courses.Find(id);
                Session["imgPath"] = courses.course_image;
                Session["materialPath"] = courses.course_materials;
                if (courses == null)
                {
                    return HttpNotFound();
                }
                ViewBag.categoryID = new SelectList(db.CourseCategories, "categoryID", "category_name", courses.categoryID);
                return View(courses);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(HttpPostedFileBase course_image, HttpPostedFileBase course_materials, [Bind(Include = "courseID,course_name,course_image,course_descriptions, course_materials, startDate, endDate, course_duration, numStd, course_status, categoryID")] Courses courses)
        {
            if (course_image !=null)
            {
                //Save file
                string rootFolder = Server.MapPath("/Images/");
                string pathImage = rootFolder + course_image.FileName;
                course_image.SaveAs(pathImage);
                //Save url
                courses.course_image = course_image.FileName;
            }
            else
            {
                courses.course_image = Session["imgPath"].ToString();             
            }

            if (course_materials !=null)
            {
                //Save file
                string rootFolder = Server.MapPath("/Materials/");
                string pathMaterial = rootFolder + course_materials.FileName;
                course_materials.SaveAs(pathMaterial);
                //Save url
                courses.course_materials = course_materials.FileName;
            }
            else
            {
                courses.course_materials = Session["materialPath"].ToString();
            }

            if (ModelState.IsValid)
            {
                var SearchData = db.Courses.Where(x => x.courseID != courses.courseID && x.course_name == courses.course_name).SingleOrDefault();

                if ( SearchData!=null )
                {
                    ViewBag.Status = "Course not available";
                }

                else
                {
                    db.Entry(courses).State = EntityState.Modified;
                    client = new FireSharp.FirebaseClient(config);
                    var data = courses;
                    SetResponse setResponse = client.Set("Courses/" + courses.courseID, data);
                    db.SaveChanges();
                    ViewBag.Message = "Edit successfully";
                }
                
            }
               
            ViewBag.categoryID = new SelectList(db.CourseCategories, "categoryID", "category_name", courses.categoryID);               
            return View(courses);

                      
        }

        // GET: Courses/Delete/5
        public ActionResult Delete(int? id)
        {
            if (Session["Staff_Username"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Courses courses = db.Courses.Find(id);
                if (courses == null)
                {
                    return HttpNotFound();
                }
                return View(courses);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            /*Courses courses = db.Courses.Find(id);
            db.Courses.Remove(courses);
            db.SaveChanges();
            return RedirectToAction("Index");*/

            try
            {
                Courses courses = db.Courses.Find(id);
                db.Courses.Remove(courses);
                db.SaveChanges();
                client = new FireSharp.FirebaseClient(config);
                FirebaseResponse response = client.Delete("Courses/" + id);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        /*public FileResult Download(string courseImage)
        {
            var FileVirtualPath = "~/Images/" + courseImage;
            return File(FileVirtualPath, "application/force- download", Path.GetFileName(FileVirtualPath));
        }*/

        public FileResult Download (string courseMaterial )
        {
            var FileVirtualPath = "~/Materials/" + courseMaterial;     
                return File(FileVirtualPath, "application/force- download", Path.GetFileName(FileVirtualPath)); 
            
        }

        public void ExportToExcel(int id)
        {
            var courses = (from a in db.Courses
                                where (a.courseID == id)
                                select new
                                {
                                    course_name = a.course_name
                                }).SingleOrDefault();

            List<TraineeViewModel> traineeList = (from a in db.Enroll
                                                  join c in db.Trainee on a.traineeID equals c.traineeID
                                                  join d in db.Courses on a.courseID equals d.courseID
                                                  where (a.courseID == id)
                                                  select new TraineeViewModel {                                                   
                                                      traineeID = c.traineeID,
                                                      trainee_fullname = c.trainee_fullname,
                                                      trainee_username = c.trainee_username,
                                                      trainee_password = c.trainee_password,
                                                      trainee_address = c.trainee_address,
                                                      trainee_age = c.trainee_age,
                                                      trainee_birthday = c.trainee_birthday,
                                                      trainee_department = c.trainee_department,
                                                      trainee_education = c.trainee_education,
                                                      trainee_email = c.trainee_email,
                                                      trainee_phone = c.trainee_phone
                                                  }).ToList();
  

            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Report");

            ws.Cells["A1"].Value = "Report";
            ws.Cells["B1"].Value = courses.course_name;

            ws.Cells["A2"].Value = "Date";
            ws.Cells["B2"].Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", DateTimeOffset.Now);

            ws.Cells["A4"].Value = "TraineeID";
            ws.Cells["B4"].Value = "Trainee Fullname";
            ws.Cells["C4"].Value = "Trainee Username";
            ws.Cells["D4"].Value = "Trainee Password";
            ws.Cells["E4"].Value = "Trainee Address";
            ws.Cells["F4"].Value = "Trainee Age";
            ws.Cells["G4"].Value = "Trainee Birthday";
            ws.Cells["H4"].Value = "Trainee Department";
            ws.Cells["I4"].Value = "Trainee Education";
            ws.Cells["J4"].Value = "Trainee Email";
            ws.Cells["K4"].Value = "Trainee Phone";


            int rowStart = 5;
            foreach (var item in traineeList)
            {
                ws.Cells[string.Format("A{0}", rowStart)].Value = item.traineeID;
                ws.Cells[string.Format("B{0}", rowStart)].Value = item.trainee_fullname;
                ws.Cells[string.Format("C{0}", rowStart)].Value = item.trainee_username;
                ws.Cells[string.Format("D{0}", rowStart)].Value = item.trainee_password;
                ws.Cells[string.Format("E{0}", rowStart)].Value = item.trainee_address;
                ws.Cells[string.Format("F{0}", rowStart)].Value = item.trainee_age;
                ws.Cells[string.Format("G{0}", rowStart)].Value = item.trainee_birthday;
                ws.Cells[string.Format("H{0}", rowStart)].Value = item.trainee_department;
                ws.Cells[string.Format("I{0}", rowStart)].Value = item.trainee_education;
                ws.Cells[string.Format("J{0}", rowStart)].Value = item.trainee_email;
                ws.Cells[string.Format("K{0}", rowStart)].Value = item.trainee_phone;
                rowStart++;
            }

            ws.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "ExcelReport.xlsx");
            Response.BinaryWrite(pck.GetAsByteArray());
            Response.End();

        }

        private void AddCourseToFirebase(Courses courses)
        {
            client = new FireSharp.FirebaseClient(config);
            var data = courses;  
            
            PushResponse response = client.Push("Courses/" + courses.courseID, data);
            //SetResponse setResponse = client.Set("Courses/", data);
        }
    }
}
