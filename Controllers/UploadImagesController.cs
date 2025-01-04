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
    public class UploadImagesController : Controller
    {
        private Storedb db = new Storedb();

        // GET: UploadImages
        [AllowAnonymous]
        public ActionResult Index(int id)
        {
            var uploadImages = db.UploadImages.Include(u => u.Category).Where(p=>p.PersonProfileid==id);
            return View(uploadImages.ToList());
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult DeleteFile(int Uploadid)
        {
            var ImageD = db.UploadImages.Where(u => u.Uploadid == Uploadid).FirstOrDefault();
            var fileUrl = ImageD.ImgAddress;
            var filePath = Server.MapPath("~" + fileUrl);

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            var viewresult = Json(new { error = String.Empty });
            //for IE8 which does not accept application/json
            if (Request.Headers["Accept"] != null && !Request.Headers["Accept"].Contains("application/json"))
                viewresult.ContentType = "text/plain";

            return viewresult; // trigger success
        }
        [AllowAnonymous]
        public ActionResult DownloadFile(int id, string mimetype = "image/jpeg")
        {
            var fileUrl = db.UploadImages.Where(p => p.Uploadid == id).Select(p => p.ImgAddress).FirstOrDefault();
            var filePath = Server.MapPath("~" + fileUrl);
            if (System.IO.File.Exists(filePath))
                return File(filePath, mimetype);
            else
            {
                return new HttpNotFoundResult("File not found");
            }
        }
        [AllowAnonymous]

        public FileResult DownlFile(int id)
        {
            var UrlUp = db.UploadImages.Where(p => p.Uploadid == id).FirstOrDefault();
            var fileUrl = UrlUp.ImgAddress;
            var filePath = Server.MapPath("~" + fileUrl);
            var FileName = UrlUp.PersonProfile.Fname + " " + UrlUp.PersonProfile.Lname;
            return base.File(filePath, "image/jpeg", Server.UrlEncode(UrlUp.ImageName));
        }
        // GET: UploadImages/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UploadImage uploadImage = db.UploadImages.Find(id);
            if (uploadImage == null)
            {
                return HttpNotFound();
            }
            return View(uploadImage);
        }

        // GET: UploadImages/Create
        public ActionResult Create()
        {
            ViewBag.CatImageid = new SelectList(db.Categories, "CatImageid", "CatTitle");
            return View();
        }

        // POST: UploadImages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Uploadid,CatImageid,ImgAddress,UserUpload,DateUpload,TimeUpload,LastDateEdit,LastTimeEdit,PersonProfileid,ImageName,ImageLenght")] UploadImage uploadImage)
        {
            if (ModelState.IsValid)
            {
                db.UploadImages.Add(uploadImage);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CatImageid = new SelectList(db.Categories, "CatImageid", "CatTitle", uploadImage.CatImageid);
            return View(uploadImage);
        }

        // GET: UploadImages/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UploadImage uploadImage = db.UploadImages.Find(id);
            if (uploadImage == null)
            {
                return HttpNotFound();
            }
            ViewBag.CatImageid = new SelectList(db.Categories, "CatImageid", "CatTitle", uploadImage.CatImageid);
            return View(uploadImage);
        }

        // POST: UploadImages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Uploadid,CatImageid,ImgAddress,UserUpload,DateUpload,TimeUpload,LastDateEdit,LastTimeEdit,PersonProfileid,ImageName,ImageLenght")] UploadImage uploadImage)
        {
            if (ModelState.IsValid)
            {
                db.Entry(uploadImage).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CatImageid = new SelectList(db.Categories, "CatImageid", "CatTitle", uploadImage.CatImageid);
            return View(uploadImage);
        }

        // GET: UploadImages/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UploadImage uploadImage = db.UploadImages.Find(id);
            if (uploadImage == null)
            {
                return HttpNotFound();
            }
            return View(uploadImage);
        }

        // POST: UploadImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UploadImage uploadImage = db.UploadImages.Find(id);
            db.UploadImages.Remove(uploadImage);
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
