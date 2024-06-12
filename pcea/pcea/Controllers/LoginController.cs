using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pceaLibrary;
using pcea.Models;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.IO;
using System.Web;
using pcea.Helpers;

namespace pcea.Controllers
{
    public class LoginController : Controller
    {
        private readonly PceaDbContext _DbContext;
        private readonly IHttpContextAccessor _HttpContext;
        public IConfiguration _Configuration { get; }
        Vars _Vars;
        public LoginController(PceaDbContext context, IConfiguration configuration, IHttpContextAccessor httpContext)
        {
            _DbContext = context;
            _Configuration = configuration;
            _HttpContext = httpContext;
            _Vars = new Vars(_HttpContext.HttpContext);
        }

        // GET: Login
        public ActionResult Index(string token)
        {
            ViewBag.Progress = "Validating user credentials...please wait";
            ViewBag.Token = token;
            ViewBag.SsoUrl = "https://eservices.ncc.gov.ng/#/redirect/";
            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Index(IFormCollection response)
        {
            try
            {
                string sApiResponse = string.Empty;
                string sAppKey = _Configuration.GetValue<string>("AppSettings:AppKey");
                string sAppServer = _Configuration.GetValue<string>("AppSettings:AppServer");
                string sAppValidateUserUrl = _Configuration.GetValue<string>("AppSettings:AppValidateUserURL");
                string sToken = response["token"];
                if(string.IsNullOrEmpty(sToken))
                {
                    ViewBag.Message = "Invalid login token";
                    return View("Index");
                }
                //reset token to null so that auto postack can stop
                ViewBag.Token = "";
                string url = sAppServer + sAppValidateUserUrl + sToken;

                //validate user credentials from api-server
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Headers.Add("Authorization: " + sAppKey);
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                    {
                        sApiResponse = reader.ReadToEnd();
                    }
                    if (string.IsNullOrEmpty(sApiResponse) == true || resp.StatusCode != HttpStatusCode.OK)
                    {
                        ViewBag.Message = "Invalid user credentials or server is offline.";
                        return BadRequest();
                    }
                }
                var UserInfo = JsonConvert.DeserializeObject<AlienUser>(sApiResponse);

                //log user details to SSO log
                if (UserInfo.userType.ToUpper() == Vars.UserTypes["ADMIN"].ToString())
                {
                    _DbContext.SsoUser.Add(new SsoUser
                    {
                        AppUserId = UserInfo.appUserId,
                        Username = UserInfo.username,
                        AppUserEmail = UserInfo.appUserEmail,
                        MobileNumber = UserInfo.mobileNumber,
                        UserType = UserInfo.userType.ToString().ToUpper(),
                        DateCreated = UserInfo.dateCreated,
                        FirstName = UserInfo.firstName,
                        LastName = UserInfo.lastName,
                        MiddleName = UserInfo.middleName,
                        Active = UserInfo.active,
                        AllowedToUseApi = UserInfo.allowedToUseApi,
                        EmailVerified = UserInfo.emailVerified,
                        PhoneVerified = UserInfo.phoneVerified,
                        Image = UserInfo.image,
                        OrganizationId = UserInfo.organizationId.organizationId,
                        OrganizationShortName = UserInfo.organizationId.organizationShortName,
                        OrganizationDescription = UserInfo.organizationId.organizationDescription,
                        OrganizationGroup = UserInfo.organizationId.organizationGroup,
                        OrganizationLongName = UserInfo.organizationId.organizationLongName,
                        LogoPath = UserInfo.organizationId.logoPath,
                        RoleName = UserInfo.roleId.roleName.ToString().ToUpper()
                    });
                }
                else
                {
                    _DbContext.SsoUser.Add(new SsoUser
                    {
                        AppUserId = UserInfo.appUserId,
                        Username = UserInfo.username,
                        AppUserEmail = UserInfo.appUserEmail,
                        MobileNumber = UserInfo.mobileNumber,
                        UserType = UserInfo.userType.ToString().ToUpper(),
                        DateCreated = UserInfo.dateCreated,
                        FirstName = UserInfo.firstName,
                        LastName = UserInfo.lastName,
                        MiddleName = UserInfo.middleName,
                        Active = UserInfo.active,
                        AllowedToUseApi = UserInfo.allowedToUseApi,
                        EmailVerified = UserInfo.emailVerified,
                        PhoneVerified = UserInfo.phoneVerified,
                        Image = UserInfo.image,
                        OrganizationId = UserInfo.organizationId.organizationId,
                        OrganizationShortName = UserInfo.organizationId.organizationShortName,
                        OrganizationDescription = UserInfo.organizationId.organizationDescription,
                        OrganizationGroup = UserInfo.organizationId.organizationGroup,
                        OrganizationLongName = UserInfo.organizationId.organizationLongName,
                        LogoPath = UserInfo.organizationId.logoPath,
                        RoleName = UserInfo.roleId.roleName.ToString().ToUpper(),
                        OtherInfoStreet = UserInfo.organizationId.jpaContractor.headOfficeAddressStreet,
                        OtherInfoCity = UserInfo.organizationId.jpaContractor.headOfficeAddressCity,
                        OtherInfoEmail = UserInfo.organizationId.jpaContractor.companyContactEmail,
                        OtherInfoFax = UserInfo.organizationId.jpaContractor.fax,
                        OtherInfoTelephone = UserInfo.organizationId.jpaContractor.phoneNumber,
                        OtherInfoWebsite = UserInfo.organizationId.jpaContractor.website
                    });
                }


                //determine if user is operator(public) or admin(private)
                string sUserId = string.Empty;
                string sImageUrl = UserInfo.image;
                string sPrefix = Vars.Organization.NCC.ToString();
                string sOrgId = Vars.Organization.NCC.ToString();
                string sRoleId = UserInfo.userType.ToUpper() == "PUBLIC" ? UserInfo.roleId.roleName.ToString().ToUpper() : Vars.UserRole.DEFAULT.ToString();
                string sFullname = (UserInfo.lastName + " " + UserInfo.firstName + " " + UserInfo.middleName).Trim();
                string sOrgName = UserInfo.organizationId.organizationShortName == null ? UserInfo.organizationId.organizationLongName : UserInfo.organizationId.organizationShortName;
                UserMgt _usermgt = new UserMgt();
                if (UserInfo.userType.ToUpper() == Vars.UserTypes["OPERATOR"].ToString())
                {
                    sPrefix = Vars.Organization.OPR.ToString();
                    sOrgId = UserInfo.organizationId.organizationId;
                }

                if (!UserExists(UserInfo.appUserId))
                {
                    //this is the first visit to the app
                    _DbContext.UserProfile.Add(new UserProfile
                    {
                        AppUserId = UserInfo.appUserId,
                        UserId = _usermgt.GetNewUserId(sPrefix),
                        Fullname = sFullname,
                        JobTitle = UserInfo.roleId.roleName.ToString().ToUpper(),
                        Telephone = UserInfo.mobileNumber,
                        Email = UserInfo.appUserEmail,
                        RoleId = sRoleId,
                        ImageUrl = UserInfo.image,
                        Password = _Vars.GetNewToken().Substring(0, 10),
                        Status = Vars.UserStatus["ENABLED"].ToString(),
                        UserType = UserInfo.userType.ToString().ToUpper(),
                        OrganizationId = sOrgId,
                        OrganizationName = sOrgName,
                        DateCreated = UserInfo.dateCreated,
                        DateLastLogin = DateTime.Now.Date
                    });
                }
                else
                {
                    //user has been here before
                    var _User = _DbContext.UserProfile.FirstOrDefault(s => s.AppUserId == UserInfo.appUserId);
                    sUserId = _User.UserId;
                    sRoleId = _User.RoleId;
                    sFullname = _User.Fullname;
                    sOrgId = _User.OrganizationId;
                    sOrgName = _User.OrganizationName;
                    sImageUrl = _User.ImageUrl;
                }
                _DbContext.SaveChanges(true);    //save all pending records

                if (UserInfo.userType.ToUpper() == Vars.UserTypes["OPERATOR"].ToString())
                {
                    //user is an operator(PUBLIC)
                    if (CreateCookie() == false)
                    {
                        return View("Index");
                    }
                    if (_Vars.LogOperator(sUserId, sFullname, sOrgId, sOrgName, sRoleId, sImageUrl) == false)
                    {
                        ViewBag.message = _Vars.FriendlyErrorMessage;
                        return View("Index");
                    }
                    TempData["FullName"] = _Vars.FullName;
                    TempData["UserType"] = _Vars.UserType;
                    TempData["ImageUrl"] = _Vars.ImageUrl;
                    TempData["UserId"] = _Vars.UserId;
                    TempData["RoleId"] = _Vars.RoleId;
                    TempData["OperatorId"] = _Vars.OperatorId;
                    TempData["OperatorName"] = _Vars.OperatorName;
                    TempData["SsoAppHost"] = _Vars.SSOAppHostMaster;
                }
                if (UserInfo.userType.ToUpper() == Vars.UserTypes["ADMIN"].ToString())
                {
                    //user is NCC officer(PRIVATE)
                    if (CreateCookie() == false)
                    {
                        return View("Index");
                    }
                    
                    if (_Vars.LogAdministrator(sUserId, sFullname, sRoleId) == false)
                    {
                        ViewBag.Message = _Vars.FriendlyErrorMessage;
                        return View("Index");
                    }
                    TempData["FullName"] = _Vars.FullName;
                    TempData["UserType"] = _Vars.UserType;
                    TempData["ImageUrl"] = _Vars.ImageUrl;
                    TempData["UserId"] = _Vars.UserId;
                    TempData["RoleId"] = _Vars.RoleId;
                    TempData["SsoAppHost"] = _Vars.SSOAppHostMaster;
                }



                //log user assigned menus from SSO
                string sMessage = string.Empty;
                sMessage = LogUserAssignedMenus(sApiResponse, _Vars.CurrentToken);
                if (string.IsNullOrEmpty(sMessage) == false)
                {
                    ViewBag.Message = sMessage;
                    return View("Index");
                }

                //move to dashboard
                return RedirectToAction("Dashboard", "Home");

            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
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
        private bool UserExists(string sAppUserId)
        {
            return _DbContext.UserProfile.Any(e => e.AppUserId == sAppUserId);
        }

        //code section to get list of user assigned Apps
        #region "get user assigned menu from SSO json"
        public class SSOUserApps
        {
            public List<Item> applications;
        }
        public class Item
        {
            public string applicationId;
            public string applicationName;
            public string applicationHost;
            public string publicAccessEnabled;
            public string applicationDescription;
        }
        string LogUserAssignedMenus(string json, string sToken)
        {
            try
            {
                string sAppId = string.Empty;
                string sAppName = string.Empty;
                string sAppHost = string.Empty;
                //string sAppTag = string.Empty;
                Dictionary<string, object> _data = new Dictionary<string, object>();
                _data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                string sMenus = _data["applications"].ToString();
                if (string.IsNullOrEmpty(sMenus) == false)
                {
                    SSOUserApps objMenus = JsonConvert.DeserializeObject<SSOUserApps>(json);
                    for (int i = 0; i < objMenus.applications.Count; i++)
                    {
                        sAppId += objMenus.applications[i].applicationId + "|";
                        sAppName += objMenus.applications[i].applicationName + "|";
                        sAppHost += HttpUtility.UrlEncode(objMenus.applications[i].applicationHost) + "|";
                    }
                    sAppId = sAppId.Substring(0, sAppId.Length - 1);
                    sAppName = sAppName.Substring(0, sAppName.Length - 1);
                    sAppHost = sAppHost.Substring(0, sAppHost.Length - 1);

                    if ((sAppName.ToLower().IndexOf("economic") < 0) && sAppName.ToLower().IndexOf("tariff") < 0)
                    {
                        //user does not have access to spectrum.  logout
                        return "You are not permitted to access this application";
                    }
                    _Vars.SSOAppIds = sAppId;
                    _Vars.SSOAppNames = sAppName;
                    _Vars.SSOAppHosts = sAppHost;

                    TempData["SSOAppIds"] = sAppId;
                    TempData["SSOAppNames"] = sAppName;
                    TempData["SSOAppHosts"] = sAppHost;

                    return string.Empty;
                }
                return "There is no application rights assigned to you at the moment.  Contact Administrator.";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        #endregion

    }




    public class AlienUser{
        public string appUserId { get; set; }
        public string username { get; set; }
        public string appUserEmail { get; set; }
        public string mobileNumber { get; set; }
        public string userType { get; set; }
        public DateTime dateCreated { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string middleName { get; set; }
        public string active { get; set; }
        public string allowedToUseApi { get; set; }
        public string emailVerified { get; set; }
        public string phoneVerified { get; set; }
        public string image { get; set; }

        public AlienOrg organizationId { get; set; }
        public AlienRole roleId { get; set; }
        public List<AlienApps> applications { get; set; }
    }

    public class AlienOrg {
        public Jpacontractor jpaContractor { get; set; }
        public string organizationId { get; set; }
        public DateTime dateCreated { get; set; }
        public string organizationShortName { get; set; }
        public string organizationDescription { get; set; }
        public string organizationGroup { get; set; }
        public string organizationLongName { get; set; }
        public string logoPath { get; set; }
    }
    public class Jpacontractor
    {
        public string contractorId { get; set; }
        public object tradeName { get; set; }
        public string rcNumber { get; set; }
        public string tinNumber { get; set; }
        public DateTime dateOfRegistration { get; set; }
        public string nccId { get; set; }
        public string headOfficeAddressStreet { get; set; }
        public string companyName { get; set; }
        public string phoneNumber { get; set; }
        public string fax { get; set; }
        public string website { get; set; }
        public string headOfficeAddressCity { get; set; }
        public string headOfficeAddressZip { get; set; }
        public string headOfficeAddressPobox { get; set; }
        public string headOfficeAddressPmb { get; set; }
        public string companyContactPhone1 { get; set; }
        public string companyContactPhone2 { get; set; }
        public string companyContactAlternativeEmail { get; set; }
        public string companyContactEmail { get; set; }
        public Countryofregistration countryOfRegistration { get; set; }
    }
    public class Countryofregistration
    {
        public string countryCode { get; set; }
        public string countryName { get; set; }
        public string unRegion { get; set; }
        public string unSubregion { get; set; }
    }
    public class AlienRole{
        public string roleId { get; set; }
        public string roleName { get; set; }
        public string roleDescription { get; set; }
        public DateTime dateCreated { get; set; }
    }

    public class AlienApps{
        public int ordering { get; set; }
        public string logoPath { get; set; }
        public string applicationId { get; set; }
        public string applicationName { get; set; }
        public string applicationHost { get; set; }
        public string applicationDescription { get; set; }
    }






}
