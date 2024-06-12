using System.Web;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace pcea.Controllers
{
    public class LogoutController : Controller
    {
        // GET: LogoutController
        public ActionResult Index()
        {
            ViewBag.Progress = "Logout!";
            ViewBag.Message = "You have been successfully logged out.  Click the link below to access other Apps.";
            ViewBag.SsoUrl = TempData["ncc_app_dashboard"].ToString(); // HttpUtility.UrlEncode(TempData["ncc_app_dashboard"].ToString());// "https://apps.ncc.gov.ng/#/redirect/";
            return View();
        }

    }



}
