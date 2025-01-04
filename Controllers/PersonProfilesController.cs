using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PhotogeraphyGrant.Models;
using MvcFileUploader.Models;
using MvcFileUploader;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Transactions;

namespace PhotogeraphyGrant.Controllers
{
    [Authorize]
    public class PersonProfilesController : Controller
    {
        private Storedb db = new Storedb();
        string UploadPath = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("/App_Data/uploads/"));
        string UploadPathImg = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("/Content/ImgPreview/"));
        [HttpPost]
        [AllowAnonymous]
        public ActionResult IsReady(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (var scope = new TransactionScope())
            {
                PersonProfile Person = db.PersonProfiles.Find(id);

                if (Person == null)
                {
                    return HttpNotFound();
                }
                Person.IsYoung = !Person.IsYoung;

                db.Entry(Person).State = EntityState.Modified;
                db.SaveChanges();
                scope.Complete();

                if (Person.IsYoung)
                    return Content(Boolean.TrueString);
                return Content(Boolean.FalseString);
            }
        }
        [AllowAnonymous]
        public int GetPoint()
        {
            int Sum = 0;
            var ListPerson = db.PersonProfiles.Where(p => p.Isshow == true && p.ResomehUrl != null).ToList();
            foreach (var item in ListPerson)
            {
                Sum = item.RefereePoints.Sum(s => s.Point);
            }
            return Sum;
        }
        [AllowAnonymous]
        public string GenerateString()
        {
            Random rand = new Random();

            const string Alphabet =
           "0123456789";
            int size = 3;
            char[] chars = new char[size];
            for (int i = 0; i < size; i++)
            {
                chars[i] = Alphabet[rand.Next(Alphabet.Length)];
            }

            return new string(chars);
        }
        [AllowAnonymous]
        public ActionResult ResendCode()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult ResendCode(Resend Resend)
        {
            if (ModelState.IsValid)
            {

                var ListP = db.PersonProfiles.Where(p => p.NationalCode == Resend.NationalCode && p.Mobile == Resend.Mobile).OrderByDescending(o=>o.PersonProfileid).FirstOrDefault();
                if (ListP != null)
                {
                    string from = "darolsaltaneh.festival@gmail.com";

                    using (MailMessage mail = new MailMessage(from, ListP.Mail))
                    {
                        mail.Subject = "دومین جشنواره ملی عکس دارالسلطنه قزوین";
                        mail.Body = "با تشکر از شرکت شما در دومین جشنواره ملی عکس دارالسلطنه قزوین. " + "\n" + "کد امنیتی شما برای ورود مجدد به سامانه جهت ویرایش اطلاعات:" + "\n" + ListP.Securitycode;
                        mail.IsBodyHtml = true;
                        SmtpClient smtp = new SmtpClient();
                        smtp.Host = "smtp.gmail.com";
                        smtp.EnableSsl = true;
                        NetworkCredential networkCredential = new NetworkCredential("darolsaltaneh.festival@gmail.com", "nosazi@97");
                        smtp.UseDefaultCredentials = true;
                        smtp.Credentials = networkCredential;
                        smtp.Port = 587;
                        smtp.Timeout = 200000;
                        try
                        {
                            smtp.Send(mail);
                            ListP.Issend = true;
                            db.Entry(ListP).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {

                            ListP.Issend = false;

                        }
                    }

                    TempData["Error"] = "ارسال ایمیل با موفقیت انجام شد";
                    return Redirect("http://78.38.56.77:6064/");
                }
                else
                {
                    TempData["Error"] = "مشخصاتی با این اطلاعات در سیستم ثبت نشده است";
                    return View();
                }
            }
            return View();
        }

        [AllowAnonymous]
        public JsonResult GetCity(string id)
        {
            var Ostanid = Convert.ToInt32(id);
            List<SelectListItem> City = new List<SelectListItem>();
            var List = db.Cities.Where(s => s.Ostanid == Ostanid).ToList();
            City.Add(new SelectListItem { Text = "انتخاب کنید", Value = "0" });
            foreach (var item in List)
            {
                City.Add(new SelectListItem { Text = item.CityName, Value = Convert.ToString(item.Cityid) });
            }
            return Json(new SelectList(City, "Value", "Text"));
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult DeleteFile(int? id)
        {

            var ImageD = db.UploadImages.Where(u => u.Uploadid == id).FirstOrDefault();
            var fileUrl = ImageD.ImgAddress;
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
        public ActionResult Search()
        {
             TempData["Message"] = TempData["Error"];
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Search(Search search)
        {
            if (ModelState.IsValid)
            {
                var NationalCode = Utility.PertionDate.PtoE(search.NationalCode);
                var Mobile = Utility.PertionDate.PtoE(search.Mobile);
                var Securitycode = Utility.PertionDate.PtoE(search.Securitycode);
              
                var ListP = db.PersonProfiles.Where(p=>p.Isshow==true)
                    .Where(p => p.NationalCode == NationalCode && p.Mobile == Mobile && p.Securitycode == Securitycode).FirstOrDefault();
                
                var List1 = db.PersonProfiles.Where(p => p.Isshow == true).Where(p => p.NationalCode == NationalCode).AsQueryable();
                var List2 = List1.Where(p=>p.Mobile==Mobile );
                var hh = List1.ToList();
                var List3 = hh.Where(p=>p.Securitycode==Securitycode);
                var hh3 = List3.FirstOrDefault();

                if (hh3 != null)
                {
                    Session["PersonProfile"] = hh3;
                    return RedirectToAction("PersonalImage", new { id = hh3.PersonProfileid });
                }
                else
                {
                    ViewBag.Error = "اطلاعات وارد شده مجاز نمی باشد";

                    return View();
                }

            }


            return View();

        }
        // GET: PersonProfiles
        public ActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!User.IsInRole("Admin"))
            {
                
                return RedirectToAction("ListPerson");
            }
            var personProfiles = db.PersonProfiles.Where(c => c.Isshow == true).ToList();
            return View(personProfiles);

        }
        public ActionResult ListPerson()
        {
            
            var List = db.PersonProfiles.Where(c => c.Isshow == true && c.ResomehUrl !=null).ToList();
            string Name = User.Identity.Name;
            var Userid = db.AspNetUsers.Where(p => p.UserName == Name).FirstOrDefault();
            ViewBag.Referee = db.RefereePoints.Where(p => p.UserInsert == Userid.Id).ToList();
            var Model = List.Select(s => new ViewModels.Refereevm()
            {
                 PersonProfileid=s.PersonProfileid,
                Fname = s.Fname,
                Lname = s.Lname,
                NationalCode = s.Fname,
                ResomehUrl = s.ResomehUrl,
                ImageUrl = s.Image,
                Degreeid = s.Degreeid,
                Reshteh = s.Reshteh,
                Country = s.Country,
                Ostanid = s.Ostanid,
                Cityid = s.Cityid,
                Note = s.Note,
                Degree = s.Degree,
                IsYoung = s.IsYoung,
                City = s.City,
                Gender = s.Gender,
                Point=s.RefereePoints.Sum(m=>m.Point)

            }).ToList();
            var list = Model.ToList();
            return View(Model);
        }
        [AllowAnonymous]
        public ActionResult Send()
        {
            var List = db.PersonProfiles.Where(c => c.Isshow == true && c.Issend == false).ToList();
            foreach (var item in List)
            {

                var Per = db.PersonProfiles.Where(c => c.PersonProfileid == item.PersonProfileid).FirstOrDefault();
                    string from = "darolsaltaneh.festival@gmail.com";

                    using (MailMessage mail = new MailMessage(from, item.Mail))
                    {
                        mail.Subject = "دومین جشنواره ملی عکس دارالسلطنه قزوین";
                        mail.Body = "با تشکر از شرکت شما در دومین جشنواره ملی عکس دارالسلطنه قزوین. " + "\n" + "کد امنیتی شما برای ورود مجدد به سامانه جهت ویرایش اطلاعات:" + "\n" + item.Securitycode;
                        mail.IsBodyHtml = false;
                        SmtpClient smtp = new SmtpClient();
                        smtp.Host = "smtp.gmail.com";
                        smtp.EnableSsl = true;
                        NetworkCredential networkCredential = new NetworkCredential("darolsaltaneh.festival@gmail.com", "nosazi@97");
                        smtp.UseDefaultCredentials = true;
                        smtp.Credentials = networkCredential;
                        smtp.Port = 587;
                        smtp.Timeout = 200000;
                    smtp.Send(mail);
                }
               
                Per.Issend = true;
                db.Entry(Per).State = EntityState.Modified;
                db.SaveChanges();
            }
            return View();

        }
        [AllowAnonymous]
        // GET: PersonProfiles/Details/5
        public ActionResult Details(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var UserLogin = (PersonProfile)Session["PersonProfile"];
            var Userid = 0;
            if (UserLogin != null)
            {
                Userid = UserLogin.PersonProfileid;
            }
            if (!User.Identity.IsAuthenticated)
            {
                if (null == Session["PersonProfile"])
                {
                    return Redirect("http://78.38.56.77:6064/");
                }
                else
                {
                    if (Userid != id)
                    {
                        return Redirect("http://78.38.56.77:6064/");
                    }
                }
            }
            PersonProfile personProfile = db.PersonProfiles.Where(p => p.PersonProfileid == id).FirstOrDefault();
            if (personProfile == null)
            {
                return HttpNotFound();
            }
            return View(personProfile);
        }



        public ActionResult PersonDetails(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (!User.Identity.IsAuthenticated)
            {
                if (null == Session["PersonProfile"])
                {
                    return Redirect("http://78.38.56.77:6064/");
                }
                else
                {
                }
            }
            PersonProfile personProfile = db.PersonProfiles.Where(p => p.PersonProfileid == id).FirstOrDefault();
            if (personProfile == null)
            {
                return HttpNotFound();
            }
            return View(personProfile);
        }

        [AllowAnonymous]
        public ActionResult PersonalImage(int? id, bool? inline, string ui = "bootstrap")
        {
            var UserLogin = (PersonProfile)Session["PersonProfile"];
            var Userid = 0;
            if (UserLogin != null)
            {
                Userid = UserLogin.PersonProfileid;
            }
            if (!User.Identity.IsAuthenticated)
            {
                if (null == Session["PersonProfile"])
                {
                    return Redirect("http://78.38.56.77:6064/");
                }
                else
                {
                    if (Userid != id)
                    {
                        return Redirect("http://78.38.56.77:6064/");
                    }
                }
            }
            var List = db.PersonProfiles.Where(p => p.PersonProfileid == id).FirstOrDefault();
            return View(List);
        }
        [AllowAnonymous]
        // POST: PersonProfiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        public ActionResult PersonalImagee(int PersonProfileid, string NationalCode, string ImageUrl, string ResomehUrl, string Image)
        {
            var UserLogin = (PersonProfile)Session["PersonProfile"];
            var Userid = 0;
            if (UserLogin != null)
            {
                Userid = UserLogin.PersonProfileid;
            }
            if (!User.Identity.IsAuthenticated)
            {
                if (null == Session["PersonProfile"])
                {
                    return Redirect("http://78.38.56.77:6064/");
                }
                else
                {
                    if (Userid != PersonProfileid)
                    {
                        return Redirect("http://78.38.56.77:6064/");
                    }
                }
            }
            var UI = db.PersonProfiles.Where(p => p.PersonProfileid == PersonProfileid).FirstOrDefault();
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
                    string path = Path.Combine(UploadPath, NationalCode);
                    string pathImg = Path.Combine(UploadPathImg, NationalCode);

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    if (!Directory.Exists(pathImg))
                    {
                        Directory.CreateDirectory(pathImg);
                    }
                    if (!string.IsNullOrEmpty(Image))
                    {
                        HttpPostedFileBase file = Request.Files.Count > 0 ? Request.Files[0] : null;
                        x.StorageDirectory = Server.MapPath("~/Content/ImgPreview/" + NationalCode);
                        x.UrlPrefix = "/Content/ImgPreview/" + NationalCode;// this is used to generate the relative url of the file
                        //overriding defaults

                        x.FileName = NationalCode + "_" + Request.Files[i].FileName;// default is filename suffixed with filetimestamp
                        UI.Image = "/Content/ImgPreview/" + NationalCode + "/" + x.FileName;
                        pathImg = Path.Combine(pathImg, x.FileName);
                        file.SaveAs(pathImg);
                        db.Entry(UI).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    if (!string.IsNullOrEmpty(ImageUrl))
                    {
                        path = Path.Combine(path, ImageUrl);
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        x.StorageDirectory = Server.MapPath("~/App_Data/uploads/" + NationalCode + "/" + ImageUrl);
                        x.UrlPrefix = "/App_Data/uploads/" + NationalCode + "/" + ImageUrl;// this is used to generate the relative url of the file
                        //overriding defaults
                        x.FileName = NationalCode + "_" + Request.Files[i].FileName;// default is filename suffixed with filetimestamp
                        UI.ImageUrl = "/App_Data/uploads/" + NationalCode + "/" + ImageUrl + "/" + x.FileName;
                        x.DeleteUrl = Url.Action("DeleteFile2", new { NationalCode = NationalCode, PersonProfileid = UI.PersonProfileid, ImageUrl = "ImageUrl" });

                        db.Entry(UI).State = EntityState.Modified;

                        db.SaveChanges();
                    }
                    else if (!string.IsNullOrEmpty(ResomehUrl))
                    {
                        path = Path.Combine(path, ResomehUrl);
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        x.StorageDirectory = Server.MapPath("~/App_Data/uploads/" + NationalCode + "/" + ResomehUrl);
                        x.UrlPrefix = "/App_Data/uploads/" + NationalCode + "/" + ResomehUrl;// this is used to generate the relative url of the file

                        //overriding defaults
                        x.FileName = NationalCode + "_" + Request.Files[i].FileName;// default is filename suffixed with filetimestamp
                        x.DeleteUrl = Url.Action("DeleteFile2", new
                        {
                            NationalCode = NationalCode,
                            PersonProfileid = UI.PersonProfileid,
                            ResomehUrl = "ResomehUrl"
                        });

                        UI.ResomehUrl = "/App_Data/uploads/" + NationalCode + "/" + ResomehUrl + "/" + x.FileName;
                        db.Entry(UI).State = EntityState.Modified;

                        db.SaveChanges();
                    }




                    x.ThrowExceptions = true;//default is false, if false exception message is set in error property

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
            if (NationalCode == " 13")
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

            var viewresult = Json(new { files = statuses });
            //for IE8 which does not accept application/json
            if (Request.Headers["Accept"] != null && !Request.Headers["Accept"].Contains("application/json"))
                viewresult.ContentType = "text/plain";
            return viewresult;
        }
        [AllowAnonymous]
        public ActionResult DownloadFile(string fileUrl, string mimetype = "image/jpeg")
        {


            var filePath = Server.MapPath("~" + fileUrl);

            if (System.IO.File.Exists(filePath))
                return File(filePath, "image/jpeg", "Image");
            //return File(filePath, mimetype);
            else
            {
                return new HttpNotFoundResult("فایل پیدا نشد");
            }
        }
        [AllowAnonymous]
        public ActionResult DownloadResomehUrl(int id, string mimetype = "image/jpeg")
        {
            var UserLogin = (PersonProfile)Session["PersonProfile"];
            var Userid = 0;
            if (UserLogin != null)
            {
                Userid = UserLogin.PersonProfileid;
            }
            if (!User.Identity.IsAuthenticated)
            {
                if (null == Session["PersonProfile"])
                {
                    return Redirect("http://78.38.56.77:6064/");
                }
                else
                {
                    if (Userid != id)
                    {
                        return Redirect("http://78.38.56.77:6064/");
                    }
                }
            }
            var Url = db.PersonProfiles.Where(p => p.PersonProfileid == id).FirstOrDefault();
            var fileUrl = Url.ResomehUrl;
            var filePath = Server.MapPath("~" + fileUrl);

            if (System.IO.File.Exists(filePath))
                return File(filePath, "application/pdf", Server.UrlEncode(filePath));
            else
            {
                return new HttpNotFoundResult("فایل پیدا نشد");
            }
        }
        
        
        public FileResult downloadI(int id)
        {
            var Url = db.PersonProfiles.Where(p => p.PersonProfileid == id).FirstOrDefault();
            var fileUrl = Url.ImageUrl;
            var filePath = Server.MapPath("~" + fileUrl);
            var FileName =  Url.Fname + " " + Url.Lname; 
            return base.File(filePath, "Image/jpeg", FileName);
        }
        public FileResult downloadR(int id)
        {
            var Url = db.PersonProfiles.Where(p => p.PersonProfileid == id).FirstOrDefault();
            var fileUrl = Url.ResomehUrl;
            var filePath = Server.MapPath("~" + fileUrl);
            var FileName = "cv" + "_" + Url.Fname + " " + Url.Lname;
            return File(filePath, "application/pdf", Server.UrlEncode(FileName));
        }
        [AllowAnonymous]
        public ActionResult DownloadImgUrl(int id, string mimetype = "image/jpeg")
        {

            var Url = db.PersonProfiles.Where(p => p.PersonProfileid == id).FirstOrDefault();
            var fileUrl = Url.ImageUrl;
            var filePath = Server.MapPath("~" + fileUrl);

            if (System.IO.File.Exists(filePath))
                return File(filePath, mimetype);
            else
            {
                return new HttpNotFoundResult("فایل پیدا نشد");
            }
        }
        [AllowAnonymous]
        [HttpPost] // should accept only post
        public ActionResult DeleteImgurl(int? id)
        {
            var ImageD = db.PersonProfiles.Where(u => u.PersonProfileid == id).FirstOrDefault();
            var fileUrl = ImageD.ImageUrl;
            var fileImage = ImageD.Image;
            ImageD.ImageUrl = null;
            ImageD.Image = null;
            var filePath = Server.MapPath("~" + fileUrl);
            var fileImagePath = Server.MapPath("~" + fileImage);
            db.Entry(ImageD).State = EntityState.Modified;
            db.SaveChanges();
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
            if (System.IO.File.Exists(fileImagePath))
                System.IO.File.Delete(fileImagePath);
            var viewresult = Json(new { error = String.Empty });
            //for IE8 which does not accept application/json
            if (Request.Headers["Accept"] != null && !Request.Headers["Accept"].Contains("application/json"))
                viewresult.ContentType = "text/plain";

            return Content("true");
        }
        [AllowAnonymous]
        [HttpPost] // should accept only post
        public ActionResult DeleteResomehurl(int? id)
        {
            var ImageD = db.PersonProfiles.Where(u => u.PersonProfileid == id).FirstOrDefault();
            var fileUrl = ImageD.ImageUrl;
            ImageD.ImageUrl = null;
            var filePath = Server.MapPath("~" + fileUrl);
            db.Entry(ImageD).State = EntityState.Modified;
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
        [HttpPost] // should accept only post
        public ActionResult DeleteFile2(int? NationalCode, int? PersonProfileid, string ImageUrl = "", string ResomehUrl = "")
        {
            var ImageD = db.PersonProfiles.Where(u => u.PersonProfileid == PersonProfileid).FirstOrDefault();
            var fileUrl = ImageD.ImageUrl;
            ImageD.ImageUrl = null;
            var filePath = Server.MapPath("~" + fileUrl);
            db.Entry(ImageD).State = EntityState.Modified;
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
        // GET: PersonProfiles/Create
        public ActionResult Create(bool? inline, string ui = "bootstrap")
        {

            ViewBag.Degreeid = new SelectList(db.Degrees, "Degreeid", "DegreeTitle");
            ViewBag.Cityid = new SelectList(db.Cities, "Cityid", "CityName");
            ViewBag.Ostanid = new SelectList(db.Ostans, "Ostanid", "OstanName");
            ViewBag.Genderid = new SelectList(db.Genders, "Genderid", "GenderTitle");
            ViewBag.UploadId = new SelectList(db.UploadImages, "Uploadid", "ImgAddress");
            return View();
        }

        // POST: PersonProfiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PersonProfile personProfile)
        {
            //var NationalCode = Utility.PertionDate.PtoE(personProfile.NationalCode);
            //var ListP = db.PersonProfiles.Where(p=>p.NationalCode==NationalCode);
            //if (ListP != null)
            //{
            //    ViewBag.Error = "این کد ملی قبلا در سامانه ثبت شده است.";
            //    ViewBag.Degreeid = new SelectList(db.Degrees, "Degreeid", "DegreeTitle");
            //    ViewBag.Cityid = new SelectList(db.Cities, "Cityid", "CityName");
            //    ViewBag.Ostanid = new SelectList(db.Ostans, "Ostanid", "OstanName");
            //    ViewBag.Genderid = new SelectList(db.Genders, "Genderid", "GenderTitle");
            //    return View();
            //}
            if (ModelState.IsValid)
            {
                
                personProfile.Isshow = true;
                personProfile.Mobile = Utility.PertionDate.PtoE(personProfile.Mobile);
                personProfile.Securitycode = Utility.PertionDate.PtoE(personProfile.NationalCode) + GenerateString();
                personProfile.NationalCode = Utility.PertionDate.PtoE(personProfile.NationalCode);
                personProfile.DateInsert = Utility.PertionDate.Today();
                personProfile.UserInsert = DateTime.Now.ToShortTimeString();
                db.PersonProfiles.Add(personProfile);
                db.SaveChanges();
                Session["PersonProfile"] = personProfile;
            
                return RedirectToAction("PersonalImage", new { id = personProfile.PersonProfileid });

               


                  
            }
            ViewBag.Ostanid = new SelectList(db.Ostans, "Ostanid", "OstanName");
            ViewBag.Degreeid = new SelectList(db.Degrees, "Degreeid", "DegreeTitle", personProfile.Degreeid);
            ViewBag.Cityid = new SelectList(db.Cities, "Cityid", "CityName", personProfile.Cityid);
            ViewBag.Genderid = new SelectList(db.Genders, "Genderid", "GenderTitle", personProfile.Genderid);
            ViewBag.UploadId = new SelectList(db.UploadImages, "Uploadid", "ImgAddress", personProfile.UploadId);
            return View(personProfile);
        }
        [AllowAnonymous]
        // GET: PersonProfiles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var UserLogin = (PersonProfile)Session["PersonProfile"];
            var Userid = 0;
            if (UserLogin != null)
            {
                Userid = UserLogin.PersonProfileid;
            }
            if (!User.Identity.IsAuthenticated)
            {
                if (null == Session["PersonProfile"])
                {
                    return Redirect("http://78.38.56.77:6064/");
                }
                else
                {
                    if (Userid != id)
                    {
                        return Redirect("http://78.38.56.77:6064/");
                    }
                }
            }
            PersonProfile personProfile = db.PersonProfiles.Find(id);
            if (personProfile == null)
            {
                return HttpNotFound();
            }
            ViewBag.Degreeid = new SelectList(db.Degrees, "Degreeid", "DegreeTitle", personProfile.Degreeid);

            ViewBag.Ostanid = new SelectList(db.Ostans, "Ostanid", "OstanName", personProfile.City.Ostanid);
            ViewBag.Cityid = new SelectList(db.Cities, "Cityid", "CityName", personProfile.Cityid);
            ViewBag.Genderid = new SelectList(db.Genders, "Genderid", "GenderTitle", personProfile.Genderid);
            ViewBag.UploadId = new SelectList(db.UploadImages, "Uploadid", "ImgAddress", personProfile.UploadId);
            return View(personProfile);
        }

        // POST: PersonProfiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PersonProfile personProfile, int hh)
        {
            if (personProfile.PersonProfileid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var UserLogin = (PersonProfile)Session["PersonProfile"];
            var Userid = 0;
            if (UserLogin != null)
            {
                Userid = UserLogin.PersonProfileid;
            }
            if (!User.Identity.IsAuthenticated)
            {
                if (null == Session["PersonProfile"])
                {
                    return Redirect("http://78.38.56.77:6064/");
                }
                else
                {
                    if (Userid != personProfile.PersonProfileid)
                    {
                        return Redirect("http://78.38.56.77:6064/");
                    }
                }
            }
            var Plist = db.PersonProfiles.Where(p => p.PersonProfileid == personProfile.PersonProfileid).FirstOrDefault();
            if (personProfile.Cityid == 0)
            {
                Plist.Cityid = hh;
            }
            else
            {
                Plist.Cityid = personProfile.Cityid;

            }
            Plist.Ostanid = personProfile.Ostanid;
            Plist.PostalCode = personProfile.PostalCode;
            Plist.Mobile = personProfile.Mobile;
            Plist.NationalCode = personProfile.NationalCode;
            Plist.Telephone = personProfile.Telephone;
            Plist.Address = personProfile.Address;
            Plist.FatherName = personProfile.FatherName;
            Plist.Fname = personProfile.Fname;
            Plist.Lname = personProfile.Lname;
            Plist.Genderid = personProfile.Genderid;
            Plist.Country = personProfile.Country;
            Plist.Note = personProfile.Note;
            Plist.Issend = personProfile.Issend;
            Plist.Degreeid = personProfile.Degreeid;

            if (ModelState.IsValid)
            {
                db.Entry(Plist).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = personProfile.PersonProfileid });

            }
            ViewBag.Degreeid = new SelectList(db.Degrees, "Degreeid", "DegreeTitle", personProfile.Degreeid);

            ViewBag.Ostanid = new SelectList(db.Ostans, "Ostanid", "OstanName", personProfile.City.Ostanid);
            ViewBag.Cityid = new SelectList(db.Cities, "Cityid", "CityName", personProfile.Cityid);
            ViewBag.Genderid = new SelectList(db.Genders, "Genderid", "GenderTitle", personProfile.Genderid);
            ViewBag.UploadId = new SelectList(db.UploadImages, "Uploadid", "ImgAddress", personProfile.UploadId);
            return View(personProfile);
        }

        // GET: PersonProfiles/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PersonProfile personProfile = db.PersonProfiles.Find(id);
            if (personProfile == null)
            {
                return HttpNotFound();
            }
            return View(personProfile);
        }

        // POST: PersonProfiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PersonProfile personProfile = db.PersonProfiles.Find(id);
            db.PersonProfiles.Remove(personProfile);
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
