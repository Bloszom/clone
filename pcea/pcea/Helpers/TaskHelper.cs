using _FrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pcea.Models;
using pceaLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace pcea.Helpers
{
    public class TaskHelper
    {
        private PceaDbContext _DbContext { get; set; }
        public static string ErrRespponse { get; set; }
        private readonly IHostingEnvironment hosting;
        Vars _vars;
        private SystemConfig sysConfig { get; set; }
        public TaskHelper(PceaDbContext dbContext, IHttpContextAccessor httpContext, IHostingEnvironment environment)
        {
            _DbContext = dbContext;
            hosting = environment;
            sysConfig = _DbContext.SystemConfig.FirstOrDefault();
            _vars = new Vars(httpContext.HttpContext);
        }

        public async Task<bool> SendMailNotification(UserProfile user, string sOrgName, string sOperatorId, string sNotificationType, string Submission)
        {
            string sEmail = user.Email;
            string sFullname = user.Fullname;

            //get mail message html
            string sFilename = "templates/" + sNotificationType + ".html";
            string sBody = string.Empty;

            // var fs = new FileStream(sFilename, FileMode.Open, FileAccess.Read);

            using (StreamReader reader = new StreamReader(Path.Combine(hosting.WebRootPath, sFilename)))
            {
                sBody = reader.ReadToEnd();
            }
            sBody = sBody.Replace("{FULLNAME}", sFullname);
            sBody = sBody.Replace("{OPERATORNAME}", sOrgName);
            sBody = sBody.Replace("{DATE}", DateTime.Now.ToString());
            sBody = sBody.Replace("{SUBMISSION}", Submission);

            //send mail
            MailHelperV2 _mail = new MailHelperV2();
            _mail.MailTo = sEmail;
            _mail.MailSubject = sNotificationType.Replace("_", " ");
            _mail.MailBody = sBody;
            _mail.ReferenceNumber = sOperatorId;
            _mail.MailType = sNotificationType;
            _mail.SaveCopyToDatabase = true;


            //if (_mail.SendMail() == false)
            if (await _mail.SendEmailAsync(new MailHelperV2.MailRequest {Attachments = null, Subject = _mail.MailSubject}) == false)
            {
                ErrRespponse = "Unable to send mail.  Contact System Administrator";
                return false;
            }

            return true;
        }

        public async Task<bool> SendMailNotification(string sEmails, string sFullname, string sOrgName, string sOperatorId, string sNotificationType, string Submission)
        {

            //get mail message html
            string sFilename = "templates/" + sNotificationType + ".html";
            string sBody = string.Empty;

            // var fs = new FileStream(sFilename, FileMode.Open, FileAccess.Read);

            using (StreamReader reader = new StreamReader(Path.Combine(hosting.WebRootPath, sFilename)))
            {
                sBody = reader.ReadToEnd();
            }
            sBody = sBody.Replace("{FULLNAME}", sFullname);
            sBody = sBody.Replace("{OPERATORNAME}", sOrgName);
            sBody = sBody.Replace("{DATE}", DateTime.Now.ToString());
            sBody = sBody.Replace("{SUBMISSION}", Submission);

            //send mail
            MailHelperV2 _mail = new MailHelperV2();
            _mail.MailTo = sEmails;
            _mail.MailSubject = sNotificationType.Replace("_", " ");
            _mail.MailBody = sBody;
            _mail.ReferenceNumber = sOperatorId;
            _mail.MailType = sNotificationType;
            _mail.SaveCopyToDatabase = true;


            //if (_mail.SendMail() == false)
            if (await _mail.SendEmailAsync(new MailHelperV2.MailRequest { Attachments = null, Subject = _mail.MailSubject }) == false)
            {
                ErrRespponse = "Unable to send mail.  Contact System Administrator";
                return false;
            }

            return true;
        }

        public async Task<bool> TerminateTask(string ActionUrl)
        {
            //fetch the task details
            var Task = await _DbContext.WorkflowManager.FirstOrDefaultAsync(e => e.ActionUrl == ActionUrl && e.UserId == _vars.UserId);

            //update the task status
            Task.CompletionFlag = "YES";
            Task.TaskType = Vars.TaskTypes.FirstOrDefault(sl => sl.Key.ToLower() == "completed").Value;
            Task.DateCompleted = DateTime.Now;

            _vars.PendingTaskCount = (int.Parse(_vars.PendingTaskCount) - 1).ToString();

            _DbContext.Update(Task);
            await _DbContext.SaveChangesAsync(true);
            //return appropriate response
            return true;
        }

        public async Task<bool> InitiateTask(int FormId, long entryId)
        {
            var ActionUrl = string.Empty;

            //Fetch processid from forms table
            var form = _DbContext.Forms.FirstOrDefault(f => f.RecId == FormId);
            if (form == null) return false;

            //search the workflow manger for the first Actor in the workflow chain
            var Actor = _DbContext.WorkflowActor.FirstOrDefault(w => w.ProcessId == form.ProcessId).ActorName;
            //var Actor = _DbContext.WorkflowActor.FirstOrDefault(w => w.ProcessId == processId && w.ActorNumber < 1).ActorName;
            if (Actor == null) return false;

            var user = _DbContext.UserProfile.FirstOrDefault(u => u.RoleId == Actor);
            if (user == null) return false;

            if (form.FormsType.ToLower().Contains("tariff"))
            {
                ActionUrl += sysConfig.TariffReviewBase + "/" + entryId.ToString();
            }
            
            if(form.FormsType.ToLower().Contains("survey") || form.FormsType.ToLower().Contains("operator"))
            {
                ActionUrl += sysConfig.SurveyReviewBase + "/" + entryId.ToString();
            }

            if (form.FormsType.ToLower().Contains("other"))
            {
                ActionUrl += sysConfig.OthersReviewBase + "/" + entryId.ToString();
            }

            var isSuccess = await CreateTask(form.ProcessId, _vars.OperatorId, Vars.DefaultTaskAction.REVIEW.ToString(), "", ActionUrl, user.UserId);

            if(!isSuccess)
                return false;
            var operInDb = _DbContext.UserProfile.FirstOrDefault(f => f.OrganizationId == _vars.OperatorId);

            // Send email notification to the assigned user
            await SendMailNotification(user, operInDb.OrganizationName, operInDb.OrganizationId, Vars.MailType["NEW_TASK_NOTIFICATION"], form.FormName);


            //-------------------Sending of emails to a group 11/6/2024-------------------
            // Fetch all users with the same role as the Actor
            var deskOfficers = _DbContext.UserProfile.Where(u => u.RoleId == Actor).ToList();
            // Concatenate email addresses
            var emailAddresses = string.Join(";", deskOfficers.Select(o => o.Email));
            await SendMailNotification(emailAddresses, user.Fullname, operInDb.OrganizationName, operInDb.OrganizationId, Vars.MailType["NEW_TASK_BROADCAST"], form.FormName);
         
            // await SendMailNotification(user, operInDb.OrganizationName, operInDb.OrganizationId, Vars.MailType["NEW_TASK_NOTIFICATION"], processId.FormName);
            return isSuccess;

        }

        public async Task<bool> CommitTask(string RecId, string ActionId, string Remarks, string UserId, bool isApproval = false, string? Status = "PENDING")
        {
            try
            {
                //get current task record and update for current user.
                var wManagerCurr = await _DbContext.WorkflowManager.Where(m => m.RecId == long.Parse(RecId)).OrderByDescending(o => o.DateAssigned).FirstOrDefaultAsync();
                if (wManagerCurr == null)
                {
                    ErrRespponse = "Task not found.";
                    return false;
                }
                if (wManagerCurr.CompletionFlag.ToUpper() == "YES")
                {
                    ErrRespponse = "Task has been previously committed to workflow.  Contact Administrator.";

                    return false;
                }

                //if (isApproval)
                //{
                //    var sysConfig = _DbContext.SystemConfig.FirstOrDefault();
                //    var id = wManagerCurr.ActionUrl.Substring(wManagerCurr.ActionUrl.LastIndexOf("/") + 1);
                //    wManagerCurr.ActionUrl = $"{sysConfig.TariffLetterUploadBase}/{id}";
                //}

                //var operInDb = _DbContext.UserProfile.Where(u => u.UserId == wManagerCurr.ReferenceNo).FirstOrDefault();

                wManagerCurr.ActionId = ActionId;
                wManagerCurr.Remarks = Remarks;
                wManagerCurr.DateCompleted = DateTime.Now;
                wManagerCurr.CompletionFlag = "YES";
                wManagerCurr.TaskType = Vars.TaskTypes["COMPLETED"].ToString();
                _DbContext.WorkflowManager.Update(wManagerCurr);


                //create second leg for the destination user
                string sDestRoleId = string.Empty;
                var userProfile = _DbContext.UserProfile.Where(u => u.UserId == UserId).FirstOrDefault();
                if (userProfile == null)
                {
                    ErrRespponse = "Destination user profile not found.";

                    return false;
                }

                sDestRoleId = userProfile.RoleId;
                if(Status != null)
                {
                    if (Status.ToLower() == "sendtosec")
                    {
                        var arr = wManagerCurr.ActionUrl.Split("/");
                        var subType = arr[arr.Length - 2];
                        Status = Status.Split("-")[1] == "A" ? "APPROVED" : "REJECTED";
                        wManagerCurr.ActionUrl = wManagerCurr.ActionUrl.Replace(subType, AppHelper.GetCurrentSettings("Tasks:SecretaryBase"));
                    }
                }


                WorkflowManager wManagerDst = new WorkflowManager
                {
                    TaskId = wManagerCurr.TaskId,
                    TaskType = Vars.TaskTypes["PENDING"].ToString(),
                    ProcessId = wManagerCurr.ProcessId,
                    ReferenceNo = wManagerCurr.ReferenceNo,
                    OperatorName = wManagerCurr.OperatorName,
                    ActionId = null,
                    Remarks = Status,
                    ActionUrl = wManagerCurr.ActionUrl,
                    UserId = UserId,
                    RoleId = sDestRoleId,
                    DateAssigned = DateTime.Now,
                    DateCompleted = null,
                    CompletionFlag = "NO"
                };
                _DbContext.WorkflowManager.Add(wManagerDst);

                try
                {
                    await _DbContext.SaveChangesAsync(true);

                    // await SendMailNotification(userProfile, operInDb.OrganizationName, operInDb.OrganizationId, Vars.MailType["NEW_TASK_NOTIFICATION"], wManagerCurr.ProcessId);

                }
                catch (DbUpdateConcurrencyException ex)
                {
                    ErrRespponse = ex.Message;
                    return false;
                }

                return true;
            }
            catch
            {
                throw;
            }
 


        }
        
        public async Task<bool> CommitTask(string RecId, string ActionId, string Remarks, string UserId, bool isApproval = false)
        {
            //get current task record and update for current user.
            var wManagerCurr = await _DbContext.WorkflowManager.Where(m => m.RecId == long.Parse(RecId)).OrderByDescending(o => o.DateAssigned).FirstOrDefaultAsync();
            if (wManagerCurr == null)
            {
                ErrRespponse = "Task not found.";
                return false;
            }
            if (wManagerCurr.CompletionFlag.ToUpper() == "YES")
            {
                ErrRespponse = "Task has been previously committed to workflow.  Contact Administrator.";

                return false;
            }

            //if (isApproval)
            //{
            //    var sysConfig = _DbContext.SystemConfig.FirstOrDefault();
            //    var id = wManagerCurr.ActionUrl.Substring(wManagerCurr.ActionUrl.LastIndexOf("/") + 1);
            //    wManagerCurr.ActionUrl = $"{sysConfig.TariffLetterUploadBase}/{id}";
            //}

            var operInDb = _DbContext.UserProfile.Where(u => u.UserId == wManagerCurr.ReferenceNo).FirstOrDefault();

            wManagerCurr.ActionId = ActionId;
            wManagerCurr.Remarks = Remarks;
            wManagerCurr.DateCompleted = DateTime.Now;
            wManagerCurr.CompletionFlag = "YES";
            wManagerCurr.TaskType = Vars.TaskTypes["COMPLETED"].ToString();
            _DbContext.WorkflowManager.Update(wManagerCurr);


            //create second leg for the destination user
            string sDestRoleId = string.Empty;
            var userProfile = _DbContext.UserProfile.Where(u => u.UserId == UserId).FirstOrDefault();
            if (userProfile == null)
            {
                ErrRespponse = "Destination user profile not found.";

                return false;
            }

            sDestRoleId = userProfile.RoleId;
            WorkflowManager wManagerDst = new WorkflowManager
            {
                TaskId = wManagerCurr.TaskId,
                TaskType = Vars.TaskTypes["PENDING"].ToString(),
                ProcessId = wManagerCurr.ProcessId,
                ReferenceNo = wManagerCurr.ReferenceNo,
                OperatorName = wManagerCurr.OperatorName,
                ActionId = null,
                Remarks = null,
                ActionUrl = wManagerCurr.ActionUrl,
                UserId = UserId,
                RoleId = sDestRoleId,
                DateAssigned = DateTime.Now,
                DateCompleted = null,
                CompletionFlag = "NO"
            };
            _DbContext.WorkflowManager.Add(wManagerDst);

            try
            {
                await _DbContext.SaveChangesAsync(true);

                // await SendMailNotification(userProfile, operInDb.OrganizationName, operInDb.OrganizationId, Vars.MailType["NEW_TASK_NOTIFICATION"], wManagerCurr.ProcessId);

            }
            catch (DbUpdateConcurrencyException ex)
            {
                ErrRespponse = ex.Message;
                return false;
            }

            return true;
        }



        private async Task<bool> CreateTask(string ProcessId, string OperatorId, string ActionId, string Remarks,
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
                IsSource = true,
                ActionUrl = ActionUrl,
                UserId = _vars.UserId,
                RoleId = _vars.RoleId,
                DateAssigned = DateTime.Now,
                DateCompleted = DateTime.Now,
                CompletionFlag = "YES"
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
                CompletionFlag = "NO"
            };
            _DbContext.WorkflowManager.Add(wManagerDst);

            try
            {
                await _DbContext.SaveChangesAsync(true);

                var staffemail = userProfile.Email;
                //var mailHelper = MailHelper();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return false;
            }

            _vars.PendingTaskCount = _vars.CountPendingTask(_vars.UserId);

            return true;
        }

        public bool ReAssignTasks(string[] taskIds, string userId, string remarks = "")
        {
            var wrkFlows = _DbContext.WorkflowManager.Where(e => taskIds.ToList().Contains(e.TaskId) && e.UserId == _vars.UserId && e.IsSource == false).ToList();
            var user = _DbContext.UserProfile.FirstOrDefault(e => e.UserId == userId);
            try
            {
                foreach (WorkflowManager wrkflow in wrkFlows)
                {

                    wrkflow.IsSource = true;
                    wrkflow.CompletionFlag = "YES";
                    _DbContext.WorkflowManager.Update(wrkflow);


                    //get taskid for the new task
                    WorkflowMgt _WorkflowMgt = new WorkflowMgt();
                    string sTaskId = _WorkflowMgt.GetNewTaskId(wrkflow.ProcessId);

                    WorkflowManager wrkmgt = new WorkflowManager()
                    {
                        TaskId = sTaskId,
                        TaskType = Vars.TaskTypes["PENDING"].ToString(),
                        ProcessId = wrkflow.ProcessId,
                        ReferenceNo = wrkflow.ReferenceNo,
                        OperatorName = wrkflow.OperatorName,
                        ActionId = null,
                        Remarks = remarks,
                        ActionUrl = wrkflow.ActionUrl,
                        UserId = userId,
                        RoleId = user.RoleId,
                        DateAssigned = DateTime.Now,
                        DateCompleted = null,
                        CompletionFlag = "NO"
                    }; 
                    //wrkmgt. = wrkflow;
                    //wrkmgt.UserId = userId;
                    //wrkmgt.Remarks = remarks;
                    //wrkmgt.RoleId = user.RoleId;
                    //wrkmgt.IsSource = false;
                    //wrkmgt.TaskId = sTaskId;

                    _DbContext.WorkflowManager.Add(wrkmgt);
                }

                _DbContext.SaveChanges(true);

                _vars.PendingTaskCount = _vars.CountPendingTask(_vars.UserId);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }
}
