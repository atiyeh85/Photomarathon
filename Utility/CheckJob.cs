using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using System.Net;
using System.Net.Mail;
using PhotogeraphyGrant.Models;
using System.Data.Entity;
using System.Threading.Tasks;

namespace PhotogeraphyGrant.Utility
{

    public class CheckJob : IJob
    {
        private Storedb db = new Storedb();

        public async Task Execute(IJobExecutionContext context)
        {
            var List = db.PersonProfiles
              .Where(x => x.Issend == false && x.Isshow == true)
              .ToList();

            foreach (var item in List)
            {

                if (List != null)
                {
                    string from = "darolsaltaneh.festival@gmail.com";

                    using (MailMessage mail = new MailMessage(from, item.Mail))
                    {
                        mail.Subject = "دومین جشنواره ملی عکس دارالسلطنه قزوین";
                        mail.Body = "با تشکر از شرکت شما در دومین جشنواره ملی عکس دارالسلطنه قزوین. " + "\n" + "کد امنیتی شما برای ورود مجدد به سامانه جهت ویرایش اطلاعات:" + "\n" + item.Securitycode;
                        mail.IsBodyHtml = true;
                        SmtpClient smtp = new SmtpClient();
                        smtp.Host = "smtp.gmail.com";
                        smtp.EnableSsl = true;
                        NetworkCredential networkCredential = new NetworkCredential("darolsaltaneh.festival@gmail.com", "");
                        smtp.UseDefaultCredentials = true;
                        smtp.Credentials = networkCredential;
                        smtp.Port = 587;
                        smtp.Timeout = 200000;
                        try
                        {
                            smtp.Send(mail);
                            item.Issend = true;
                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChangesAsync();
                        }
                        catch (Exception)
                        {

                        }
                    }

                }
            }
        }
    }
}