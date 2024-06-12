using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using _FrameworkCore;
using pceaLibrary;
using pcea.Models;
using Microsoft.AspNetCore.Routing;
using System.Data;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using System.Text.Json;
using DocumentFormat.OpenXml.ExtendedProperties;
using System.Web;
using pcea.Helpers;
//using System.Collections;

namespace pcea.Controllers
{
    public class HomeController : Controller
    {
        private readonly PceaDbContext _DbContext;
        private readonly IHttpContextAccessor _HttpContext;
        private IConfiguration _Configuration;
        Vars _Vars;

        public HomeController(PceaDbContext context, IHttpContextAccessor httpContext, IConfiguration configuration)
        {
            _DbContext = context;
            _HttpContext = httpContext;
            _Configuration = configuration;
            _Vars = new Vars(_HttpContext.HttpContext);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Forbidden()
        {
            return View();
        }


        public IActionResult Landing()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            try
            {
                if (_Vars.UserType.ToUpper() == Vars.UserTypes["OPERATOR"]) return View("OperatorDashboard");
                if (_Vars.UserType.ToUpper() == Vars.UserTypes["ADMIN"]) return View("AdminDashboard");

                return RedirectToAction("Index", "Logout");
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Logout");
            }
        }
        public IActionResult LogOut()
        {
            try
            {
                TempData["ncc_app_dashboard"] = _Vars.SSOAppHostMaster + HttpUtility.UrlEncode("https://apps.ncc.gov.ng/#/redirect/");
                _Vars.LogoutSession(_HttpContext.HttpContext);
                return RedirectToAction("Index", "Logout");
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Logout");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Dashboard(Login _Login)
        {
            if (IsAdmin(_Login) == true)
            {
                return View("AdminDashboard");
            }
            else if (IsOperator(_Login) == true)
            {
                return View("OperatorDashboard");
            }
            else
            {
                return View("Index");
            }

        }


        private bool CreateCookie()
        {
            try
            {
                string sCookieName = _Configuration.GetValue<string>("AppSettings:AppCookieName");
                string sCookieSpan = _Configuration.GetValue<string>("AppSettings:AppCookieSpan");
                string sToken = _Vars.GetNewToken();
                CookieOptions option = new CookieOptions
                {
                    Expires = DateTime.Now.AddHours(double.Parse(sCookieSpan))
                };
                Response.Cookies.Append(sCookieName, sToken, option);
                _Vars.CurrentToken = sToken;

                return true;
            }
            catch (Exception ex)
            {
                _Vars.TechnicalErrorMessage = ex.Message;
                ViewBag.Message = "Error setting application cookie.  Please enable cookie in your browser setting.";
                return false;
            }
        }

        /// <summary>
        /// Checks if user is an Operator.  Sets up the environment accordingly
        /// </summary>
        /// <param name="_Login"></param>
        /// <returns>True if user is an operator, False if otherwise</returns>
        private bool IsOperator(Login _Login)
        {
            try
            {
                //---this module will be changed to SSO API user management during integration
                var _user = _DbContext.UserProfile.Where(s => s.UserId == _Login.Username && s.UserType == Vars.UserTypes["OPERATOR"]);
                if (_user.Any())
                {
                    if (_user.Where(s => s.Password == _Login.Password).Any())
                    {
                        if (CreateCookie() == false)
                        {
                            return false;
                        }
                        var opp = _DbContext.UserProfile.FirstOrDefault(s => s.UserId == _Login.Username);
                        if (_Vars.LogOperator(opp.UserId, opp.Fullname) == false)
                        {
                            ViewBag.message = _Vars.FriendlyErrorMessage;
                            return false;
                        }
                        ViewBag.Fullname = _Vars.FullName;
                        ViewBag.UserType = _Vars.UserType;
                        ViewBag.ImageUrl = _Vars.ImageUrl;
                        ViewBag.UserId = _Vars.UserId;

                        //String _user = "Admin";
                        //HttpContext.Session.SetString("User", _user);
                        //ViewBag.Name = HttpContext.Session.GetString("User");
                        return true;
                    }
                    else
                    {
                        //return Json(new { status = false, message = "Invalid Password!" });   for ajax
                        ViewBag.Message = "Invalid Password!";
                        return false;
                    }
                }
                else
                {
                    //return Json(new { status = false, message = "Invalid Student ID!" });     for ajax
                    ViewBag.Message = "Invalid Username!";
                    return false;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Checks if user is NCC Staff.  Sets up the environment accordingly
        /// </summary>
        /// <param name="_Login"></param>
        /// <returns>True if user is NCC Staff, False if otherwise</returns>
        private bool IsAdmin(Login _Login)
        {
            try
            {
                //---this module will be changed to SSO API user management during integration
                var _user = _DbContext.UserProfile.Where(s => s.UserId == _Login.Username && s.UserType == Vars.UserTypes["ADMIN"]);
                if (_user.Any())
                {
                    if (_user.Where(s => s.Password == _Login.Password).Any())
                    {
                        if (CreateCookie() == false)
                        {
                            return false;
                        }
                        var opp = _DbContext.UserProfile.FirstOrDefault(s => s.UserId == _Login.Username);
                        if (_Vars.LogAdministrator(opp.UserId, opp.Fullname, opp.RoleId) == false)
                        {
                            ViewBag.Message = _Vars.FriendlyErrorMessage;
                            return false;
                        }
                        ViewBag.Fullname = _Vars.FullName;
                        ViewBag.UserType = _Vars.UserType;
                        ViewBag.ImageUrl = _Vars.ImageUrl;
                        ViewBag.UserId = _Vars.UserId;
                        ViewBag.RoleId = _Vars.RoleId;
                        return true;
                    }
                    else
                    {
                        //return Json(new { status = false, message = "Invalid Password!" });   for ajax
                        ViewBag.Message = "Invalid Password!";
                        return false;
                    }
                }
                else
                {
                    //return Json(new { status = false, message = "Invalid Student ID!" });     for ajax
                    ViewBag.Message = "Invalid Username!";
                    return false;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Fetches and makes data ready for populating the dashboard charts
        /// </summary>
        /// <returns>A json object list of all necessary data for populating dashboard charts</returns>
        public JsonResult GetCharts()
        {
            try
            {
                //Create data containers
                var HashSurForms = new HashSet<List<string>>();
                List<object> objData = new List<object>();
                var list = new List<int?>();
                var ops = new HashSet<string>();
                var sur = new List<List<ReportOperatorEntry>>();
                var check = new HashSet<string>();
                var checker = new HashSet<string>();

                //Fetch survey forms types designed in the formbuilder
                var _frm = (from m in _DbContext.Forms where m.FormsType == "OPERATOR_TYPE" && m.Published orderby m.FormsTypeCategory descending select m.FormType + "-" + m.FormsTypeCategory + "-" + m.FormYear);

                //Fetch survey operator submissions and data count by formstypecategory
                var _sur = (from m in _DbContext.ReportOperatorEntry where m.FormsType == "OPERATOR_TYPE" orderby m.FormsTypeCategory descending select m);

                HashSurForms.Add(_frm.ToList());
                sur.Add(_sur.ToList());

                //Groups the submission totals into a sequential arrangement that matches the order of survey forms
                foreach (var itm in _frm)
                {
                    list = new List<int?>();
                    var itmArray = itm.Split("-").ToList();
                    //var _check = itm.Substring(index1, index2);
                    foreach (var item in sur.FirstOrDefault().Where(e => itmArray.Contains(e.FormsTypeCategory) && itmArray.First() == e.FormType))
                    {
                        list.Add(item.TotalSubmission);
                    }
                    var count = list.Sum();
                    objData.Add(count);
                }

                _frm = _frm.Select(sl => sl.Replace("-", Environment.NewLine));


                //Fetch survey operator submissions and data count by formstypecategory
                var _tarAccepted = from m in _DbContext.FormsAndEntry where m.ReviewStatus == "APPROVED" && m.Status == "SUBMITTED" && m.FormsType == "TARIFF_TYPE" select m.ReviewStatus;
                var _tarRejected = from m in _DbContext.FormsAndEntry where m.ReviewStatus == "REJECTED" && m.Status == "SUBMITTED" && m.FormsType == "TARIFF_TYPE" select m.ReviewStatus;
                var _tarPending = from m in _DbContext.FormsAndEntry where m.ReviewStatus == "PENDING" && m.Status == "SUBMITTED" && m.FormsType == "TARIFF_TYPE" select m.ReviewStatus;

                return Json(new List<object> { new { SurveyData = objData, tarAccept = _tarAccepted.Count(), tarReject = _tarRejected.Count(), surOps = list, tarPend = _tarPending.Count(), surForms = _frm, surCount = _sur.Count() } }[0]);
            }
            catch (Exception ex)
            {
                return Json("");
            }
        }


        /// <summary>
        /// Fetches and makes data ready for populating the operator dashboard charts
        /// </summary>
        /// <returns>A json object list of all necessary data for populating operator dashboard charts</returns>
        public JsonResult OpCharts()
        {
            try
            {
                //Create data containers
                var HashSurForms = new HashSet<List<string>>();
                List<object> objData = new List<object>();
                var list = new List<int?>();
                var ops = new HashSet<string>();
                var sur = new List<List<ReportOperatorEntry>>();
                var check = new HashSet<string>();
                var checker = new HashSet<string>();


                //Fetch survey operator submissions and data count by formstypecategory
                var _sur = (from m in _DbContext.ReportOperatorEntry where m.FormsType == "OPERATOR_TYPE" && m.OperatorId == _Vars.OperatorId orderby m.FormsTypeCategory descending select m);

                //Fetch survey forms types designed in the formbuilder
                var _frm = (from m in _DbContext.Forms where m.FormsType == "OPERATOR_TYPE" orderby m.FormsTypeCategory descending  select m.FormType + "-" + m.FormsTypeCategory + "-" + m.FormYear);


                HashSurForms.Add(_frm.ToList());
                sur.Add(_sur.ToList());

                //Groups the submission totals into a sequential arrangement that matches the order of survey forms
                foreach (var itm in _frm)
                {
                    list = new List<int?>();
                    var itmArray = itm.Split("-").ToList();
                    foreach (var item in sur.FirstOrDefault().Where(e => itmArray.Contains(e.FormsTypeCategory) && itmArray.First() == e.FormType))
                    {
                        list.Add(item.TotalSubmission);
                    }
                    var count = list.Sum();
                    objData.Add(count);
                }

                _frm = _frm.Select(sl => sl.Replace("-", Environment.NewLine));
                //Fetch survey operator submissions and data count by formstypecategory
                var _tarAccepted = from m in _DbContext.FormsAndEntry where m.ReviewStatus == "APPROVED" && m.OperatorId == _Vars.OperatorId && m.Status == "SUBMITTED" && m.FormsType == "TARIFF_TYPE" select m.ReviewStatus;
                var _tarRejected = from m in _DbContext.FormsAndEntry where m.ReviewStatus == "REJECTED" && m.OperatorId == _Vars.OperatorId && m.Status == "SUBMITTED" && m.FormsType == "TARIFF_TYPE" select m.ReviewStatus;
                var _tarPending = from m in _DbContext.FormsAndEntry where m.ReviewStatus == "PENDING" && m.OperatorId == _Vars.OperatorId && m.Status == "SUBMITTED" && m.FormsType == "TARIFF_TYPE" select m.ReviewStatus;

                return Json(new List<object> { new { SurveyData = objData, tarAccept = _tarAccepted.Count(), tarReject = _tarRejected.Count(), surOps = list, tarPend = _tarPending.Count(), surForms = _frm, surCount = _sur.Count() } }[0]);
            }
            catch (Exception ex)
            {
                return Json("");
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
