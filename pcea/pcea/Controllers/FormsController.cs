using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using pceaLibrary;
using pcea.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Path = System.IO.Path;
using _FrameworkCore;
using pcea.Helpers;

namespace pcea.Controllers
{
    public class FormsController : Controller
    {
        //private readonly ILogger<FormsController> _logger;
        //public FormsController(ILogger<FormsController> logger)
        //{
        //    _logger = logger;
        //}
        private readonly PceaDbContext _DbContext;
        private readonly IHttpContextAccessor _HttpContext;
        private readonly IWebHostEnvironment _webHostEnv;
        private readonly IHostingEnvironment hosting;
        private IConfiguration _Configuration;
        Vars _Vars;
        NotificationMgt _notificationMgt;
        AuditTrail _AuditTrail;
        TaskHelper _taskHelper;
        FormMsgHelper _formMsgHelper;
        WorkflowsController _workflowsController;

        public FormsController(PceaDbContext context, IHttpContextAccessor httpContext, IConfiguration configuration, IWebHostEnvironment webHostEnv, IHostingEnvironment hosting)
        {
            _DbContext = context;
            _HttpContext = httpContext;
            _webHostEnv = webHostEnv;
            this.hosting = hosting;
            _taskHelper = new TaskHelper(_DbContext, httpContext, hosting);
            _Configuration = configuration;
            _Vars = new Vars(_HttpContext.HttpContext);
            _notificationMgt = new NotificationMgt(hosting);
            _AuditTrail = new AuditTrail(webHostEnv);
            _formMsgHelper = new FormMsgHelper(configuration);
            _workflowsController = new WorkflowsController(_DbContext, httpContext, hosting, configuration);
        }
        enum FormsType
        {
            SURVEY, TARIFF, OTHER_SERVICES
        }
        SelectList LoadForm(FormsType fType)
        {
            string _ftype = fType == FormsType.SURVEY ? "OPERATOR_TYPE".ToLower() : (fType == FormsType.TARIFF ? "TARIFF_TYPE".ToLower() : "other_service");

            var select = new SelectList(
                    (from m in _DbContext.Forms select m).Where(m => m.FormsType.ToLower() == _ftype),
                    "RecId", "FormName");
            foreach(var item in select)
            {
                item.Text = item.Text.Replace("NEW ", "").Replace("New ", "");
            }
            return select;
        }

        public IActionResult Index()
        {
            var objForms = from m in _DbContext.FormsSubmission where m.FormsType.ToLower() != "archive" orderby m.DateCreated descending select m;
            //var objForms = from m in _DbContext.FormsSubmission select m;
            return View(objForms);
        }
        //SelectList OperatorTypes()
        //{
        //    return new SelectList(
        //            (from m in _DbContext.MetaDataRef select m).Where(m => m.MetaDataType == "OPERATOR_TYPE")
        //            .OrderBy(m => m.ReferenceId), "ReferenceId", "ReferenceId");
        //}
        //SelectList TariffTypes()
        //{
        //    return new SelectList(
        //            (from m in _DbContext.MetaDataRef select m).Where(m => m.MetaDataType == "TARIFF_TYPE")
        //            .OrderBy(m => m.ReferenceId), "ReferenceId", "ReferenceId");
        //}
        SelectList GetFormTypes(FormsType fType)
        {
            string _fType = fType == FormsType.SURVEY ? "OPERATOR_TYPE" : (fType == FormsType.TARIFF ? "TARIFF_TYPE" : "OTHER_SERVICE");
            return new SelectList(
                    (from m in _DbContext.MetaDataRef select m).Where(m => m.MetaDataType == _fType)
                    .OrderBy(m => m.ReferenceId), "ReferenceId", "ReferenceId");
        }  

        public IActionResult Details(long? id)
        {
            try
            {
                //list of forms
                ViewBag.SurveyList = LoadForm(FormsType.SURVEY);
                ViewBag.OperType = GetFormTypes(FormsType.SURVEY);

                if (id != null)
                {
                    //current selected form
                    ViewBag.entrys = _DbContext.FormsAndEntry.Where(c => c.RecId == id).OrderByDescending(s => s.DateSubmitted);
                }
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.error = ex.Message;
                return View();
            }
        }

