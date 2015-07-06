using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BeerBubbleWeb
{
    public class BaseController : Controller
    {
        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            var anonymous = filterContext.ActionDescriptor.GetCustomAttributes(typeof(AnonymousAttribute), false);
            var api = filterContext.ActionDescriptor.GetCustomAttributes(typeof(ApiAttribute), false);

            if (anonymous.Length == 1 ) //允许匿名访问
            {

            }
            else if (api.Length == 1)
            {
                PassportEngine pe = new PassportEngine();
                LoginIdentity ui = pe.CreateIdentity(System.Web.HttpContext.Current);
                string sid = System.Web.HttpContext.Current.Request.Headers["sid"];

                if (ui.UserID > 0)
                {
                    ViewData["User"] = ui;
                }
                else if (!string.IsNullOrEmpty(sid))
                {
                    ViewData["User"] = new LoginIdentity(sid);
                }
                else
                {
                    ViewData["User"] = new LoginIdentity();
                }
            }
            else
            {
                PassportEngine pe = new PassportEngine();
                LoginIdentity ui = pe.CreateIdentity(System.Web.HttpContext.Current);

                if (ui.UserID <= 0)
                {
                    filterContext.Result = new RedirectResult(ConfigurationManager.AppSettings["LoginUrl"]);
                }

                ViewData["User"] = ui;
            }

            ViewData["iPhone"] = false;

            if (System.Web.HttpContext.Current.Request.Headers["platform"] != null && System.Web.HttpContext.Current.Request.Headers["platform"].ToLower() == "iphone")
            {
                ViewData["iPhone"] = true;
            }

            base.OnAuthorization(filterContext);
        }
    }
}
