using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using pcea.Models;
using pceaLibrary;

namespace pcea
{
    public class AppResultFilter : Attribute, IResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context)
        {

        }
        public void OnResultExecuting(ResultExecutingContext context)
        {
            //context.HttpContext.Response.Headers.Add("Author", new string[] { _value });
        }
        private void Log(string methodName, RouteData routeData)
        {
            var controllerName = routeData.Values["controller"];
            var actionName = routeData.Values["action"];
            var message = String.Format("{0}- controller:{1} action:{2}", methodName,
                                                                        controllerName,
                                                                        actionName);
            //Debug.WriteLine(message);
        }
    }

    public class AppAsyncResultFilter : IAsyncResultFilter
    {
        //Vars _Vars;
        public async Task OnResultExecutionAsync(ResultExecutingContext context,
                                                 ResultExecutionDelegate next)
        {
            if (!(context.Result is EmptyResult))
            {
                // Do something before the action executes.
                var result = await next();
                // Do something after the action executes.
            }
        }
    }


    public class AppActionFilter : IActionFilter
    {
        Vars _Vars;
        private readonly PceaDbContext _DbContext;
        private readonly IHttpContextAccessor _HttpContext;
        private readonly IWebHostEnvironment environment;
        private readonly IActionResult _actionResult;
        private ViewDataDictionary viewData;

        public AppActionFilter(PceaDbContext context, IHttpContextAccessor httpContext, IWebHostEnvironment environment)
        {
            _DbContext = context;
            _HttpContext = httpContext;
            this.environment = environment;
            _Vars = new Vars(httpContext.HttpContext);
            //_AuditTrail = new AuditTrail(environment);
        }
        public void OnActionExecuting(ActionExecutingContext context)
       {
            //viewdata to pass messages to the view of the current controller context
            viewData = (ViewDataDictionary)context.Controller
                            .GetType()
                            .GetProperty("ViewData").GetValue(context.Controller);

            // Do something before the action executes.
            var controllerName = context.RouteData.Values["controller"];
            var actionName = context.RouteData.Values["action"];
            var message = String.Format("controller:{0} action:{1}", controllerName, actionName);
            var user = _DbContext.UserProfile.SingleOrDefault(x => x.UserId == _Vars.UserId);


            if (!(context.Result is EmptyResult))
            {
                Controller controller = context.Controller as Controller;
                try
                {
                    if (!string.IsNullOrEmpty(_Vars.UserType.ToString()))
                    {
                        if (Vars.AdminControllers.ContainsKey(controllerName.ToString().ToUpper()))
                        {
                            var _user = Vars.UserTypes.Where(e => e.Value == _Vars.UserType);
                            if (_user.Any())
                            {
                                var Uid = user.UserId.Substring(0, 3);
                                if (Uid != Vars.AdminUsersType.NCC.ToString())
                                {
                                    context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                                    {
                                        action = "Forbidden",
                                        controller = "Home"
                                    }));
                                }

                                if (string.IsNullOrEmpty(user.RoleId))
                                {
                                    context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                                    {
                                        action = "Forbidden",
                                        controller = "Home",

                                    }));
                                }

                                if (!_DbContext.AppRole.Any(x => x.RoleId == user.RoleId))
                                {
                                    context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                                    {
                                        action = "Forbidden",
                                        controller = "Home"
                                    }));
                                }

                                var priviledges = _DbContext.UserPrivilege.Where(x => x.RoleId == user.RoleId)
                                    .Select(sl => new
                                    {
                                        privilege = _DbContext.AppPrivilege.FirstOrDefault(e => e.PrivilegeId == sl.PrivilegeId)
                                    })
                                    .ToList();

                                if (priviledges.Count > 0)
                                {
                                    if (!priviledges.Any(a => a.privilege.ControllerName.Contains(controllerName.ToString())
                                    && a.privilege.ActionName.Split(",").Contains(actionName.ToString())))
                                    {
                                        context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                                        {
                                            action = "Forbidden",
                                            controller = "Home"
                                        }));
                                    }

                                    if (actionName.ToString().ToLower().Contains("tariff"))
                                    {
                                        var eval = priviledges.Any(a => a.privilege.ActionName == "EvaluateTariff");
                                        var letter = priviledges.Any(a => a.privilege.ActionName == "LetterUpload");
                                        var final = priviledges.Any(a => a.privilege.ActionName == "ApproveTariff");

                                        controller.ViewBag.eval = eval;
                                        controller.ViewBag.letter = letter;
                                        controller.ViewBag.final = final;
                                    }

                                    var final_A = priviledges.Any(a => a.privilege.ActionName.Contains("TaskApproval"));
                                    var final_R = priviledges.Any(a => a.privilege.ActionName.Contains("TaskRejection"));

                                    controller.ViewBag.approve = final_A;
                                    controller.ViewBag.reject = final_R;
                                }



                                //foreach (var item in priviledges)
                                //{
                                //    var appPrivilege = _DbContext.AppPrivilege.FirstOrDefault(d => d.PrivilegeId == item.PrivilegeId && d.ActionName == actionName.ToString());

                                //    if (appPrivilege.ControllerName != controllerName.ToString() || appPrivilege.ActionName != actionName.ToString() && item.Assigned == false)
                                //    {
                                //        context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                                //        {
                                //            action = "Forbidden",
                                //            controller = "Home"
                                //        }));

                                //        break;
                                //    }
                                //}
                            }
                            else
                            {
                                if (Vars.AdminControllers.ContainsKey(controllerName.ToString().ToUpper()) && _Vars.UserType != Vars.AdminUsersType.NCC.ToString())
                                {
                                    context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                                    {
                                        action = "Forbidden",
                                        controller = "Home"
                                    }));
                                }
                            }
                        }

                        if (controllerName.ToString() == "Survey")
                        {
                            if (_Vars.UserType == Vars.AdminUsersType.NCC.ToString())
                            {
                                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                                {
                                    action = "Forbidden",
                                    controller = "Home"
                                }));
                            }

                            //var user = _DbContext.UserProfile.SingleOrDefault(x => x.UserId == _Vars.UserId);

                            if (user == null)
                            {
                                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                                {
                                    action = "Forbidden",
                                    controller = "Home"
                                }));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Do something after the action executes.
            //MyDebug.Write(MethodBase.GetCurrentMethod(), context.HttpContext.Request.Path);
            if (!(context.Result is EmptyResult))
            {
                _Vars = new Vars(context.HttpContext);

                Controller controller = context.Controller as Controller;
                if (_Vars.UserType.ToUpper() == Vars.UserTypes["ADMIN"].ToString())
                {
                    //if (context.Controller is Controller controller)
                    if (controller != null)
                    {
                        /*
                        controller.ViewBag.UserId = _Vars.UserId;
                        controller.ViewBag.FullName = _Vars.FullName;
                        //controller.ViewBag.UserType = _Vars.UserType;
                        controller.ViewBag.ImageUrl = _Vars.ImageUrl;
                        controller.ViewBag.RoleId = _Vars.RoleId;
                        controller.ViewBag.DocumentPath = _Vars.DocumentPath;
                        controller.ViewBag.ImagePath = _Vars.ImagePath;
                        controller.ViewBag.ProcessId = _Vars.ProcessId;
                        controller.ViewBag.TaskId = _Vars.TaskId;
                        controller.ViewBag.AppIds = _Vars.SSOAppIds;
                        controller.ViewBag.AppNames = _Vars.SSOAppNames;
                        controller.ViewBag.AppHosts = _Vars.SSOAppHosts;
                        controller.ViewBag.SsoAppHost = _Vars.SSOAppHostMaster;
                        */

                        controller.TempData["UserId"] = _Vars.UserId;
                        controller.TempData["FullName"] = _Vars.FullName;
                        //controller.TempData["UserType"] = _Vars.UserType;
                        controller.TempData["ImageUrl"] = _Vars.ImageUrl;
                        controller.TempData["RoleId"] = _Vars.RoleId;
                        controller.TempData["DocumentPath"] = _Vars.DocumentPath;
                        controller.TempData["ImagePath"] = _Vars.ImagePath;
                        //controller.TempData["ProcessId"] = _Vars.ProcessId;
                        //controller.TempData["TaskId"] = _Vars.TaskId;
                        controller.TempData["AppIds"] = _Vars.SSOAppIds;
                        controller.TempData["AppNames"] = _Vars.SSOAppNames;
                        controller.TempData["AppHosts"] = _Vars.SSOAppHosts;
                        controller.TempData["SsoAppHost"] = _Vars.SSOAppHostMaster;
                        controller.TempData["SSOAppIds"] = _Vars.SSOAppIds;
                        controller.TempData["SSOAppNames"] = _Vars.SSOAppNames;
                        controller.TempData["SSOAppHosts"] = _Vars.SSOAppHosts;

                        //count pending tasks
                        controller.TempData["PendingTaskCount"] = _Vars.PendingTaskCount;
                    }
                }
                if (_Vars.UserType.ToUpper() == Vars.UserTypes["OPERATOR"].ToString())
                {
                    if (controller != null)
                    {
                        /*
                        controller.ViewBag.OperatorId = _Vars.OperatorId;
                        controller.ViewBag.UserId = _Vars.UserId;
                        controller.ViewBag.FullName = _Vars.FullName;
                        //controller.ViewBag.UserType = _Vars.UserType;
                        controller.ViewBag.ImageUrl = _Vars.ImageUrl;
                        controller.ViewBag.RoleId = _Vars.RoleId;
                        controller.ViewBag.OperatorId = _Vars.OperatorId;
                        controller.ViewBag.OperatorName = _Vars.OperatorName;
                        controller.ViewBag.DocumentPath = _Vars.DocumentPath;
                        controller.ViewBag.ImagePath = _Vars.ImagePath;
                        controller.ViewBag.AppIds = _Vars.SSOAppIds;
                        controller.ViewBag.AppNames = _Vars.SSOAppNames;
                        controller.ViewBag.AppHosts = _Vars.SSOAppHosts;
                        controller.ViewBag.SsoAppHost = _Vars.SSOAppHostMaster;
                        */

                        controller.TempData["OperatorId"] = _Vars.OperatorId;
                        controller.TempData["UserId"] = _Vars.UserId;
                        controller.TempData["FullName"] = _Vars.FullName;
                        //controller.TempData["UserType"] = _Vars.UserType;
                        controller.TempData["ImageUrl"] = _Vars.ImageUrl;
                        controller.TempData["RoleId"] = _Vars.RoleId;
                        controller.TempData["OperatorId"] = _Vars.OperatorId;
                        controller.TempData["OperatorName"] = _Vars.OperatorName;
                        controller.TempData["DocumentPath"] = _Vars.DocumentPath;
                        controller.TempData["ImagePath"] = _Vars.ImagePath;
                        controller.TempData["AppIds"] = _Vars.SSOAppIds;
                        controller.TempData["AppNames"] = _Vars.SSOAppNames;
                        controller.TempData["AppHosts"] = _Vars.SSOAppHosts;
                        controller.TempData["SsoAppHost"] = _Vars.SSOAppHostMaster;
                        controller.TempData["SSOAppIds"] = _Vars.SSOAppIds;
                        controller.TempData["SSOAppNames"] = _Vars.SSOAppNames;
                        controller.TempData["SSOAppHosts"] = _Vars.SSOAppHosts;
                    }
                }
            }
            /*else
            {
                context.Canceled = true;
            }*/
        }
    }

    public class AppAsyncActionFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            // Do something before the action executes.

            // next() calls the action method.
            var resultContext = await next();
            // resultContext.Result is set.
            // Do something after the action executes.
        }
    }




    /*public class LogAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            Log("OnActionExecuted", filterContext.RouteData);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Log("OnActionExecuting", filterContext.RouteData);
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            Log("OnResultExecuted", filterContext.RouteData);
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            Log("OnResultExecuting ", filterContext.RouteData);
        }

        private void Log(string methodName, RouteData routeData)
        {
            var controllerName = routeData.Values["controller"];
            var actionName = routeData.Values["action"];
            var message = String.Format("{0}- controller:{1} action:{2}", methodName,
                                                                        controllerName,
                                                                        actionName);
            Debug.WriteLine(message);
        }
    }*/




}
