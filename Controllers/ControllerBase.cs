
using PhotogeraphyGrant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PhotogeraphyGrant.Controllers
{
    
    public class ControllerBase : Controller
    {
        public int PageSize = 4;
        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
        }

        #region Set Error And Success Messages
        protected void SetSuccessMessage(string message, params string[] args)
        {
            ViewBag.SuccessMessage = string.Format(message, args);
        }

        protected void SetErrorMessage(string message, params string[] args)
        {
            ViewBag.ErrorMessage = string.Format(message, args);
        }
        #endregion

        #region Error messages

        public JsonResult Success(string url, string message, bool dismissable = false)
        {
            return BuildMessage(true, url, message, AlertStyles.Success, dismissable);

        }
        public JsonResult Information(string url, string message, bool dismissable = false)
        {
            return BuildMessage(false, url, message, AlertStyles.Information, dismissable);

        }
        public JsonResult Warning(string url, string message, bool dismissable = false)
        {
            return BuildMessage(false, url, message, AlertStyles.Warning, dismissable);

        }

        public JsonResult Danger(string url, string message, bool dismissable = false)
        {
            return BuildMessage(false, url, message, AlertStyles.Danger, dismissable);

        }

        private JsonResult BuildMessage(bool resultCondition, string url, string message, string AlertStyle, bool dismissable)
        {
            return Json(new
            {
                success = resultCondition,
                url = url,
                msg = message,
                alertType = AlertStyle,
                dismissableButton = dismissable
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}