using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using pcea.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Newtonsoft.Json;
using System.Text.Encodings.Web;
using System.Text;
using System.Web;
using pceaLibrary;
using pcea.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace pcea.Controllers
{
    public class WorkflowsController : Controller
    {
        private readonly PceaDbContext _DbContext;
        private readonly IHttpContextAccessor _HttpContext;
        Vars _Vars;
        TaskHelper _taskHelper;
        private IConfiguration _Config;
        public Stream InputStream { get; }
        public void ProcessRequest(HttpContext context)
        {
            using (var reader = new StreamReader(context.Request.Body))
            {
                string values = reader.ReadToEnd();
            }
        }
        public WorkflowsController(PceaDbContext context, IHttpContextAccessor httpContext, IHostingEnvironment hosting, IConfiguration configuration)
        {
            _DbContext = context;
            _HttpContext = httpContext;
            _Vars = new Vars(_HttpContext.HttpContext);
            _taskHelper = new TaskHelper(_DbContext, httpContext, hosting);
            _Config = configuration;
        }

        // GET: Workflows
        public async Task<IActionResult> Index()
        {
            return View(await _DbContext.Workflow.ToListAsync());
        }

        // GET: Workflows/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workflow = await _DbContext.Workflow
                .FirstOrDefaultAsync(m => m.ProcessId == id);
            if (workflow == null)
            {
                return NotFound();
            }

            return View(workflow);
        }

        // GET: Workflows/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Workflows/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProcessId,ProcessName,DateCreated,Status")] Workflow workflow)
        {
            if (ModelState.IsValid)
            {
                _DbContext.Add(workflow);
                await _DbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(workflow);
        }

        // GET: Workflows/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workflow = await _DbContext.Workflow.FindAsync(id);
            if (workflow == null)
            {
                return NotFound();
            }
            return View(workflow);
        }

        // POST: Workflows/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ProcessId,ProcessName,DateCreated,Status")] Workflow workflow)
        {
            if (id != workflow.ProcessId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _DbContext.Update(workflow);
                    await _DbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkflowExists(workflow.ProcessId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(workflow);
        }

        // GET: Workflows/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workflow = await _DbContext.Workflow
                .FirstOrDefaultAsync(m => m.ProcessId == id);
            if (workflow == null)
            {
                return NotFound();
            }

            return View(workflow);
        }

        // POST: Workflows/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var workflow = await _DbContext.Workflow.FindAsync(id);
            _DbContext.Workflow.Remove(workflow);
            await _DbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        private bool WorkflowExists(string id)
        {
            return _DbContext.Workflow.Any(e => e.ProcessId == id);
        }


        [HttpGet]
        public IActionResult DeleteWorkflow(string id)
        {
            try
            {
                //check if form already got entries

                _DbContext.Workflow.Remove(_DbContext.Workflow.FirstOrDefault(m => m.ProcessId == id));
                _DbContext.SaveChanges();

                TempData["message"] = "Form deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
        /// <summary>
        /// Gets the name of the process
        /// </summary>
        /// <param name="ProcessName"></param>
        /// <returns>Returns the add new view</returns>
        public IActionResult getWorkflow(string ProcessName)
        {
            return PartialView("AddNewWorkflow", ProcessName);
        }

        public async Task<IActionResult> NewProcess(string sStatus, string sProcessid)
        {
            try
            {
                var newWorkflow = await _DbContext.Workflow.FirstOrDefaultAsync(m => m.ProcessName == sProcessid);
            if (newWorkflow == null)
            {
                var _newProcess = new Workflow();
                var _processid = sProcessid.Replace(" ", "_");

                _newProcess.ProcessName = sProcessid;
                _newProcess.ProcessId = _processid;
                _newProcess.Status = sStatus;
                _DbContext.Add(_newProcess);
                await _DbContext.SaveChangesAsync();
                return Ok();
            }
                return View();
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest();
            }
        }
        //Fetches the workflow builder
        public async Task<IActionResult> GetWorkflowBuild(string sProcessId)
        {
            var loadWorkflow = await _DbContext.Workflow.FirstOrDefaultAsync(m => m.ProcessId == sProcessId);
            if (loadWorkflow != null)
            {
                var data = loadWorkflow.ProcessData;
                var flowData = HttpUtility.UrlDecode(data);
                ViewBag.flowData = flowData;
            }

            ViewBag.Processid = sProcessId;
            return PartialView("Build", await _DbContext.AppRole.ToListAsync());
        }

        private bool WorkflowActorExists(string processId)
        {
            return _DbContext.WorkflowActor.Any(e => e.ProcessId == processId);
        }

        //SAVE WORKFLOW DATA
        [HttpPost]
        public async Task<IActionResult> SaveFlowData(string sProcessdata, string sProcessid, List<WorkflowActor> workflowActors, List<WorkflowLink> workflowLinks)
        {
            try
            {
                var newWorkflow = await _DbContext.Workflow.Where(m => m.ProcessId == sProcessid).FirstOrDefaultAsync();

                if (WorkflowActorExists(sProcessid))
                {
                    _DbContext.WorkflowActor.RemoveRange(_DbContext.WorkflowActor.Where(m => m.ProcessId == sProcessid));
                    _DbContext.WorkflowLink.RemoveRange(_DbContext.WorkflowLink.Where(m => m.ProcessId == sProcessid));
                    await _DbContext.SaveChangesAsync(true);
                }

                foreach (var actor in workflowActors)
                {
                    _DbContext.WorkflowActor.Add(actor);
                }

                foreach(var link in workflowLinks)
                {
                    _DbContext.WorkflowLink.Add(link);
                }
               
                if (newWorkflow != null)
                {
                    var flowdata = sProcessdata;
                    newWorkflow.ProcessData = flowdata;
                    _DbContext.Update(newWorkflow);
                }
                await _DbContext.SaveChangesAsync(true);
                return Ok();
            }
            catch (Exception ex)
            {
                ViewBag.error = ex.Message + " " + ex.InnerException + " " + "Workflow Saving Failed";
                return RedirectToAction(nameof(GetWorkflowBuild)); ;
            }
        }




        //--------------------NEW WORKFLOW MANAGER MODULES-----------------------------
        public async Task<List<SelectListItem>> GetUserByWorkflow(string ActionUrl)
        {
            var TaskList = new List<SelectListItem>();
            string sUplinkRoleId = string.Empty, sDownlinkRoleId = string.Empty;

            var wfManager = await _DbContext.WorkflowManager.Where(m => m.ActionUrl == ActionUrl && m.UserId == _Vars.UserId)
                .OrderByDescending(o => o.RecId).FirstOrDefaultAsync();
            if (wfManager == null)
            {
                ViewBag.TaskMessage = "The current work page is not in the list of tasks assigned to you.";
                return TaskList;
            }

            //check if this actor's role exists in the workflow manager
            var wfActor = await _DbContext.WorkflowActor.Where(m => m.ProcessId == wfManager.ProcessId && m.ActorName == wfManager.RoleId)
                .FirstOrDefaultAsync();
            if (wfActor == null)
            {
                ViewBag.TaskMessage = "Your role is not defined in the workflow for the work page.";
                return TaskList;
            }

            /*
            //get role above this actor in the workflow manager
            var wfUpLink = await _DbContext.WorkflowLink.Where(m => m.ProcessId == wfActor.ProcessId && m.ToActorNumber == wfActor.ActorNumber)
                .FirstOrDefaultAsync();
            if (wfUpLink != null)
            {
                //get userrole above this actor in the workflow
                var wfUplinkActor = await _DbContext.WorkflowActor.Where(m => m.ProcessId == wfManager.ProcessId && m.ActorNumber == wfUpLink.FromActorNumber)
                .FirstOrDefaultAsync();
                if (wfUplinkActor != null)
                {
                    sUplinkRoleId = wfUplinkActor.ActorName;
                }
            }

            //get role below this actor in the workflow manager
            var wfDownLink = await _DbContext.WorkflowLink.Where(m => m.ProcessId == wfActor.ProcessId && m.FromActorNumber == wfActor.ActorNumber)
                .FirstOrDefaultAsync();
            if (wfDownLink != null)
            {
                //get userrole below this actor in the workflow
                var wfDownlinkActor = await _DbContext.WorkflowActor.Where(m => m.ProcessId == wfManager.ProcessId && m.ActorNumber == wfDownLink.ToActorNumber)
                .FirstOrDefaultAsync();
                if (wfDownlinkActor != null)
                {
                    sDownlinkRoleId = wfDownlinkActor.ActorName;
                }
            }

            //get users in the uplink and downlink of this user into the UserList dropdown
            TaskList = _DbContext.UserProfile
                .Where(m => m.RoleId.ToUpper() == sUplinkRoleId.ToUpper() || m.RoleId.ToUpper() == sDownlinkRoleId.ToUpper())
                .Select(a => new SelectListItem
                {
                    Value = a.UserId,
                    Text = a.RoleId + " - " + a.Fullname
                }).ToList();
            */

            //NEW REQUIREMENT:
            //get all roles in this workflow from the workflow actors table
            string sRoles = string.Empty;
            var wfActorList = await _DbContext.WorkflowActor.Where(m => m.ProcessId == wfManager.ProcessId).ToListAsync();
            foreach(WorkflowActor _Actor in wfActorList)
            {
                sRoles += "'" + _Actor.ActorName + "',";
            }

            if(!string.IsNullOrEmpty(sRoles))
            {
                sRoles = sRoles[0..^1];
            }

            //get all users in the workflow into the UserList dropdown instead of the uplink and downlink of this user 
            TaskList = _DbContext.UserProfile.FromSqlRaw("SELECT * FROM UserProfile WHERE RoleId IN (" + sRoles +")")
                .Select(a => new SelectListItem
                {
                    Value = a.UserId,
                    Text = a.RoleId + " - " + a.Fullname
                }).ToList();

            //wfManager.UserList = AppendSecretaryToList(TaskList, wfActorList, wfManager.ProcessId);
            ////get workflow actions
            //wfManager.ActionList = _DbContext.MetaDataRef.Where(m => m.MetaDataType.ToUpper() == Vars.MetaData.WORKFLOW_ACTION.ToString())
            //    .Select(a => new SelectListItem
            //    {
            //        Value = a.ReferenceId,
            //        Text = a.ReferenceId
            //    }).ToList();

            return TaskList;
        }

        [HttpPost]
        public IActionResult BulkAssign(IFormCollection resp)
        {
            var taskIds = resp["taskIds"].ToString().Split(",");
            //Dictionary<string, string> keys = new Dictionary<string, string>();

            var response = _taskHelper.ReAssignTasks(taskIds, resp["UserId"], resp["Remarks"]);
            return RedirectToAction("TaskList");
        }

        public async Task<IActionResult> TaskList()
        {
            var TaskList = await _DbContext.WorkflowManager.Where(m => m.UserId == _Vars.UserId && m.IsSource == false && m.CompletionFlag == "NO").OrderByDescending(o => o.DateAssigned).ToListAsync();
            List<SelectListItem> result = new List<SelectListItem>();
            if (TaskList.Count > 0)
                result = await GetUserByWorkflow(TaskList.FirstOrDefault().ActionUrl);

            //var reassignList = _DbContext.WorkflowManager.Where(e => e.CompletionFlag == "NO" && e.IsSource == false).Select(sl => new {
            //    DateAssigned = sl.DateAssigned,
            //    TaskType = sl.TaskType,
            //    UserId = _DbContext.AppUserProfileView.FirstOrDefault(e => e.UserId == sl.UserId).Username,
            //    ProcessId = sl.ProcessId,
            //    OperatorName = sl.OperatorName,
            //    TaskId = sl.TaskId
            //}).OrderByDescending(o => o.DateAssigned).AsQueryable().Cast<List<WorkflowManager>>();

            var reassignList = _DbContext.WorkflowManager.Where(e => e.UserId == _Vars.UserId && e.CompletionFlag == "NO" && e.IsSource == false).Select(sl => new WorkflowManager()
            {
                TaskId = sl.TaskId,
                DateAssigned = sl.DateAssigned,
                UserId = _DbContext.AppUserProfileView.SingleOrDefault(s => s.UserId == sl.UserId).Username,
                TaskType = sl.TaskType,
                ProcessId = sl.ProcessId,
                OperatorName = sl.OperatorName
            }).ToList();
            var wrkMgt = new WorkflowManager()
            {
                UserList = result
            };

            ViewData["UserList"] = wrkMgt;
            ViewData["reassignList"] = reassignList;

            return PartialView("TaskList", TaskList);
        }

        public async Task<IActionResult> GetTaskTrailByUrl(string ActionUrl)
        {
            if (ActionUrl.Contains("SurveyDetails2"))
            {
                ActionUrl = ActionUrl.Replace("SurveyDetails2", "SurveyDetails");
            }

            if (ActionUrl.Contains("ApproveTariff"))
            {
                ActionUrl = ActionUrl.Replace("ApproveTariff", "ViewTariff");
            }

            if (ActionUrl.Contains("localhost"))
            {
                var uri = new Uri(ActionUrl);
                var host = uri.Host;
                var port = uri.Port;
                
                ActionUrl = ActionUrl.Replace($"{host}:{port}", "pcea.ncc.gov.ng");
            }

            var trails = await _DbContext.WorkflowManager.Where(m => m.ActionUrl == ActionUrl).OrderByDescending(o => o.RecId).ToListAsync();
            var ids = trails.Select(sl => sl.UserId).ToList();

            var usersintrail = await _DbContext.UserProfile.Where(w => ids.Contains(w.UserId)).ToListAsync();

            trails.ForEach((t) =>
            {
                t.AdminFullName = usersintrail.FirstOrDefault(f => f.UserId == t.UserId).Fullname;
            });

            return PartialView("TaskTrail", trails);
        }

        public async Task<IActionResult> NewTaskByUrl(string ActionUrl)
        {
            //string sUplinkRoleId = string.Empty, sDownlinkRoleId = string.Empty;

            //check if this actor's role exists in the workflow manager
            var wfActor = await _DbContext.WorkflowActor.Where(m => m.ActorName == _Vars.RoleId)
                .FirstOrDefaultAsync();
            if (wfActor == null)
            {
                ViewBag.TaskMessage = "Your role is not defined in any workflow.";
                return PartialView("TaskError");
            }

            /*
            //get role above this actor in the workflow manager
            var wfUpLink = await _DbContext.WorkflowLink.Where(m => m.ProcessId == wfActor.ProcessId && m.ToActorNumber == wfActor.ActorNumber)
                .FirstOrDefaultAsync();
            if (wfUpLink != null)
            {
                //get userrole above this actor in the workflow
                var wfUplinkActor = await _DbContext.WorkflowActor.Where(m => m.ProcessId == wfUpLink.ProcessId && m.ActorNumber == wfUpLink.FromActorNumber)
                .FirstOrDefaultAsync();
                if (wfUplinkActor != null)
                {
                    sUplinkRoleId = wfUplinkActor.ActorName;
                }
            }

            //get role below this actor in the workflow manager
            var wfDownLink = await _DbContext.WorkflowLink.Where(m => m.ProcessId == wfActor.ProcessId && m.FromActorNumber == wfActor.ActorNumber)
                .FirstOrDefaultAsync();
            if (wfDownLink != null)
            {
                //get userrole below this actor in the workflow
                var wfDownlinkActor = await _DbContext.WorkflowActor.Where(m => m.ProcessId == wfDownLink.ProcessId && m.ActorNumber == wfDownLink.ToActorNumber)
                .FirstOrDefaultAsync();
                if (wfDownlinkActor != null)
                {
                    sDownlinkRoleId = wfDownlinkActor.ActorName;
                }
            }
            */

            //NEW REQUIREMENT:
            //get all roles in this workflow from the workflow actors table
            string sRoles = string.Empty;
            var wfActorList = await _DbContext.WorkflowActor.Where(m => m.ProcessId == wfActor.ProcessId).ToListAsync();
            foreach (WorkflowActor _Actor in wfActorList)
            {
                sRoles += "'" + _Actor.ActorName + "',";
            }
            if (!string.IsNullOrEmpty(sRoles))
            {
                sRoles = sRoles[0..^1];
            }

            WorkflowManager wfManager = new WorkflowManager
            {
                ActionUrl = ActionUrl,
                //get workflow actions
                ActionList = _DbContext.MetaDataRef.Where(m => m.MetaDataType.ToUpper() == Vars.MetaData.WORKFLOW_ACTION.ToString())
                .Select(a => new SelectListItem
                {
                    Value = a.ReferenceId,
                    Text = a.ReferenceId
                }).ToList(),

                //get operator list
                OperatorList = _DbContext.UserProfile.Distinct()
                .Select(a => new SelectListItem
                {
                    Value = a.OrganizationId,
                    Text = a.OrganizationName
                }).ToList(),

                //get process list
                ProcessList = _DbContext.Workflow.
                    FromSqlRaw("SELECT * FROM [dbo].[Workflow] WHERE [ProcessId] IN " +
                        "(SELECT [ProcessId] FROM [dbo].[WorkflowActor] WHERE [ActorName] = '" + _Vars.RoleId + "')").Distinct()
                .Select(a => new SelectListItem
                {
                    Value = a.ProcessId,
                    Text = a.ProcessName
                }).ToList(),

                /*
                //get users in the uplink and downlink of this user into the UserList dropdown
                UserList = _DbContext.UserProfile
                    .Where(m => m.RoleId.ToUpper() == sUplinkRoleId.ToUpper() || m.RoleId.ToUpper() == sDownlinkRoleId.ToUpper())
                .Select(a => new SelectListItem
                {
                    Value = a.UserId,
                    Text = a.RoleId + " - " + a.Fullname
                }).ToList()
                */

                //get all users in the workflow into the UserList dropdown instead of the uplink and downlink of this user 
                UserList = _DbContext.UserProfile.FromSqlRaw("SELECT * FROM UserProfile WHERE RoleId IN (" + sRoles + ")")
                .Select(a => new SelectListItem
                {
                    Value = a.UserId,
                    Text = a.RoleId + " - " + a.Fullname
                }).ToList()

            };

            //wfManager.UserList = AppendSecretaryToList(wfManager.UserList, wfActorList, wfManager.ProcessId);

            return PartialView("TaskNew", wfManager);
        }


        public async Task<IActionResult> GetTaskByUrl(string ActionUrl)
        {
            string sUplinkRoleId = string.Empty, sDownlinkRoleId = string.Empty;

            if(ActionUrl.Contains("SurveyDetails2"))
            {
                ActionUrl = ActionUrl.Replace("SurveyDetails2", "SurveyDetails");
            }

            if (ActionUrl.Contains("ApproveTariff"))
            {
                ActionUrl = ActionUrl.Replace("ApproveTariff", "ViewTariff");
            }

            if (ActionUrl.Contains("localhost"))
            {
                ActionUrl = ActionUrl.Replace("localhost:44345", "pcea.ncc.gov.ng");
            }


            var wfManager = await _DbContext.WorkflowManager.Where(m => m.ActionUrl == ActionUrl && m.UserId == _Vars.UserId)
                .OrderByDescending(o => o.RecId).FirstOrDefaultAsync();
            if (wfManager == null)
            {
                ViewBag.TaskMessage = "The current work page is not in the list of tasks assigned to you.";
                return PartialView("TaskError");
            }
            
            if (wfManager.CompletionFlag.ToUpper() == "YES")
            {
                ViewBag.TaskMessage = "The current work page is no LONGER in the list of tasks assigned to you.";
                return PartialView("TaskError");
            }

            /*
            //check if this actor's role exists in the workflow manager
            var wfActor = await _DbContext.WorkflowActor.Where(m => m.ProcessId == wfManager.ProcessId && m.ActorName == wfManager.RoleId )
                .FirstOrDefaultAsync();
            if (wfActor == null)
            {
                ViewBag.TaskMessage = "Your role is not defined in the workflow for the work page.";
                return PartialView("TaskError");
            }

            //get role above this actor in the workflow manager
            var wfUpLink = await _DbContext.WorkflowLink.Where(m => m.ProcessId == wfActor.ProcessId && m.ToActorNumber == wfActor.ActorNumber)
                .FirstOrDefaultAsync();
            if (wfUpLink != null)
            {
                //get userrole above this actor in the workflow
                var wfUplinkActor = await _DbContext.WorkflowActor.Where(m => m.ProcessId == wfManager.ProcessId && m.ActorNumber == wfUpLink.FromActorNumber)
                .FirstOrDefaultAsync();
                if (wfUplinkActor != null)
                {
                    sUplinkRoleId = wfUplinkActor.ActorName;
                }
            }

            //get role below this actor in the workflow manager
            var wfDownLink = await _DbContext.WorkflowLink.Where(m => m.ProcessId == wfActor.ProcessId && m.FromActorNumber == wfActor.ActorNumber)
                .FirstOrDefaultAsync();
            if (wfDownLink != null)
            {
                //get userrole below this actor in the workflow
                var wfDownlinkActor = await _DbContext.WorkflowActor.Where(m => m.ProcessId == wfManager.ProcessId && m.ActorNumber == wfDownLink.ToActorNumber)
                .FirstOrDefaultAsync();
                if (wfDownlinkActor != null)
                {
                    sDownlinkRoleId = wfDownlinkActor.ActorName;
                }
            }

            //get users in the uplink and downlink of this user into the UserList dropdown
            var groupByUpLink = new SelectListGroup() { Name = "UpLinkActors" };
            var groupByDownLink = new SelectListGroup() { Name = "DownLinkActors" };

            wfManager.UserList = _DbContext.UserProfile
                .Where(m => m.RoleId.ToUpper() == sDownlinkRoleId.ToUpper())
                .Select(a => new SelectListItem
                {
                    Value = a.UserId,
                    Text = a.RoleId + " - " + a.Fullname,
                    Group = groupByDownLink
                }).ToList();
            wfManager.UserList.AddRange(_DbContext.UserProfile
                .Where(m => m.RoleId.ToUpper() == sUplinkRoleId.ToUpper())
                .Select(a => new SelectListItem
                {
                    Value = a.UserId,
                    Text = a.RoleId + " - " + a.Fullname,
                    Group = groupByUpLink
                }).ToList());
            */

            //NEW REQUIREMENT:
            //get all roles in this workflow from the workflow actors table
            string sRoles = string.Empty;
            var wfActorList = await _DbContext.WorkflowActor.Where(m => m.ProcessId == wfManager.ProcessId).ToListAsync();
            foreach (WorkflowActor _Actor in wfActorList)
            {
                sRoles += "'" + _Actor.ActorName + "',";
            }

            if (!string.IsNullOrEmpty(sRoles))
            {
                sRoles = sRoles[0..^1];
            }

            //get all users in the workflow into the UserList dropdown instead of the uplink and downlink of this user 
            //if(_Vars.RoleId.ToLower() == "secretary")
            //{
            //    //include only end of task as option in user list dropdown
            //    wfManager.UserList = new List<SelectListItem>();
            //    wfManager.UserList.Add(new SelectListItem
            //        {
            //            Value = "TERMINATE_PROCESS",
            //            Text = "Terminate Process"
            //        });
            //}
            //else
            //{
                wfManager.UserList = _DbContext.UserProfile.FromSqlRaw("SELECT * FROM UserProfile WHERE RoleId IN (" + sRoles + ")")
                    .Select(a => new SelectListItem
                    {
                        Value = a.UserId,
                        Text = a.RoleId + " - " + a.Fullname
                    }).ToList();
            //}

            //wfManager.UserList = AppendSecretaryToList(wfManager.UserList, wfActorList, wfManager.ProcessId);

            //get workflow actions
            wfManager.ActionList = _DbContext.MetaDataRef.Where(m => m.MetaDataType.ToUpper() == Vars.MetaData.WORKFLOW_ACTION.ToString())
                .Select(a => new SelectListItem
            {
                Value = a.ReferenceId,
                Text = a.ReferenceId
            }).ToList();

            return PartialView("TaskDetails", wfManager);
        }

        public async Task<IActionResult> GetTaskByUrlSec(string ActionUrl, string AppendSecFlag)
        {
            string sUplinkRoleId = string.Empty, sDownlinkRoleId = string.Empty;

            if (ActionUrl.Contains("SurveyDetails2"))
            {
                ActionUrl = ActionUrl.Replace("SurveyDetails2", "SurveyDetails");
            }

            if (ActionUrl.Contains("ApproveTariff"))
            {
                ActionUrl = ActionUrl.Replace("ApproveTariff", "ViewTariff");
            }

            if (ActionUrl.Contains("localhost"))
            {
                ActionUrl = ActionUrl.Replace("localhost:44345", "pcea.ncc.gov.ng");
            }


            var wfManager = await _DbContext.WorkflowManager.Where(m => m.ActionUrl == ActionUrl && m.UserId == _Vars.UserId)
                .OrderByDescending(o => o.RecId).FirstOrDefaultAsync();
            if (wfManager == null)
            {
                ViewBag.TaskMessage = "The current work page is not in the list of tasks assigned to you.";
                return PartialView("TaskError");
            }

            /*
            //check if this actor's role exists in the workflow manager
            var wfActor = await _DbContext.WorkflowActor.Where(m => m.ProcessId == wfManager.ProcessId && m.ActorName == wfManager.RoleId )
                .FirstOrDefaultAsync();
            if (wfActor == null)
            {
                ViewBag.TaskMessage = "Your role is not defined in the workflow for the work page.";
                return PartialView("TaskError");
            }

            //get role above this actor in the workflow manager
            var wfUpLink = await _DbContext.WorkflowLink.Where(m => m.ProcessId == wfActor.ProcessId && m.ToActorNumber == wfActor.ActorNumber)
                .FirstOrDefaultAsync();
            if (wfUpLink != null)
            {
                //get userrole above this actor in the workflow
                var wfUplinkActor = await _DbContext.WorkflowActor.Where(m => m.ProcessId == wfManager.ProcessId && m.ActorNumber == wfUpLink.FromActorNumber)
                .FirstOrDefaultAsync();
                if (wfUplinkActor != null)
                {
                    sUplinkRoleId = wfUplinkActor.ActorName;
                }
            }

            //get role below this actor in the workflow manager
            var wfDownLink = await _DbContext.WorkflowLink.Where(m => m.ProcessId == wfActor.ProcessId && m.FromActorNumber == wfActor.ActorNumber)
                .FirstOrDefaultAsync();
            if (wfDownLink != null)
            {
                //get userrole below this actor in the workflow
                var wfDownlinkActor = await _DbContext.WorkflowActor.Where(m => m.ProcessId == wfManager.ProcessId && m.ActorNumber == wfDownLink.ToActorNumber)
                .FirstOrDefaultAsync();
                if (wfDownlinkActor != null)
                {
                    sDownlinkRoleId = wfDownlinkActor.ActorName;
                }
            }

            //get users in the uplink and downlink of this user into the UserList dropdown
            var groupByUpLink = new SelectListGroup() { Name = "UpLinkActors" };
            var groupByDownLink = new SelectListGroup() { Name = "DownLinkActors" };

            wfManager.UserList = _DbContext.UserProfile
                .Where(m => m.RoleId.ToUpper() == sDownlinkRoleId.ToUpper())
                .Select(a => new SelectListItem
                {
                    Value = a.UserId,
                    Text = a.RoleId + " - " + a.Fullname,
                    Group = groupByDownLink
                }).ToList();
            wfManager.UserList.AddRange(_DbContext.UserProfile
                .Where(m => m.RoleId.ToUpper() == sUplinkRoleId.ToUpper())
                .Select(a => new SelectListItem
                {
                    Value = a.UserId,
                    Text = a.RoleId + " - " + a.Fullname,
                    Group = groupByUpLink
                }).ToList());
            */

            //NEW REQUIREMENT:
            //get all roles in this workflow from the workflow actors table
            string sRoles = string.Empty;
            var wfActorList = await _DbContext.WorkflowActor.Where(m => m.ProcessId == wfManager.ProcessId).ToListAsync();
            foreach (WorkflowActor _Actor in wfActorList)
            {
                sRoles += "'" + _Actor.ActorName + "',";
            }

            if (!string.IsNullOrEmpty(sRoles))
            {
                sRoles = sRoles[0..^1];
            }

            //get all users in the workflow into the UserList dropdown instead of the uplink and downlink of this user 
            wfManager.UserList = _DbContext.UserProfile.FromSqlRaw("SELECT * FROM UserProfile WHERE RoleId IN (" + sRoles + ")")
                .Select(a => new SelectListItem
                {
                    Value = a.UserId,
                    Text = a.RoleId + " - " + a.Fullname
                }).ToList();

            //adds secretary to the list if calling from approval/rejection (tariff)
            if (AppendSecFlag == "1")
            {
                var secUser = _DbContext.UserProfile.FirstOrDefault(f => f.RoleId.ToLower() == "secretary");
                wfManager.UserList.Add(new SelectListItem
                {
                    Value = secUser.UserId,
                    Text = secUser.RoleId + " - " + secUser.Fullname
                });
            }

            //wfManager.UserList = AppendSecretaryToList(wfManager.UserList, wfActorList, wfManager.ProcessId);

            //get workflow actions
            wfManager.ActionList = _DbContext.MetaDataRef.Where(m => m.MetaDataType.ToUpper() == Vars.MetaData.WORKFLOW_ACTION.ToString())
                .Select(a => new SelectListItem
                {
                    Value = a.ReferenceId,
                    Text = a.ReferenceId
                }).ToList();

            return PartialView("TaskDetails", wfManager);
        }

        private List<SelectListItem> AppendSecretaryToList(List<SelectListItem> userlist, List<WorkflowActor> actors, string processid)
        {
            if (!processid.ToLower().Contains("tariff"))
            {
                return userlist;
            }

            if (_Vars.RoleId.ToLower() != "secretary")
            {
                var actorIndex = actors.FirstOrDefault(f => f.ActorName == _Vars.RoleId).ActorName;
                //if (actorIndex == "HOD")
                //{
                    var secMail = _DbContext.SystemConfig.FirstOrDefault().Secretary;

                    if (string.IsNullOrEmpty(secMail))
                    {
                        return userlist;
                    }

                    var secUser = _DbContext.UserProfile.FirstOrDefault(f => f.Email == secMail);
                    if (secUser == null)
                    {
                        return userlist;
                    }

                    userlist.Add(new SelectListItem { Value = secUser.UserId, Text = $"{secUser.RoleId} - {secUser.Fullname}" });
                //}
            }
            else
            {
                var archiveAcctMail = _DbContext.SystemConfig.FirstOrDefault().TaskClosureAccount;
                if (string.IsNullOrEmpty(archiveAcctMail))
                {
                    return userlist;
                }
                var archiveAcct = _DbContext.UserProfile.FirstOrDefault(f => f.Email == archiveAcctMail);

                if (archiveAcct == null)
                {
                    return userlist;
                }

                userlist = new List<SelectListItem> { new SelectListItem { Value = archiveAcct.UserId, Text = $"{archiveAcct.RoleId} - {archiveAcct.Fullname}" } };
            }
            return userlist;
        }

        public async Task<IActionResult> CreateTask(string ProcessId, string OperatorId, string ActionId, string Remarks, 
            string ActionUrl, string UserId)
        {
            //user OperatorId to get OperatorName
            string sOperatorName = string.Empty;
            var orgProfile = _DbContext.UserProfile.Where(u => u.OrganizationId == OperatorId).FirstOrDefault();
            if (orgProfile != null) sOperatorName = orgProfile.OrganizationName;

            //get taskid for the new task
            WorkflowMgt _WorkflowMgt = new WorkflowMgt();
            string sTaskId = _WorkflowMgt.GetNewTaskId(ProcessId);

            //create first leg for the source user
            WorkflowManager wManagerSrc = new WorkflowManager
            {
                TaskId = sTaskId,
                TaskType = Vars.TaskTypes["NEW"].ToString(),
                ProcessId = ProcessId,
                ReferenceNo = OperatorId,
                OperatorName = sOperatorName,
                ActionId = ActionId,
                Remarks = Remarks,
                ActionUrl = ActionUrl,
                UserId = _Vars.UserId,
                RoleId = _Vars.RoleId,
                DateAssigned = DateTime.Now,
                DateCompleted = DateTime.Now,
                CompletionFlag = "YES",
                IsSource = true
            };
            _DbContext.WorkflowManager.Add(wManagerSrc);


            //create second leg for the destination user
            string sDestRoleId = string.Empty;
            var userProfile = _DbContext.UserProfile.Where(u => u.UserId == UserId).FirstOrDefault();
            if (userProfile != null) sDestRoleId = userProfile.RoleId;
            WorkflowManager wManagerDst = new WorkflowManager
            {
                TaskId = sTaskId,
                TaskType = Vars.TaskTypes["PENDING"].ToString(),
                ProcessId = ProcessId,
                ReferenceNo = OperatorId,
                OperatorName = sOperatorName,
                ActionId = null,
                Remarks = null,
                ActionUrl = ActionUrl,
                UserId = UserId,
                RoleId = sDestRoleId,
                DateAssigned = DateTime.Now,
                DateCompleted = null,
                CompletionFlag = "NO",
                IsSource = false
            };
            _DbContext.WorkflowManager.Add(wManagerDst);

            try
            {
                await _DbContext.SaveChangesAsync(true);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                ViewBag.TaskMessage = ex.Message;
                return PartialView("TaskCommitFail");
            }

            _Vars.PendingTaskCount = _Vars.CountPendingTask(_Vars.UserId);
            ViewBag.TaskMessage = "Task committed successfully";
            return PartialView("TaskCommitSuccess");
        }

        public async Task<IActionResult> TerminateTask(string ActionUrl, string ActionId, string remarks, string status, string DestUserId)
        {

            if (ActionUrl.Contains("ApproveTariff"))
                ActionUrl = ActionUrl.Replace("ApproveTariff", "ViewTariff");
            else
                ActionUrl = ActionUrl.Replace("SurveyDetails2", "SurveyDetails");

            if (ActionUrl.Contains("localhost"))
            {
                var uri = new Uri(ActionUrl);
                var host = uri.Host;
                var port = uri.Port;

                ActionUrl = ActionUrl.Replace($"{host}:{port}", "pcea.ncc.gov.ng");
            }

            var currTask = await _DbContext.WorkflowManager.FirstOrDefaultAsync(e => e.ActionUrl == ActionUrl && e.UserId == _Vars.UserId && e.CompletionFlag == "NO");
            if (currTask == null)
            {
                ViewBag.TaskMessage = "Task not found.";
                return PartialView("TaskCommitFail");
            }
            if (currTask.CompletionFlag.ToUpper() == "YES")
            {
                ViewBag.TaskMessage = "Task has been previously committed to workflow. Contact Administrator.";
                return PartialView("TaskCommitFail");
            }
            var roleId = _Vars.RoleId;


            //var taskArchive = _DbContext.SystemConfig.FirstOrDefault().TaskClosureAccount;
            //if (string.IsNullOrEmpty(taskArchive))
            //{
            //    ViewBag.TaskMessage = "A task closure account has not been setup yet. Please contact a System Administrator";
            //    return PartialView("TaskCommitFail");
            //}

            //DestUserId = _DbContext.UserProfile.FirstOrDefault(f => f.Email == taskArchive).UserId;
            //if (string.IsNullOrEmpty(DestUserId))
            //{
            //    ViewBag.TaskMessage = "A task closure account has not been setup yet. Please contact a System Administrator";
            //    return PartialView("TaskCommitFail");
            //}

            //var response = await _taskHelper.CommitTask(currTask.RecId.ToString(), ActionId, remarks, DestUserId, true);

            var isASucess = await _taskHelper.TerminateTask(currTask.ActionUrl);

            if (!isASucess)
            {
                ViewBag.TaskMessage = TaskHelper.ErrRespponse;
                return PartialView("TaskCommitFail");
            }

            _Vars.PendingTaskCount = _Vars.CountPendingTask(_Vars.UserId);
                //return response;
            
            
            ViewBag.TaskMessage = "Task committed successfully";
            return PartialView("TaskCommitSuccess");
        }

        //public async Task<IActionResult> TerminateTask(string ActionUrl, string ActionId, string remarks, string status, string DestUserId)
        //{

        //    if (ActionUrl.Contains("ApproveTariff"))
        //        ActionUrl = ActionUrl.Replace("ApproveTariff", "ViewTariff");
        //    else
        //        ActionUrl = ActionUrl.Replace("SurveyDetails2", "SurveyDetails");

        //    if (ActionUrl.Contains("localhost"))
        //    {
        //        var uri = new Uri(ActionUrl);
        //        var host = uri.Host;
        //        var port = uri.Port;

        //        ActionUrl = ActionUrl.Replace($"{host}:{port}", "pcea.ncc.gov.ng");
        //    }

        //    var currTask = await _DbContext.WorkflowManager.FirstOrDefaultAsync(e => e.ActionUrl == ActionUrl && e.UserId == _Vars.UserId && e.CompletionFlag == "NO");
        //    if (currTask == null)
        //    {
        //        ViewBag.TaskMessage = "Task not found.";
        //        return PartialView("TaskCommitFail");
        //    }
        //    if (currTask.CompletionFlag.ToUpper() == "YES")
        //    {
        //        ViewBag.TaskMessage = "Task has been previously committed to workflow. Contact Administrator.";
        //        return PartialView("TaskCommitFail");
        //    }
        //    var roleId = _Vars.RoleId;

        //    if(!ActionUrl.Contains("SurveyDetails") && (status.ToLower() == "approved" || status.ToLower() == "rejected"))
        //    {
        //        var taskArchive = _DbContext.SystemConfig.FirstOrDefault().TaskClosureAccount;
        //        if (string.IsNullOrEmpty(taskArchive))
        //        {
        //            ViewBag.TaskMessage = "A task closure account has not been setup yet. Please contact a System Administrator";
        //            return PartialView("TaskCommitFail");
        //        }

        //        DestUserId = _DbContext.UserProfile.FirstOrDefault(f => f.Email == taskArchive).UserId;
        //        if (string.IsNullOrEmpty(DestUserId))
        //        {
        //            ViewBag.TaskMessage = "A task closure account has not been setup yet. Please contact a System Administrator";
        //            return PartialView("TaskCommitFail");
        //        }

        //        var response = await _taskHelper.CommitTask(currTask.RecId.ToString(), ActionId, remarks, DestUserId, true);

        //        if (!response)
        //        {
        //            ViewBag.TaskMessage = TaskHelper.ErrRespponse;
        //            return PartialView("TaskCommitFail");
        //        }
        //        _Vars.PendingTaskCount = _Vars.CountPendingTask(_Vars.UserId);
        //        //return response;
        //    }
        //    else
        //    {
        //        var isASucess = await _taskHelper.TerminateTask(currTask.ActionUrl);
        //    }
        //    ViewBag.TaskMessage = "Task committed successfully";
        //    return PartialView("TaskCommitSuccess");
        //}

        //Added (STATUS) to cater for the task movement to the secretary for TARIFF unit

        [HttpPost]
        public async Task<JsonResult> SendToSec(string ActionUrl, string status, string remark)
        {
            try
            {
                var host = ActionUrl.Split("/")[2];
                var page = ActionUrl.Split("/")[4];

                ActionUrl = ActionUrl.Replace(host, "pcea.ncc.gov.ng");

                //if (page.ToLower().Contains("tariff"))
                //{
                ActionUrl = ActionUrl.Replace(page, _Config["Tasks:TariffView"]);
                //}                

                var workflow = _DbContext.WorkflowManager.FirstOrDefault(f => f.ActionUrl == ActionUrl && f.UserId == _Vars.UserId && f.CompletionFlag == "NO");

                if(workflow == null)
                {
                    return Json(new { response = "You do not have access to this task", success = false });
                }

                var user = _DbContext.UserProfile.FirstOrDefault(f => f.RoleId.ToLower() == "secretary");
                if (user == null)
                {
                    return Json(new { response = "No user has been setup to function as the Secretary!", success = false });
                }

                var response = await CommitTaskAsync(workflow.RecId.ToString(), status, remark, user.UserId, status);

                return Json(new { response = "Task was successfully moved to the Secretary!", success = true });
            }
            catch
            {
                return Json(new { response = "An error occured while performing operation!", success = false });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CommitTaskAsync(string RecId, string ActionId, string Remarks, string UserId, string Status)
        {
            var IsSuccess = await _taskHelper.CommitTask(RecId, ActionId, Remarks, UserId, false, Status);
            if (!IsSuccess)
            {
                var errorMsg = TaskHelper.ErrRespponse;

                ViewBag.TaskMessage = errorMsg;
                return PartialView("TaskCommitFail");
            }

            _Vars.PendingTaskCount = _Vars.CountPendingTask(_Vars.UserId);
            ViewBag.TaskMessage = "Task committed successfully";
            return PartialView("TaskCommitSuccess");
        }
        
        public async Task<IActionResult> CommitTask(string RecId, string ActionId, string Remarks, string UserId)
        {
            if(UserId.ToUpper()=="TERMINATE_PROCESS")
            {
                //end the process
                //get current task record and update for current user.
                var wManagerCurr = await _DbContext.WorkflowManager.Where(m => m.RecId == long.Parse(RecId)).OrderByDescending(o => o.DateAssigned).FirstOrDefaultAsync();
                if (wManagerCurr == null)
                {
                    ViewBag.TaskMessage = "Task not found.";
                    return PartialView("TaskCommitFail");
                }
                if (wManagerCurr.CompletionFlag.ToUpper() == "YES")
                {
                    ViewBag.TaskMessage = "Task has been previously committed to workflow.  Contact Administrator.";
                    return PartialView("TaskCommitFail");
                }
                var operInDb = _DbContext.UserProfile.Where(u => u.UserId == wManagerCurr.ReferenceNo).FirstOrDefault();
                wManagerCurr.ActionId = ActionId;
                wManagerCurr.Remarks = Remarks;
                wManagerCurr.DateCompleted = DateTime.Now;
                wManagerCurr.CompletionFlag = "YES";
                wManagerCurr.TaskType = Vars.TaskTypes["COMPLETED"].ToString();
                _DbContext.WorkflowManager.Update(wManagerCurr);
                await _DbContext.SaveChangesAsync(true);
            }
            else
            {
                var IsSuccess = await _taskHelper.CommitTask(RecId, ActionId, Remarks, UserId, false);
                if (!IsSuccess)
                {
                    var errorMsg = TaskHelper.ErrRespponse;

                    ViewBag.TaskMessage = errorMsg;
                    return PartialView("TaskCommitFail");
                }
            }

            _Vars.PendingTaskCount = _Vars.CountPendingTask(_Vars.UserId);
            ViewBag.TaskMessage = "Task committed successfully";
            return PartialView("TaskCommitSuccess");
        }
        
        
        //public async Task<IActionResult> CommitTask(string RecId, string ActionId, string Remarks, string UserId)
        //{
        //    //get current task record and update for current user.
        //    var wManagerCurr = await _DbContext.WorkflowManager.Where(m => m.RecId == long.Parse(RecId)).FirstOrDefaultAsync();
        //    if (wManagerCurr == null)
        //    {
        //        ViewBag.TaskMessage = "Task not found.";
        //        return PartialView("TaskCommitFail");
        //    }
        //    if (wManagerCurr.CompletionFlag.ToUpper() == "YES")
        //    {
        //        ViewBag.TaskMessage = "Task has been previously committed to workflow.  Contact Administrator.";
        //        return PartialView("TaskCommitFail");
        //    }
        //    wManagerCurr.ActionId = ActionId;
        //    wManagerCurr.Remarks = Remarks;
        //    wManagerCurr.DateCompleted = DateTime.Now;
        //    wManagerCurr.CompletionFlag = "YES";
        //    wManagerCurr.TaskType = Vars.TaskTypes["COMPLETED"].ToString();
        //    _DbContext.WorkflowManager.Update(wManagerCurr);


        //    //create second leg for the destination user
        //    string sDestRoleId = string.Empty;
        //    var userProfile = _DbContext.UserProfile.Where(u => u.UserId == UserId).FirstOrDefault();
        //    if (userProfile == null)
        //    {
        //        ViewBag.TaskMessage = "Destination user profile not found.";
        //        return PartialView("TaskCommitFail");
        //    }
        //    sDestRoleId = userProfile.RoleId;
        //    WorkflowManager wManagerDst = new WorkflowManager
        //    {
        //        TaskId = wManagerCurr.TaskId,
        //        TaskType = Vars.TaskTypes["PENDING"].ToString(),
        //        ProcessId = wManagerCurr.ProcessId,
        //        ReferenceNo = wManagerCurr.ReferenceNo,
        //        OperatorName = wManagerCurr.OperatorName,
        //        ActionId = null,
        //        Remarks = null,
        //        ActionUrl = wManagerCurr.ActionUrl,
        //        UserId = UserId,
        //        RoleId = sDestRoleId,
        //        DateAssigned = DateTime.Now,
        //        DateCompleted = null,
        //        CompletionFlag = "NO"
        //    };
        //    _DbContext.WorkflowManager.Add(wManagerDst);

        //    try
        //    {
        //        await _DbContext.SaveChangesAsync(true);
        //    }
        //    catch (DbUpdateConcurrencyException ex)
        //    {
        //        ViewBag.TaskMessage = ex.Message;
        //        return PartialView("TaskCommitFail");
        //    }

        //    _Vars.PendingTaskCount = _Vars.CountPendingTask(_Vars.UserId);
        //    ViewBag.TaskMessage = "Task committed successfully";
        //    return PartialView("TaskCommitSuccess");
        //}

        private bool SendTaskNotification()
        {
            return true;
        }

        public IActionResult ProcessAudit()
        {
            var processes = _DbContext.WorkflowManager.ToList();
            return View(processes);
        }
        //!--------------------NEW WORKFLOW MANAGER MODULES-----------------------------



    }
}

