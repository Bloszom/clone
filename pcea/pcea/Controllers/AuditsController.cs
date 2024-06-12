using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using pcea.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using pcea.Helpers;
using pcea.Models;
using pceaLibrary;
using Microsoft.EntityFrameworkCore;

namespace pcea.Controllers
{
    public class AuditsController : Controller
    {

        private readonly PceaDbContext _DbContext;
        private readonly IHttpContextAccessor _HttpContext;
        private readonly IWebHostEnvironment _webHostEnv;
        private readonly IHostingEnvironment hosting;
        private IConfiguration _Configuration;
        Vars _Vars;
        NotificationMgt _notificationMgt;
        AuditTrail _AuditTrail;

        public AuditsController(PceaDbContext context, IHttpContextAccessor httpContext, IConfiguration configuration, IWebHostEnvironment webHostEnv, IHostingEnvironment hosting)
        {
            _DbContext = context;
            _HttpContext = httpContext;
            _webHostEnv = webHostEnv;
            this.hosting = hosting;
            _Configuration = configuration;
            _Vars = new Vars(_HttpContext.HttpContext);
            _notificationMgt = new NotificationMgt(hosting);
            _AuditTrail = new AuditTrail(webHostEnv);
        }
        public IActionResult AuditTrail()
        {
            string pattern = @".*\.json";
            var sPath = Path.Combine(_webHostEnv.WebRootPath, "log");
            var matches = Directory.GetFiles(sPath).Where(path => Regex.IsMatch(Path.GetFileName(path), pattern));

            var nameList = new List<KeyPair>();

            foreach (var itm in matches)
            {
                var audit = new KeyPair();

                var name = Path.GetFileName(itm);
                name.Replace(".json", "");
                audit.Name = name;
                audit.Value = name;

                nameList.Add(audit);
            }

            var names = nameList.Select(x => new SelectListItem { Text = x.Name, Value = x.Value }).ToList();

            var model = new AuditTrailObj
            {
                NameList = names
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ReOpenTask(string Taskid)
        {
            try
            {
                var lastFlowData = await _DbContext.WorkflowManager.FirstOrDefaultAsync(f => f.RecId == long.Parse(Taskid));

                //if(workflowManager.Any(a => a.CompletionFlag.ToLower() == "no"))
                //{
                //    //return invalid operation, task has not ended
                //}

                //var lastFlowData = workflowManager.OrderByDescending(o => o.DateCompleted).FirstOrDefault();
                
                var entryId = lastFlowData.ActionUrl.Substring(lastFlowData.ActionUrl.LastIndexOf('/') + 1);
                var sql = $"UPDATE dbo.workflowmanager SET completionflag = 'NO', tasktype = 'PENDING', datecompleted = null, dateassigned = '{DateTime.Now}' WHERE recid = '{lastFlowData.RecId}'; UPDATE dbo.formsreview SET status = 'PENDING', fileurl = null WHERE entryid = '{entryId}'";
                var check = await _DbContext.Database.ExecuteSqlRawAsync(sql);

                var checcounttask = _Vars.CountPendingTask(lastFlowData.UserId);

                ViewBag.msg = "Task reactivation was successful!";

            }
            catch (Exception ex)
            {
                ViewBag.err = "An error occurred while carrying out reactivation process";
            }

            return RedirectToAction(nameof(ProcessAudit));
        }

        [HttpPost]
        public async Task<IActionResult> AuditTrail(AuditRequest request)
        {
            string pattern = @".*\.json";
            var sPath = Path.Combine(_webHostEnv.WebRootPath, "log");
            var matches = Directory.GetFiles(sPath).Where(path => Regex.IsMatch(Path.GetFileName(path), pattern));

            var nameList = new List<KeyPair>();

            foreach (var itm in matches)
            {
                var audit = new KeyPair();

                var name = Path.GetFileName(itm);
                name.Replace(".json", "");
                audit.Name = name;
                audit.Value = name;

                nameList.Add(audit);
            }

            var names = nameList.Select(x => new SelectListItem { Text = x.Name, Value = x.Value }).ToList();

            var model = new AuditTrailObj
            {
                NameList = names,
                LogItems = _AuditTrail.ReadLogFile(request.Name)
            };

            return View(model);
        }

        public IActionResult ProcessAudit()
        {
            var processes = _DbContext.WorkflowManager.ToList();
            var pros = processes.GroupBy(g => g.TaskId).ToList();
            List<WorkflowManager> grpedList = new List<WorkflowManager>();
            foreach(var item in pros)
            {
                //var itm = item.OrderBy(o => o.DateAssigned).ToList();
                //var flag = itm.Last().CompletionFlag;

                if(!item.Any(a => a.CompletionFlag == "NO"))
                {
                    item.Where(f => !f.IsSource).ToList();

                    foreach (var task in item)
                    {
                        if(!task.IsSource)
                        {
                            task.Reopen = true;
                        }

                        grpedList.Add(task);
                    }
                }
                else
                {
                    grpedList.AddRange(item.ToList());
                }
            }

            processes = grpedList;
            //var now = pros.FirstOrDefault().FirstOrDefault().Reopen = true;
            return View(processes);
        }
    }
}