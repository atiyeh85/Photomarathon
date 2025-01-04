using PhotogeraphyGrant.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace PhotogeraphyGrant.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private Storedb db = new Storedb();

        //public ActionResult Isyoung()
        //{
        //    var list = db.PersonProfiles.ToList();
        //    foreach (var item in list)
        //    {
        //        item.IsYoung = "False";
        //        db.Entry(item).State = EntityState.Modified;
        //        db.SaveChanges();
        //    }
        //    return View();
        //}
        public ActionResult Arshive()
        {
            string UploadPath = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("/Data/"));
            //var ListU = db.UploadImages.ToList();
            var List = db.UploadImages.Where(p=>p.PersonProfile.Isshow==true).ToList();
            //var ListP = db.PersonProfiles.ToList();

            foreach (var item in List)
            {
                var CatName = "";
                var Listup = db.UploadImages.Where(p => p.PersonProfileid == item.PersonProfileid).ToList();
                if (item.ImgAddress.Contains("Archive"))
                {
                    CatName = "Archive";
                }
                else if (item.ImgAddress.Contains("Architecture"))
                {
                    CatName = "Architecture";

                }
                else if (item.ImgAddress.Contains("Mobile"))
                {
                    CatName = "Mobile";

                }
                string path = Path.Combine(UploadPath, CatName);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = Path.Combine(path, item.PersonProfile.NationalCode +"_"+item.PersonProfile.Fname+" "+item.PersonProfile.Lname);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var sourceLoc =item.ImgAddress;
                var filePath = Server.MapPath("~" + sourceLoc);
               var  fileName = System.IO.Path.GetFileName(filePath);
               var  destFile = System.IO.Path.Combine(path, fileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Copy(filePath, destFile, true);

                }


            }
            return View();

        }
    }
}
