using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PhotogeraphyGrant.Models;

namespace PhotogeraphyGrant.Controllers
{
    public class RefereePointsController : ControllerBase
    {
        private Storedb db = new Storedb();

        // GET: RefereePoints
        public ActionResult Index()
        {
            var refereePoints = db.RefereePoints.Include(r => r.PersonProfile);
            return View(refereePoints.ToList());
        }
        public int GetPoint()
        {
            int Sum = 0;
            var ListPerson = db.PersonProfiles.Where(p => p.Isshow == true && p.ResomehUrl != null).ToList();
            foreach (var item in ListPerson)
            {
            Sum=   item.RefereePoints.Sum(s => s.Point);
            }
            return Sum;
        }
        // GET: RefereePoints/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RefereePoint refereePoint = db.RefereePoints.Find(id);
            if (refereePoint == null)
            {
                return HttpNotFound();
            }
            return View(refereePoint);
        }

        // GET: RefereePoints/Create
        public ActionResult Create(int id)
        {

            string Name = User.Identity.Name;
            var Userid = db.AspNetUsers.Where(p => p.UserName == Name).FirstOrDefault();
            ViewBag.Name = db.PersonProfiles.Where(p => p.PersonProfileid == id).FirstOrDefault();
            var Model = db.PersonProfiles.Where(p => p.PersonProfileid == id).Select(
                p => new PhotogeraphyGrant.ViewModels.Refereevm()
                {
                    PersonProfileid = p.PersonProfileid,
                    RefereeNote = p.RefereePoints.Where(s => s.UserInsert == Userid.Id).Select(r => r.RefereeNote).FirstOrDefault(),
                    Fname = p.Fname,
                    Lname = p.Lname,
                    Point = p.RefereePoints.Where(s=>s.UserInsert==Userid.Id).Select(r => r.Point).FirstOrDefault()


                }).FirstOrDefault();
            //var Model = db.RefereePoints.Where(p => p.PersonProfileid == id).Select(
            //   p => new PhotogeraphyGrant.ViewModels.Refereevm()
            //   {
            //       PersonProfileid = id,
            //       RefereeNote = p.RefereeNote,
            //       Point = p.Point,Fname=p.PersonProfile.Fname,Lname=p.PersonProfile.Lname


            //   }).FirstOrDefault();
            return PartialView("_Create", Model);
        }

        // POST: RefereePoints/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ViewModels.Refereevm refereePointvm)
        {
           
            if (ModelState.IsValid)
            {
                string Name = User.Identity.Name;
                var Userid = db.AspNetUsers.Where(p => p.UserName == Name).FirstOrDefault();
                var Referee = db.RefereePoints.Where(r => r.UserInsert == Userid.Id).ToList();
                var PerRef = Referee.Where(p => p.PersonProfileid == refereePointvm.PersonProfileid).FirstOrDefault();

                if (PerRef != null)
                {
                    PerRef.RefereeNote = refereePointvm.RefereeNote;
                    PerRef.EditDate =Utility.PertionDate.Today();
                    PerRef.EditTime = DateTime.Now.ToShortTimeString();
                    PerRef.EditUser = Name;
                    PerRef.Point = refereePointvm.Point;
                    db.Entry(PerRef).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    var refereePoint = new RefereePoint();

                    refereePoint.DateInsert = Utility.PertionDate.Today();
                    refereePoint.PersonProfileid =refereePointvm.PersonProfileid;
                    refereePoint.Point =refereePointvm.Point;
                    refereePoint.RefereeNote = refereePointvm.RefereeNote;
                    refereePoint.UserInsert = Userid.Id;
                    refereePoint.Flag = true;
                    refereePoint.TimeInsert = DateTime.Now.ToShortTimeString();
                    db.RefereePoints.Add(refereePoint);
                    db.SaveChanges();
                }
                
                string url = Url.Action("_Create");
                return Success(url, string.Format("امتیاز ثبت شد", refereePointvm.Point), true);

            }

            return PartialView("_Create", refereePointvm);

        }

        // GET: RefereePoints/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RefereePoint refereePoint = db.RefereePoints.Find(id);
            if (refereePoint == null)
            {
                return HttpNotFound();
            }
            ViewBag.PersonProfileid = new SelectList(db.PersonProfiles, "PersonProfileid", "Fname", refereePoint.PersonProfileid);
            return View(refereePoint);
        }

        // POST: RefereePoints/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Refereeid,RefereeNote,Point,DateInsert,UserInsert,TimeInsert,PersonProfileid,Flag")] RefereePoint refereePoint)
        {
            refereePoint.PersonProfileid = 138;
            if (ModelState.IsValid)
            {
                db.Entry(refereePoint).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.PersonProfileid = new SelectList(db.PersonProfiles, "PersonProfileid", "Fname", refereePoint.PersonProfileid);
            return View(refereePoint);
        }

        // GET: RefereePoints/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RefereePoint refereePoint = db.RefereePoints.Find(id);
            if (refereePoint == null)
            {
                return HttpNotFound();
            }
            return View(refereePoint);
        }

        // POST: RefereePoints/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            RefereePoint refereePoint = db.RefereePoints.Find(id);
            db.RefereePoints.Remove(refereePoint);
            db.SaveChanges();
            return RedirectToAction("Index");
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
