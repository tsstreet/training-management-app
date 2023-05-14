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
using OfficeOpenXml;
using FireSharp.Interfaces;
using FireSharp.Config;
using FireSharp.Response;

namespace TrainingSystem.Controllers
{
    public class TraineeController : Controller
    {
        private InternalTrainingSystemEntities db = new InternalTrainingSystemEntities();

        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "74UiAQSl7LINPN1eV2TWj7cDReSioM5YkzWRfPog",
            BasePath = "https://mvc-project-9443e-default-rtdb.firebaseio.com/"
        };
        IFirebaseClient client;

        // GET: Trainee
        public ActionResult Index(int? page)
        {
            if (Session["Admin_UserName"] != null)
            {
                return View(db.Trainee.ToList().ToPagedList(page ?? 1, 20));
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            
        }

        // GET: Trainee/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["Staff_UserName"] != null || Session["Admin_UserName"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Trainee trainee = db.Trainee.Find(id);


                ViewBag.Enroll = (from a in db.Enroll
                                        join c in db.Trainee on a.traineeID equals c.traineeID
                                        join d in db.Courses on a.courseID equals d.courseID
                                        where (c.traineeID == id)
                                        select a).ToList();
                if (trainee == null)
                {
                    return HttpNotFound();
                }
                return View(trainee);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

            

        }

        // GET: Trainee/Create
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

        // POST: Trainee/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "traineeID,trainee_username,trainee_password,trainee_fullname,trainee_age,trainee_birthday,trainee_education,trainee_email,trainee_phone,trainee_experience_details,trainee_department,trainee_address")] Trainee trainee)
        {
            if (ModelState.IsValid)
            {
                var SearchData = db.Trainee.Where(x => x.trainee_username == trainee.trainee_username).SingleOrDefault();

                if (SearchData != null)
                {
                    ViewBag.Status = "Account not available";
                }

                else
                {
                    db.Trainee.Add(trainee);
                    db.SaveChanges();

                    try
                    {
                        AddTraineeToFirebase(trainee);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                    }

                    ViewBag.Message = "Create successfully";
                }
            }

            return View(trainee);
        }

        // GET: Trainee/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["Admin_UserName"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Trainee trainee = db.Trainee.Find(id);
                if (trainee == null)
                {
                    return HttpNotFound();
                }
                return View(trainee);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Trainee/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "traineeID, trainee_username, trainee_password, trainee_fullname, trainee_age, trainee_birthday, trainee_education, trainee_email, trainee_phone, trainee_experience_details, trainee_department, trainee_address")] Trainee trainee)
        {
            if (ModelState.IsValid)
            {
                db.Entry(trainee).State = EntityState.Modified;
                db.SaveChanges();

                client = new FireSharp.FirebaseClient(config);
                var data = trainee;
                SetResponse setResponse = client.Set("Trainee/" + trainee.traineeID, data);

                ViewBag.Message = "Edit successfully";
            }
            return View(trainee);
        }

        // GET: Trainee/Delete/5
        public ActionResult Delete(int? id)
        {
            if (Session["Admin_UserName"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Trainee trainee = db.Trainee.Find(id);
                if (trainee == null)
                {
                    return HttpNotFound();
                }
                return View(trainee);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Trainee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Trainee trainee = db.Trainee.Find(id);
            db.Trainee.Remove(trainee);
            db.SaveChanges();

            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Delete("Trainee/" + id);

            return RedirectToAction("Index");
        }


        public void ExportToExcel()
        {
            List<TraineeViewModel> traineeList = db.Trainee.Select(x => new TraineeViewModel
            {
                traineeID = x.traineeID,
                trainee_fullname = x.trainee_fullname,
                trainee_username = x.trainee_username,
                trainee_password = x.trainee_password,
                trainee_address = x.trainee_address,
                trainee_age = x.trainee_age,
                trainee_birthday = x.trainee_birthday,
                trainee_department = x.trainee_department,
                trainee_education = x.trainee_education,
                trainee_email = x.trainee_email,
                trainee_phone = x.trainee_phone
            }).ToList();

            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Report");

            ws.Cells["A1"].Value = "Report";
            ws.Cells["B1"].Value = "Report1";

            ws.Cells["A2"].Value = "Date";
            ws.Cells["B2"].Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", DateTimeOffset.Now);

            ws.Cells["A4"].Value = "traineeID";
            ws.Cells["B4"].Value = "trainee_fullname";
            ws.Cells["C4"].Value = "trainee_username";
            ws.Cells["D4"].Value = "trainee_password";
            ws.Cells["E4"].Value = "trainee_address";
            ws.Cells["F4"].Value = "trainee_age";
            ws.Cells["G4"].Value = "trainee_birthday";
            ws.Cells["H4"].Value = "trainee_department"; 
            ws.Cells["I4"].Value = "trainee_education";
            ws.Cells["J4"].Value = "trainee_email";
            ws.Cells["K4"].Value = "trainee_phone";


            int rowStart = 5;
            foreach (var item in traineeList)
            {
                ws.Cells[string.Format("A{0}", rowStart)].Value = item.traineeID;
                ws.Cells[string.Format("B{0}", rowStart)].Value = item.trainee_fullname;
                ws.Cells[string.Format("C{0}", rowStart)].Value = item.trainee_username;
                ws.Cells[string.Format("D{0}", rowStart)].Value = item.trainee_password;
                ws.Cells[string.Format("E{0}", rowStart)].Value = item.trainee_address;
                ws.Cells[string.Format("F{0}", rowStart)].Value = item.trainee_age;
                ws.Cells[string.Format("G{0}", rowStart)].Value = string.Format("{0:dd MMMM yyyy}", item.trainee_birthday);
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


        private void AddTraineeToFirebase(Trainee trainee)
        {
            client = new FireSharp.FirebaseClient(config);
            var data = trainee;

            PushResponse response = client.Push("Trainee/" + trainee.traineeID, data);
        }
    }
}
