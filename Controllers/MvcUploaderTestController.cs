using System;
using System.Collections.Generic;
using System.Web.Mvc;
using MvcFileUploader;
using MvcFileUploader.Models;
using System.IO;
using PhotogeraphyGrant.Models;
using System.Linq;

namespace PhotogeraphyGrant.Controllers
{
    [Authorize]
    public class MvcUploaderTestController : Controller
    {
        //
        // GET: /MvcUploaderTest/Demo
        string UploadPath = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("/App_Data/uploads/"));
        private Storedb db = new Storedb();

        [AllowAnonymous]
        public ActionResult Demo(bool? inline, string ui = "bootstrap")
        {
            return View(inline);
        }
        [AllowAnonymous]
		public ActionResult UploadFile(string Mobile, string NationalCode,string Category, int CatImageid,int PersonProfileid) // optionally receive values specified with Html helper
        {
            var Person = db.PersonProfiles.Where(p => p.PersonProfileid == PersonProfileid).FirstOrDefault();
            UploadImage UI = new UploadImage();
            // here we can send in some extra info to be included with the delete url 
            var statuses = new List<ViewDataUploadFileResult>();
            for (var i = 0; i < Request.Files.Count; i++)
            {
                var st = FileSaver.StoreFile(x =>
                {
                    x.File = Request.Files[i];
                    //note how we are adding an additional value to be posted with delete request
                    //and giving it the same value posted with upload
                    //x.DeleteUrl = Url.Action("DeleteFile", new { entityId = NationalCode,NationalCode=NationalCode});
                    //string path = Path.Combine(UploadPath, NationalCode);
                    string path = Path.Combine(UploadPath, Category);

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                     path = Path.Combine(path, NationalCode+"_"+Person.Fname  +" "+Person.Lname);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    x.StorageDirectory = Server.MapPath("~/App_Data/uploads/" + Category + "/"+ NationalCode + "_" + Person.Fname + " " + Person.Lname);
                    x.UrlPrefix = "/App_Data/uploads/" + Category + "/" + NationalCode + "_" + Person.Fname + " " + Person.Lname;// this is used to generate the relative url of the file
                    UI.CatImageid = CatImageid;

                    //overriding defaults
                    x.FileName = NationalCode + "_" + Request.Files[i].FileName;// default is filename suffixed with filetimestamp
                    UI.ImgAddress = "/App_Data/uploads/" + Category + "/" + NationalCode + "_" + Person.Fname + " " + Person.Lname + "/" + x.FileName;
                    UI.ImageName = x.FileName;
                    UI.PersonProfileid = PersonProfileid;
                    UI.DateUpload =Utility.PertionDate.Today();

                    db.UploadImages.Add(UI);
                    db.SaveChanges();
                    x.ThrowExceptions = true;//default is false, if false exception message is set in error property
                    x.DeleteUrl = Url.Action("DeleteFile", new { NationalCode = NationalCode, Uploadid=UI.Uploadid });

                });
                st.name = NationalCode + "_" + Request.Files[i].FileName;
                statuses.Add(st);
            }            

            //statuses contains all the uploaded files details (if error occurs then check error property is not null or empty)
            //todo: add additional code to generate thumbnail for videos, associate files with entities etc

            //adding thumbnail url for jquery file upload javascript plugin
            statuses.ForEach(x => x.thumbnailUrl = x.url + "?width=80&height=80"); // uses ImageResizer httpmodule to resize images from this url

            //setting custom download url instead of direct url to file which is default
            statuses.ForEach(x => x.url = Url.Action("DownloadFile", new { fileUrl = x.url, mimetype = x.type }));


            //server side error generation, generate some random error if entity id is 13
            if (Mobile ==" 13")
            {
                var rnd = new Random();
                statuses.ForEach(x =>
                {
                    //setting the error property removes the deleteUrl, thumbnailUrl and url property values
                    x.error = rnd.Next(0, 2) > 0 ? "We do not have any entity with unlucky Id : '13'" : String.Format("Your file size is {0} bytes which is un-acceptable", x.size);
                    //delete file by using FullPath property
                    if (System.IO.File.Exists(x.FullPath)) System.IO.File.Delete(x.FullPath);
                });
            }

            var viewresult = Json(new {files = statuses});
            //for IE8 which does not accept application/json
            if (Request.Headers["Accept"] != null && !Request.Headers["Accept"].Contains("application/json"))
                viewresult.ContentType = "text/plain";            

            return viewresult;
        }
        [AllowAnonymous]
        //here i am receving the extra info injected
        [HttpPost] // should accept only post
        public ActionResult DeleteFile(int? NationalCode, int? Uploadid, string fileUrl="")
        {

            var ImageD = db.UploadImages.Where(u => u.Uploadid == Uploadid).FirstOrDefault();
             fileUrl = ImageD.ImgAddress;
            var filePath = Server.MapPath("~" + fileUrl);
            db.UploadImages.Remove(ImageD);
            db.SaveChanges();
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            var viewresult = Json(new { error = String.Empty });
            //for IE8 which does not accept application/json
            if (Request.Headers["Accept"] != null && !Request.Headers["Accept"].Contains("application/json"))
                viewresult.ContentType = "text/plain";

            return Content("true");
        }
        [AllowAnonymous]

        public ActionResult DownloadFile(string fileUrl, string mimetype= "image/jpeg")
        {




            var filePath = Server.MapPath("~" + fileUrl);

            if (System.IO.File.Exists(filePath))
                return File(filePath, mimetype);
            else
            {
                return new HttpNotFoundResult("File not found");
            }
        }
    }
}