        [HttpPost]
        public JsonResult Details(IFormCollection frm)
        {
            try
            {
                if (frm != null)
                {
                    if (frm.Keys.Count() == 1 && long.Parse(frm["id"]) > 0)
                    {
                        var objFields = (from m in _DbContext.Forms where m.RecId == long.Parse(frm["id"]) select m).FirstOrDefault().FormFields;

                        //var obj = objFields;
                        return Json(new List<object> { new { formfields = objFields } }[0]);
                    }
                    else if (long.Parse(frm["entry"]) > 0)
                    {
                        var objEntry = (from m in _DbContext.FormsEntry
                                        select m).Where(m => m.EntryId == long.Parse(frm["entry"])).ToList();
                        var _objrecid = objEntry.FirstOrDefault().FormId;

                        var objFields = (from m in _DbContext.Forms where m.RecId == long.Parse(frm["id"]) select m).FirstOrDefault().FormFields;
                        var objPrev = _DbContext.FormsDetails.FirstOrDefault(m => m.EntryId == long.Parse(frm["entry"]));
                        string _temp = objPrev == null ? "" : objPrev.FormDetails;

                        return Json(new List<object> { new { data = objEntry, template = _temp, formfields = objFields } }[0]);
                    }
                    else
                    {
                        return Json("");
                    }
                }
                else
                {

                    return Json("");
                }
            }
            catch (Exception ex)
            {
                ViewBag.error = ex.Message;
                return Json(ex.Message);
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetTerminalDate(long id)
        {
            try
            {
                var form = await _DbContext.Forms.FirstOrDefaultAsync(f => f.RecId == id);

                if (form == null)
                {
                    return Json(new { success = false });
                }

                return Json(new { success = true, date = form.TerminalDate.Date });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExtendTerminalDate(IFormCollection form)
        {
            try
            {
                var newdate = form["NewTerminalDate"];
                var id = form["RecId"];

                var formindb = await _DbContext.Forms.AsNoTracking().FirstOrDefaultAsync(f => f.RecId == long.Parse(id));
                formindb.TerminalDate = DateTime.Parse(newdate).Date;
                _DbContext.Forms.Update(formindb).Property(p => p.RecId).IsModified = false;
                await _DbContext.SaveChangesAsync(true);

                TempData["message"] = $"Terminal date for {formindb.FormName} has been extended till {formindb.TerminalDate}";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["error"] = "There was an error while updating terminal date";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddTariffHistory(TariffHistory history)
        {
            try
            {
                var entrId = DateTime.Now.ToFileTime();

                var frm = _DbContext.Forms.FirstOrDefault(f => f.FormsType.ToUpper() == "TARIFF_TYPE" && f.RecId == history.PlanType);

                // var user = await _DbContext.UserProfile.FirstOrDefaultAsync(f => f.OrganizationId == history.OperatorName);

                var orgid = history.OperatorName.Split(" ");

                var processor = await _DbContext.UserProfile.FirstOrDefaultAsync(f => f.UserId == history.Processor);
                //var date = DateTime.Now;
                //var DateofConveyance = date;
                //var daySpan = new TimeSpan(36, 0, 0, 0);
                //var yearSpan = new TimeSpan(730, 0, 0, 0);
                //history.LaunchDate = date.Add(daySpan);
                //history.ExpiryDate = date.Add(yearSpan);

                // if (user == null)
                // {
                //     user = new UserProfile
                //     {
                //         UserId = "N/A",
                //         OrganizationId = history.OperatorName,
                //         OrganizationName = history.OperatorName
                //     };
                // }

                //if (string.IsNullOrEmpty(frmId))
                //{
                //    frmId = "N/A";
                //}

                var frmDetailsSql = $"INSERT INTO [dbo].[FormsDetails] ([OperatorId],[FormId],[FormDetails],[DateCreated],[EntryId],[Status],[AppType],[ProductName],[ProductConcept],[LicenseCategory]) VALUES ('{orgid[0]}','{frm.RecId}','N/A','{DateTime.Now}',{entrId},'SUBMITTED','{frm.FormType}','{history.PlanName}','{history.Description}','{history.Category}')";

                var formsReview = $"INSERT INTO [dbo].[FormsReview] ([EntryId],[Status],[Remarks],[EvaluationRemarks],[DateofConveyance],[LaunchDate],[ExpiryDate],[Processor],[ShortCode]) VALUES ({entrId},'{history.Decision}','{history.Decision}','{history.Decision}','{history.ConveyanceDate}','{history.ExpectedLaunchDate}','{history.ExpectedExpiryDate}','{processor.Fullname}', '{history.ShortCode}')";

                var formsEntry = $"INSERT INTO [dbo].[FormsEntry] ([FormId],[FieldName],[FieldLabel],[Response],[DateSubmitted],[OperatorId],[UserId],[EntryId],[Status],[IsFile]) VALUES({frm.RecId},'','','','{history.SubmissionDate}','{orgid[0]}','','{entrId}','SUBMITTED',0)";

                var finalSql = $"{frmDetailsSql}; {formsReview}; {formsEntry}";
                var check = _DbContext.Database.ExecuteSqlRaw(finalSql);

                if (check <= 0)
                {
                    //return an error message back to the view
                    ViewBag.error = "Tariff history operation unsuccessful";
                    return RedirectToAction("TariffReport", "Report");
                }

                //else return a success message
                TempData["message"] = "Tariff history was saved successfully";

                return RedirectToAction("TariffReport", "Report");
            }
            catch
            {
                ViewBag.err = "Tariff history operation unsuccessful";

                return RedirectToAction("TariffReport", "Report");
            }
        }

        public IActionResult ViewTariff(long id, int isWorkflow)
        {
            var details = _DbContext.FormsAndEntry.FirstOrDefault(c => c.EntryId == id);
            var getUserId = _DbContext.FormsEntry.FirstOrDefault(c => c.EntryId == id);

            ViewBag.OperatorId = details.OrganizationName;
            ViewData["OppUserId"] = getUserId.UserId;
            ViewData["OppId"] = details.OperatorId;
            ViewData["EntryId"] = id;
            ViewBag.title = details.ProductName;
            ViewBag.remarksUrl = details.ReviewFileUrl;
            ViewBag.remarks = details.ReviewRemarks;
            ViewBag.evaluationUrl = details.EvaluationFileUrl;
            ViewBag.evaluation = details.EvaluationRemarks;

            if (isWorkflow > 0)
                ViewBag.link = "/Forms/TariffRequest";
            else
                ViewBag.link = "/Workflows/TaskList";

            try
            {
                if (details.MasterEntryId > 0)
                {
                    var masterentry = _DbContext.FormsAndEntry.FirstOrDefault(c => c.EntryId == details.MasterEntryId);
                    ViewBag.MasterEntryId = masterentry.EntryId;
                    ViewBag.MasterEntryName = masterentry.ProductName;
                }
            }
            catch (Exception)
            {
            }

            return View();
        }

        public IActionResult ViewOthers(long id)
        {
            try
            {
                var details = _DbContext.FormsAndEntry.FirstOrDefault(c => c.EntryId == id);

                ViewBag.OperatorId = details.OrganizationName;
                ViewBag.title = details.ProductName;
                ViewBag.remarksUrl = details.ReviewFileUrl;
                ViewBag.remarks = details.ReviewRemarks;
                ViewBag.evaluationUrl = details.EvaluationFileUrl;
                ViewBag.evaluation = details.EvaluationRemarks;
            }
            catch (Exception)
            {
            }

            return View();
        }

        public async Task<IActionResult> LetterUpload(string Id)
        {
            var details = _DbContext.FormsAndEntry.FirstOrDefault(c => c.EntryId == long.Parse(Id));
            ViewBag.status = details.Status;
            ViewBag.OperatorId = details.OrganizationName;
            ViewBag.title = details.ProductName;
            ViewBag.remarksUrl = details.ReviewFileUrl;
            ViewBag.evalUrl = details.EvaluationFileUrl;
            ViewBag.evalremarks = details.EvaluationRemarks;
            try
            {
                if (details.MasterEntryId > 0)
                {
                    var masterentry = _DbContext.FormsAndEntry.FirstOrDefault(c => c.EntryId == details.MasterEntryId);
                    ViewBag.MasterEntryId = masterentry.EntryId;
                    ViewBag.MasterEntryName = masterentry.ProductName;
                }
            }
            catch (Exception)
            {
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LetterUpload(string Id, IFormCollection response)
        {
            try
            {
                var entryid = long.Parse(Id);

                var history = _DbContext.FormsReview.Where(x => x.EntryId == entryid).AsNoTracking().FirstOrDefault();

                if (response.Files.Count != 0)
                {
                    var file = response.Files.FirstOrDefault();
                    var uploadsFolderPath = Path.Combine(_webHostEnv.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolderPath))
                        Directory.CreateDirectory(uploadsFolderPath);
                    var Extension = Path.GetExtension(file.FileName);
                    string fileName = "";

                    fileName = history.EntryId + "remark" + Extension;
                    history.FileUrl = fileName;
                    if (history != null)
                        history.FileUrl = fileName;


                    var filePath = Path.Combine(uploadsFolderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    var task = _DbContext.WorkflowManager.FirstOrDefault(e => e.ActionUrl.Contains(entryid.ToString()) && e.TaskType.ToLower() == "pending" && e.UserId == _Vars.UserId);

                    if (task == null)
                    {
                        TempData["error"] = "Task is not yet assigned to you!";
                        //ViewBag.message = 
                    }
                    else
                    {
                        var resp = await _taskHelper.TerminateTask(task.ActionUrl);

                        if (resp)
                        {
                            history.Status = task.Remarks.ToLower().Split("-")[1] == "a" ? "APPROVED" : "REJECTED";

                            _DbContext.FormsReview.Update(history);
                            await _DbContext.SaveChangesAsync(true);
                            TempData["message"] = "upload was successful...";

                            ViewBag.message = "upload was successful...";
                        }
                        else
                        {
                            TempData["error"] = "upload was Unsuccessful...";
                        }
                    }

 
                }
            }
            catch (Exception Ex)
            {
                TempData["error"] = Ex.Message;
            }

            return RedirectToAction("LetterUpload");
        }

        public IActionResult EvaluateTariff(long id)
        {
            var details = _DbContext.FormsAndEntry.FirstOrDefault(c => c.EntryId == id);

            ViewBag.OperatorId = details.OrganizationName;
            ViewBag.title = details.ProductName;
            ViewBag.remarksUrl = details.ReviewFileUrl;
            ViewBag.remarks = details.ReviewRemarks;
            ViewBag.evaluationUrl = details.EvaluationFileUrl;
            ViewBag.evaluation = details.EvaluationRemarks;
            ViewBag.status = details.ReviewStatus;

            try
            {
                //if (!string.IsNullOrEmpty(details.MasterEntryId.ToString()))
                if (details.MasterEntryId > 0)
                {
                    var masterentry = _DbContext.FormsAndEntry.FirstOrDefault(c => c.EntryId == details.MasterEntryId);
                    ViewBag.MasterEntryId = masterentry.EntryId;
                    ViewBag.MasterEntryName = masterentry.ProductName;
                }
            }
            catch (Exception ex)
            {
            }

            return View();
        }

        public IActionResult ApproveTariff(long id)
        {
            var details = _DbContext.FormsAndEntry.FirstOrDefault(c => c.EntryId == id);

            ViewBag.OperatorId = details.OrganizationName;
            ViewBag.title = details.ProductName;
            ViewBag.remarksUrl = details.ReviewFileUrl;
            ViewBag.remarks = details.ReviewRemarks;
            ViewBag.evaluationUrl = details.EvaluationFileUrl;
            ViewBag.evaluation = details.EvaluationRemarks;
            ViewBag.status = details.ReviewStatus;
            ViewBag.id = id;

            try
            {
                if (details.MasterEntryId > 0)
                {
                    var masterentry = _DbContext.FormsAndEntry.FirstOrDefault(c => c.EntryId == details.MasterEntryId);
                    ViewBag.MasterEntryId = masterentry.EntryId;
                    ViewBag.MasterEntryName = masterentry.ProductName;
                }
            }
            catch (Exception)
            {
            }

            return View();
        }

        public IActionResult TariffRequest(string status)
        {
            try
            {
                ViewBag.title = "List of Tariff Request(s)";
                ViewBag.SurveyList = new SelectList(Vars.ReviewStatus.Keys.ToList());
                ViewBag.TariffType = GetFormTypes(FormsType.TARIFF);

                if (status != null)
                {
                    ViewBag.entrys = _DbContext.FormsAndEntry.Where(c => c.Status == "SUBMITTED" && c.ReviewStatus == status && c.FormsType == Vars.FormTypes.TARIFF_TYPE.ToString()).OrderByDescending(x => x.DateSubmitted);
                    if (ViewBag.entrys == null) ViewBag.error += "Could not load report";
                }
                else
                {
                    ViewBag.entrys = _DbContext.FormsAndEntry.Where(c => c.Status == "SUBMITTED" && c.FormsType == Vars.FormTypes.TARIFF_TYPE.ToString()).OrderByDescending(x => x.DateSubmitted);
                }

                var host = HttpContext.Request.Host;
                var path = HttpContext.Request.Path;
                var scheme = HttpContext.Request.Scheme;
                var url = scheme + "://" + host + path;

                ViewBag.Url = url;

                return View("TariffRequest");
            }
            catch (Exception ex)
            {
                ViewBag.error = ex.Message;
                return View("TariffRequest");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateMemo(GenerateMemoRequest generateMemo)
        {
            var memoList = await _DbContext.Memo.Where(x => x.ProcessId == generateMemo.ProcessId).Select(x => new SelectListItem
            {
                Value = x.MemoContent,
                Text = x.MemoName
            }).ToListAsync();

            var viewModel = new FormsAndEntry()
            {
                MemoList = memoList,
                EntryId = generateMemo.EntryId,
                ProcessId = generateMemo.ProcessId,
                ProductName = generateMemo.ProductName,
                OrganizationName = generateMemo.OrganizationName
            };
            ViewBag.ActionType = generateMemo.ActionType;
            return View("~/Views/Forms/Memo/GenerateMemo.cshtml", viewModel);
        }

        [HttpPost]
        public IActionResult PreloadMemo(FormsAndEntry memoRequest)
        {
            var viewModel = new FormsAndEntry();
            try
            {
                viewModel.MemoContent = memoRequest.MemoContent;
                viewModel.EntryId = memoRequest.EntryId;
                viewModel.ProcessId = memoRequest.ProcessId;
                viewModel.ProductName = memoRequest.ProductName;
                viewModel.OrganizationName = memoRequest.OrganizationName;
            }
            catch (Exception ex)
            {
                ViewBag.error = "Could retrieve memo content. Please try again.";
                return RedirectToAction("TariffRequest");
            }

            ViewBag.ActionType = "PreloadMemo";
            return View("~/Views/Forms/Memo/GenerateMemo.cshtml", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveMemoContent(FormsAndEntry memoRequest)
        {
            try
            {
                var form = await _DbContext.FormsReview.Where(x => x.EntryId == memoRequest.EntryId).SingleOrDefaultAsync();

                form.MemoContent = memoRequest.MemoContent;
                _DbContext.FormsReview.Update(form).Property(x => x.RecId).IsModified = false;
                await _DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["error"] = "Could not save memo. Please try again.";
                return RedirectToAction("TariffRequest");
            }
            TempData["message"] = "Memo saved successfully";
            return RedirectToAction("TariffRequest");
        }

        public async Task<IActionResult> EditMemo(long id, string processId, string productName, string orgName)
        {
            var viewModel = new FormsAndEntry();
            try
            {
                var dbValue = await _DbContext.FormsReview.Where(x => x.EntryId == id).SingleOrDefaultAsync();

                viewModel.MemoContent = dbValue.MemoContent;
                viewModel.EntryId = id;
                viewModel.ProcessId = processId;
                viewModel.ProductName = productName;
                viewModel.OrganizationName = orgName;
            }
            catch (Exception ex)
            {
                ViewBag.error = "Could retrieve memo content. Please try again.";
                return RedirectToAction("TariffRequest");
            }

            ViewBag.ActionType = "PreloadMemo";

            return View("~/Views/Forms/Memo/GenerateMemo.cshtml", viewModel);
        }

        public async Task<IActionResult> PrintMemo(long id, int isWorkflow, string url, string actionType)
        {
            var details = _DbContext.FormsAndEntry.FirstOrDefault(c => c.EntryId == id);

            var msgInDb = _DbContext.FormsMessage.Where(x => x.EntryId == id)
           .OrderBy(x => x.DateSent.TimeOfDay).ToList();

            url = url.Replace("TariffRequest", $"ApproveTariff/{id}");

            if (url.Contains("SurveyDetails2"))
            {
                url = url.Replace("SurveyDetails2", "SurveyDetails");
            }

            if (url.Contains("ApproveTariff"))
            {
                url = url.Replace("ApproveTariff", "ViewTariff");
            }

            if (url.Contains("localhost"))
            {
                var uri = new Uri(url);
                var host = uri.Host;
                var port = uri.Port;

                url = url.Replace($"{host}:{port}", "pcea.ncc.gov.ng");
            }

            var Tasktrails = await _DbContext.WorkflowManager.Where(m => m.ActionUrl == url).OrderByDescending(o => o.RecId).ToListAsync();
            var getIds = Tasktrails.Select(sl => sl.UserId).ToList();

            var usersIntrail = await _DbContext.UserProfile.Where(w => getIds.Contains(w.UserId)).ToListAsync();

            Tasktrails.ForEach((t) =>
            {
                t.AdminFullName = usersIntrail.FirstOrDefault(f => f.UserId == t.UserId).Fullname;
            });

            ViewBag.TaskTrails = Tasktrails;

            ViewBag.Action = "PrintMemo";
            ViewBag.MemoContent = details.MemoContent;
            ViewBag.OperatorId = details.OrganizationName;
            ViewBag.title = details.ProductName;
            ViewBag.remarksUrl = details.ReviewFileUrl;
            ViewBag.remarks = details.ReviewRemarks;
            ViewBag.evaluationUrl = details.EvaluationFileUrl;
            ViewBag.evaluation = details.EvaluationRemarks;
            ViewBag.Messages = msgInDb;

            if (isWorkflow > 0)
                ViewBag.link = "/Forms/TariffRequest";
            else
                ViewBag.link = "/Workflows/TaskList";

            try
            {
                if (details.MasterEntryId > 0)
                {
                    var masterentry = _DbContext.FormsAndEntry.FirstOrDefault(c => c.EntryId == details.MasterEntryId);
                    ViewBag.MasterEntryId = masterentry.EntryId;
                    ViewBag.MasterEntryName = masterentry.ProductName;
                }
            }
            catch (Exception)
            {
            }

            if(actionType == "downloadMemo") return View("~/Views/Forms/Memo/DownloadMemo.cshtml");

            return View("~/Views/Forms/Memo/PrintMemo.cshtml");
        }

        [HttpGet]
        public IActionResult LoadMessages(long entryId, string oppUserId, string productName)
        {
            var msgInDB = _DbContext.FormsMessage.Where(x => x.EntryId == entryId
            && x.ProductName == productName && x.OperatorUserId == oppUserId)
            .OrderBy(x => x.DateSent.TimeOfDay).ToList();

            var oppDetails = _DbContext.UserProfile.SingleOrDefault(x => x.UserId == oppUserId);
            var nccDetails = _DbContext.UserProfile.SingleOrDefault(x => x.UserId == _Vars.UserId);

            var viewModel = new FormsMessage()
            {
                Messages = msgInDB,
                OperatorUserId = oppUserId,
                OperatorEmail = oppDetails.Email,
                OperatorId = oppDetails.OrganizationId,
                NccUserId = _Vars.UserId,
                ProductName = productName,
                NccEmail = nccDetails.Email,
                EntryId = entryId,
            };

            return View("~/Views/Forms/MessagesAdmin.cshtml", viewModel);
        }

        public async Task<IActionResult> SendMessage(FormsMessage message)
        {

            await _formMsgHelper.SendMail(message.OperatorEmail, "Form Message", message.MessageBody, 1);

            message.DateSent = DateTime.Now;
            if (_Vars.UserId.Contains("NCC"))
            {
                message.SentBy = "NCC";
            }
            else
            {
                message.SentBy = "OPERATOR";
            }

            _DbContext.Add(message);
            await _DbContext.SaveChangesAsync();
            //ViewBag.message = "Message sent successfully";
            return RedirectToAction("ViewTariff", new { id = message.EntryId, isWorkflow = 0 });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Others(long? id)
        {
            try
            {
                ViewBag.title = "List of Request(s) [Other Services]";
                ViewBag.SurveyList = LoadForm(FormsType.OTHER_SERVICES);
                ViewBag.OtherType = GetFormTypes(FormsType.OTHER_SERVICES);

                if (id != null)
                {
                    ViewBag.remarkStatus = Vars.ReviewStatus.ToList();
                    ViewBag.entrys = _DbContext.FormsAndEntry.Where(c => c.RecId == id);
                    if (ViewBag.entrys == null) ViewBag.error += "Could not load report";
                }

                return View("Others");
            }
            catch (Exception ex)
            {
                ViewBag.error = ex.Message;
                return View("Others");
            }
        }

        private bool FormsReviewExists(long id)
        {
            return _DbContext.FormsReview.Any(e => e.EntryId == id);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateStatus(long EntryId, string status, string Remarks)
        {
            try
            {
                var history = _DbContext.FormsReview.Where(x => x.EntryId == EntryId).FirstOrDefault();

                if (history != null)
                {
                    if (status != null)
                    {
                        if (status.ToLower().Contains("sendtosec"))
                        {
                            if (status.Split("-")[1].ToLower() == "a")
                                status = "APPROVED";
                            else if (status.Split("-")[1].ToLower() == "r")
                                status = "REJECTED";
                        }

                        history.Remarks = Remarks;
                        history.Status = status;
                    }
                    var date = DateTime.Now;
                    history.DateofConveyance = date;
                    var daySpan = new TimeSpan(36, 0, 0, 0);
                    var yearSpan = new TimeSpan(730, 0, 0, 0);
                    history.LaunchDate = date.Add(daySpan);
                    history.ExpiryDate = date.Add(yearSpan);
                    _DbContext.FormsReview.Update(history);
                    await _DbContext.SaveChangesAsync(true);
                }


                //var notificationActionTypes = "";

                //switch (status)
                //{
                //    case "APPROVED":
                //        notificationActionTypes = Vars.NotificationActionTypes["APPROVE"].ToString();
                //        break;
                //    case "REJECTED":
                //        notificationActionTypes = Vars.NotificationActionTypes["REJECT"].ToString();
                //        break;
                //}

                //var mailType = "";

                //switch (remark.DataType)
                //{
                //    case "SURVEY_NOTIFICATION":
                //        mailType = Vars.MailType["SURVEY_NOTIFICATION"].ToString();
                //        break;
                //    case "TARIFF_REQUEST_NOTIFICATION":
                //        mailType = Vars.MailType["TARIFF_REQUEST_NOTIFICATION"].ToString();
                //        break;
                //    case "OTHER_SERVICE_NOTIFICATION":
                //        mailType = Vars.MailType["OTHER_SERVICE_NOTIFICATION"].ToString();
                //        break;
                //}

                //var submission = _DbContext.FormsAndEntry.FirstOrDefault(x => x.EntryId == EntryId);

                //if (remark.Status != null)
                //{

                //    if (mailType != "SURVEY_NOTIFICATION")
                //    {
                //        // // _notificationMgt.SendNotification(submission.OperatorId, mailType, notificationActionTypes, submission.FormName);
                //    }
                //    else if (notificationActionTypes != "APPROVE")
                //    {
                //        // // _notificationMgt.SendNotification(submission.OperatorId, mailType, notificationActionTypes, submission.FormName);
                //    }

                //}

                return Ok();

            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }

        [HttpPost]
        public async Task<IActionResult> PostRemark(FormsReview remark)
        {
            try
            {
                var history = _DbContext.FormsReview.Where(x => x.EntryId == remark.EntryId).FirstOrDefault();


                if (remark.File != null)
                {
                    var uploadsFolderPath = Path.Combine(_webHostEnv.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolderPath))
                        Directory.CreateDirectory(uploadsFolderPath);
                    var Extension = Path.GetExtension(remark.File.FileName);
                    string fileName = "";
                    if (remark.Status != null)
                    {
                        fileName = remark.EntryId + "remark" + Extension;
                        remark.FileUrl = fileName;
                        if (history != null)
                            history.FileUrl = fileName;
                    }
                    else
                    {
                        fileName = remark.EntryId + "evaluation" + Extension;
                        remark.EvaluationFileUrl = fileName;
                        if (history != null)
                            history.EvaluationFileUrl = fileName;
                    }

                    var filePath = Path.Combine(uploadsFolderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        remark.File.CopyTo(stream);
                    }

                }

                if (FormsReviewExists(remark.EntryId))
                {
                    if (remark.Status != null)
                    {
                        history.Remarks = remark.Remarks;
                        history.Status = remark.Status;
                    }
                    else
                    {
                        history.EvaluationRemarks = remark.EvaluationRemarks;
                    }
                    var date = DateTime.Now;
                    history.DateofConveyance = date;
                    var daySpan = new TimeSpan(36, 0, 0, 0);
                    var yearSpan = new TimeSpan(730, 0, 0, 0);
                    history.LaunchDate = date.Add(daySpan);
                    history.ExpiryDate = date.Add(yearSpan);
                    _DbContext.FormsReview.Update(history);
                    await _DbContext.SaveChangesAsync(true);
                }
                else
                {
                    _DbContext.FormsReview.Add(remark);
                    await _DbContext.SaveChangesAsync(true);
                }

                var notificationActionTypes = "";

                switch (remark.Status)
                {
                    case "APPROVED":
                        notificationActionTypes = Vars.NotificationActionTypes["APPROVE"].ToString();
                        break;
                    case "REJECTED":
                        notificationActionTypes = Vars.NotificationActionTypes["REJECT"].ToString();
                        break;
                }

                var mailType = "";

                switch (remark.DataType)
                {
                    case "SURVEY_NOTIFICATION":
                        mailType = Vars.MailType["SURVEY_NOTIFICATION"].ToString();
                        break;
                    case "TARIFF_REQUEST_NOTIFICATION":
                        mailType = Vars.MailType["TARIFF_REQUEST_NOTIFICATION"].ToString();
                        break;
                    case "OTHER_SERVICE_NOTIFICATION":
                        mailType = Vars.MailType["OTHER_SERVICE_NOTIFICATION"].ToString();
                        break;
                }

                var submission = _DbContext.FormsAndEntry.FirstOrDefault(x => x.EntryId == remark.EntryId);

                if (remark.Status != null)
                {

                    if (mailType != "SURVEY_NOTIFICATION")
                    {
                        // // _notificationMgt.SendNotification(submission.OperatorId, mailType, notificationActionTypes, submission.FormName);
                    }
                    else if (notificationActionTypes != "APPROVE")
                    {
                        // // _notificationMgt.SendNotification(submission.OperatorId, mailType, notificationActionTypes, submission.FormName);
                    }

                }

                return Ok();

            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }

        public IActionResult Notifications(string type)
        {

            var title = "";

            var notification = new List<MailMessage>();

            if (type == null)
            {
                notification = _DbContext.MailMessage.OrderByDescending(x => x.RecId).ToList();

                title = "List of Notifications";

            }
            else
            {
                notification = _DbContext.MailMessage.OrderByDescending(x => x.RecId).Where(x => x.MailType == type).ToList();

                switch (type)
                {
                    case "TARIFF_REQUEST_NOTIFICATION":
                        title = "Tariff Notifications";
                        break;
                    case "SURVEY_NOTIFICATION":
                        title = "Survey Notifications";
                        break;
                }

            }

            notification.ForEach((itm) =>
            {
                itm.MailBody = System.Web.HttpUtility.HtmlDecode(itm.MailBody);
            });
            ViewBag.title = title;

            return View(notification);
        }


        [HttpPost]
        public JsonResult SDetails(IFormCollection frm)
        {
            try
            {
                string ErrMsg = string.Empty;
                string sCondition = string.Empty;

                if (frm != null)
                {

                    var objEntry = (from m in _DbContext.FormsEntry
                                    select m).Where(m => m.EntryId == long.Parse(frm["entry"])).ToList();

                    var objPrev = _DbContext.FormsDetails.FirstOrDefault(m => m.EntryId == long.Parse(frm["entry"]));
                    string _temp = objPrev == null ? "" : objPrev.FormDetails;

                    return Json(new List<object> { new { data = objEntry, template = _temp } }[0]);
                }
                else
                {
                    return Json("");
                }
            }
            catch (Exception ex)
            {
                ViewBag.error = ex.Message;
                return Json(ex.Message);
            }
        }

        public IActionResult SurveyDetails()
        {
            return View();
        }

        public IActionResult SurveyDetails2(long id)
        {
            ViewBag.ReviewStats = _DbContext.FormsReview.FirstOrDefault(e => e.EntryId == id).Status;
            ViewBag.id = id;
            return View();
        }

        [HttpPost]
        public JsonResult SurveyDetails(IFormCollection entrId)
        {
            try
            {
                string ErrMsg = string.Empty;
                string sCondition = string.Empty;
                var entryId = long.Parse(entrId["entry"].ToString());

                if (entryId <= 0 || !string.IsNullOrEmpty(entryId.ToString()))
                {
                    var objEntry = (from m in _DbContext.FormsEntry
                                    select m).Where(m => m.EntryId == entryId).ToList();

                    var objPrev = _DbContext.FormsDetails.FirstOrDefault(m => m.EntryId == entryId);
                    string _temp = objPrev == null ? "" : objPrev.FormDetails;

                    var formName = _DbContext.Forms.FirstOrDefault(e => e.RecId == objPrev.FormId).FormName;
                    ViewBag.title = formName;
                    ViewBag.OperatorId = objPrev.OperatorId;

                    return Json(new List<object> { new { data = objEntry, template = _temp } }[0]);
                }
                else
                {
                    return Json("");
                }
            }
            catch (Exception ex)
            {
                ViewBag.error = ex.Message;
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult TDetails(IFormCollection frm)
        {
            try
            {
                string ErrMsg = string.Empty;
                string sCondition = string.Empty;

                if (frm != null)
                {
                    var objEntry = (from m in _DbContext.FormsEntry
                                    select m).Where(m => m.EntryId == long.Parse(frm["entry"])).ToList();

                    var objPrev = _DbContext.FormsDetails.FirstOrDefault(m => m.EntryId == long.Parse(frm["entry"]));
                    string _temp = objPrev == null ? "" : objPrev.FormDetails;

                    //get list of changed field value in case form is an upgrade
                    try
                    {
                        if (objPrev.AppType.ToUpper() == "MODIFICATION")
                        {
                            var lMasterEntryId = objPrev.MasterEntryId;
                            var lEntryId = objPrev.EntryId;
                            foreach (var _NewFormsEntry in objEntry)
                            {
                                var _PrevFormsEntry = _DbContext.FormsEntry.FirstOrDefault(m => m.EntryId == lMasterEntryId && m.FieldName == _NewFormsEntry.FieldName);
                                if (_PrevFormsEntry != null)
                                {
                                    //field name exists, check if response changed
                                    if (_NewFormsEntry.Response.ToLower() != _PrevFormsEntry.Response.ToLower())
                                    {
                                        //response was amended. flag it
                                        sCondition += _NewFormsEntry.FieldName + ",";
                                    }
                                }
                                else
                                {
                                    //this is a newly added fieldname/entry. flag it
                                    sCondition += _NewFormsEntry.FieldName + ",";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrMsg = ex.Message;
                    }
                    if (string.IsNullOrEmpty(sCondition) == false)
                    {
                        sCondition = sCondition[0..^1];
                        sCondition = sCondition.Replace(",", "','");
                        sCondition = "'" + sCondition + "'";
                        var objChanges = _DbContext.FormsEntry.FromSqlRaw("SELECT * FROM dbo.FormsEntry WHERE FieldName IN(" + sCondition + ")");

                        return Json(new List<object> { new { data = objEntry, template = _temp, changeddata = objChanges } }[0]);
                    }
                    return Json(new List<object> { new { data = objEntry, template = _temp } }[0]);

                    //return Json(new List<object> { new { data = objEntry, template = _temp } }[0]);
                }
                else
                {
                    return Json("");
                }
            }
            catch (Exception ex)
            {
                ViewBag.error = ex.Message;
                return Json(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            try
            {
                //check if form already got entries
                _DbContext.Forms.Remove(_DbContext.Forms.FirstOrDefault(m => m.RecId == id));
                _DbContext.SaveChanges();

                TempData["message"] = "Form deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Duplicate(int id)
        {
            try
            {
                var objForm = _DbContext.Forms.FirstOrDefault(m => m.RecId == id);
                objForm.RecId = 0;
                objForm.Published = false;
                objForm.DateCreated = DateTime.Now;
                objForm.UserId = _Vars.UserId;
                string err = "";
                //if (objForm.UserId == _Vars.UserId)
                //{
                _DbContext.Forms.Add(objForm);
                _DbContext.SaveChanges(true);
                err = "Duplication was successful";
                //}
                //else err = "Only the form creator is allowed to duplicate";

                TempData["message"] = err;
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Build(long? id)
        {
            try
            {

                var objForm = await _DbContext.Forms.FindAsync(id == null ? 0 : id);
                if (id != null && objForm == null)
                {
                    return NotFound();
                }

                if (objForm != null)
                {
                    if (objForm.Published)
                    {
                        TempData["error"] = "Editing published Forms definition is not allowed... Contact System Adminstrator!";
                        return RedirectToAction(nameof(Index));
                    }
                }


                ViewBag.processList = new SelectList((from c in _DbContext.Workflow select c).ToList(), "ProcessId", "ProcessName");
                ViewBag.OperatorType = GetFormTypes(FormsType.SURVEY);
                ViewBag.TariffType = GetFormTypes(FormsType.TARIFF);
                ViewBag.OtherType = GetFormTypes(FormsType.OTHER_SERVICES);

                return View(objForm);
            }
            catch (Exception ex)
            {
                ViewBag.message = ex.Message;
                return View();
            }


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Build(Forms strPost, IFormCollection Report)
        {
            try
            {
                //if (ModelState.IsValid)
                //{
                var reports = Report["Report"];
                strPost.UserId = _Vars.UserId;
                if (strPost.RecId > 0)
                {
                    strPost.LastUpdate = DateTime.Now;
                    strPost.FormsTypeCategory = Report["CategoryType"];
                    _DbContext.Entry(strPost).State = EntityState.Modified;
                    _DbContext.Entry(strPost).Property("DateCreated").IsModified = false;
                    if (strPost.TerminalDate < DateTime.Today) _DbContext.Entry(strPost).Property("TerminalDate").IsModified = false;
                    //_DbContext.Forms.Update(strPost);
                }
                else
                {
                    _DbContext.Forms.Add(strPost);
                }

                //var rpts = reports.ToString().Split(",");
                //var title = new List<string>();

                //int i = 0;
                //foreach (var rpt in rpts)
                //{
                //    var _name = rpt.Substring(0, rpt.IndexOf(":"));
                //    if (i <= 0)
                //        title.Add(_name);
                //    else if (!title.Contains(_name))
                //    {

                //    }
                //    i++;
                //}
                _DbContext.SaveChanges(true);

                TempData["message"] = "Forms update was successfully";
                return RedirectToAction(nameof(Index));
                //}
            }
            catch (DbUpdateConcurrencyException ex)
            {
                TempData["error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction(nameof(Build), strPost.RecId);
        }



        //-----------DELETE ALL CODES BELOW HERE ONES I FINISH WITH MY ADJUSTMENTS TO Survey Details, TariffRequest, backend Remarks.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessForm(IFormCollection frm)
        {
            try
            {
                if (frm != null)
                {
                    var objEntry = from c in _DbContext.FormsEntry where c.EntryId == long.Parse(frm["Entry"]) select c;
                    if (objEntry != null)
                    {
                        foreach (var entry in objEntry) entry.Status = frm["Status"];
                        if (frm.ContainsKey("REJECTED"))
                        {
                            //var objDetails = from c in _DbContext.FormsDetails where c.EntryId == long.Parse(frm["Entry"]) sele
                            var objDetails = _DbContext.FormsDetails.FirstOrDefault(m => m.EntryId == long.Parse(frm["Entry"]));
                            objDetails.Message = frm["REJECTED"];
                            _DbContext.Update(objDetails);
                        }
                        _DbContext.UpdateRange(objEntry);
                        _DbContext.SaveChanges();
                        TempData["message"] += "Survey was processed successfully";
                    }
                    else
                    {
                        TempData["error"] += "Invalid Survey ID";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["error"] += ex.Message;
            }
            return RedirectToAction(nameof(Details));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessTariffRejectOrAccept(IFormCollection frm)
        {
            try
            {
                if (frm != null)
                {
                    var objEntry = from c in _DbContext.FormsEntry where c.EntryId == long.Parse(frm["Entry"]) select c;
                    if (objEntry != null)
                    {
                        foreach (var entry in objEntry) entry.Status = frm["Status"];
                        if (frm.ContainsKey("REJECTED"))
                        {
                            //var objDetails = from c in _DbContext.FormsDetails where c.EntryId == long.Parse(frm["Entry"]) sele
                            var objDetails = _DbContext.FormsDetails.FirstOrDefault(m => m.EntryId == long.Parse(frm["Entry"]));
                            objDetails.Message = frm["REJECTED"];
                            objDetails.AppType = "NEW";
                            _DbContext.Update(objDetails);
                        }
                        _DbContext.UpdateRange(objEntry);
                        _DbContext.SaveChanges();
                        TempData["message"] += "Message processed successfully";
                    }
                    else
                    {
                        TempData["error"] += "Invalid Tariff Request ID";
                    }
                }
                //return View("TariffRequest");
            }
            catch (Exception ex)
            {
                TempData["error"] += ex.Message;
            }
            //return RedirectToAction(nameof(Details));
            return View("TariffRequest");
        }


    }
}
