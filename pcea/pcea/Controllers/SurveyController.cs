using HtmlAgilityPack;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using pcea.Models;
using pceaLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Path = System.IO.Path;
using System.Threading.Tasks;
using pcea.Helpers;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;

namespace pcea.Controllers
{
    public class SurveyController : Controller
    {
        private readonly PceaDbContext _DbContext;
        private readonly IHttpContextAccessor _HttpContext;
        private readonly IWebHostEnvironment _webHostEnv;
        private readonly IHostingEnvironment hosting;
        private IConfiguration _Configuration;
        private Vars _Vars;
        private NotificationMgt _notificationMgt;
        private TaskHelper TaskHelper;
        FormMsgHelper _formMsgHelper;
        public SurveyController(PceaDbContext context, IHttpContextAccessor httpContext, IConfiguration configuration, IWebHostEnvironment webHostEnv, IHostingEnvironment hosting)
        {
            _DbContext = context;
            _HttpContext = httpContext;
            _webHostEnv = webHostEnv;
            this.hosting = hosting;
            _Configuration = configuration;
            _Vars = new Vars(_HttpContext.HttpContext);
            _notificationMgt = new NotificationMgt(hosting);
            TaskHelper = new TaskHelper(context, httpContext, hosting);
            _formMsgHelper = new FormMsgHelper(configuration);
        }

        private async Task<List<Forms>> UpdatePublished(List<Forms> forms)
        {
            var expiredforms = _DbContext.Forms.Where(w => !forms.Select(sl => sl.RecId).ToList().Contains(w.RecId)).ToList();

            var sql = $"UPDATE [dbo].[Forms] SET [Published]=0 WHERE [RecId] IN ({string.Join(",", expiredforms.Select(sl => sl.RecId).ToArray())})";

            _DbContext.Database.ExecuteSqlRaw(sql);

            return (from m in _DbContext.Forms select m).Where(m => m.Published && m.FormsType.ToLower() == "operator_type" && m.TerminalDate <= DateTime.Now).ToList();
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.title = "Questionnaires for Operators";

            ViewBag.operatorName = _Vars.OperatorName;
            ViewBag.operatorID = _Vars.OperatorId;

            ViewBag.OperType = new SelectList(
                    (from m in _DbContext.MetaDataRef select m).Where(m => m.MetaDataType == "OPERATOR_TYPE")
                    .OrderBy(m => m.ReferenceId), "ReferenceId", "ReferenceId");

            var currentdate = DateTime.Now;
            var objForms = _DbContext.Forms.FromSqlRaw($"SELECT * FROM dbo.Forms WHERE Published=1 AND FormsType='OPERATOR_TYPE'").ToList();
            // objForms = await UpdatePublished(objForms);

            var recs = (from m in _DbContext.FormsOperators where m.OperatorId == _Vars.OperatorId select m).ToList();
            //objForms = objForms.TakeWhile(t => t.TerminalDate <= DateTime.Now).ToList();
            ViewBag.entry = recs;
            var frmreviews = _DbContext.FormsReview.Where(e => recs.Select(f => f.EntryId).Contains(e.EntryId)).ToList();
            ViewBag.review = frmreviews;
            //ViewBag.processList = new SelectList((from c in _DbContext.Workflow select c).ToList(), "ProcessId", "ProcessName");

            return View(objForms);
        }        
        
        public async Task<IActionResult> SurveyIndex(Object data)
        {
            ViewBag.title = "Questionnaires for Operators";

            ViewBag.operatorName = _Vars.OperatorName;
            ViewBag.operatorID = _Vars.OperatorId;

            ViewBag.OperType = new SelectList(
                    (from m in _DbContext.MetaDataRef select m).Where(m => m.MetaDataType == "OPERATOR_TYPE")
                    .OrderBy(m => m.ReferenceId), "ReferenceId", "ReferenceId");

            var currentdate = DateTime.Now;
            var objForms = _DbContext.Forms.FromSqlRaw($"SELECT * FROM dbo.Forms WHERE Published=1 AND FormsType='OPERATOR_TYPE'").ToList();
            // objForms = await UpdatePublished(objForms);

            var recs = (from m in _DbContext.FormsOperators where m.OperatorId == _Vars.OperatorId select m).ToList();
            //objForms = objForms.TakeWhile(t => t.TerminalDate <= DateTime.Now).ToList();
            ViewBag.entry = recs;
            var frmreviews = _DbContext.FormsReview.Where(e => recs.Select(f => f.EntryId).Contains(e.EntryId)).ToList();
            ViewBag.review = frmreviews;
            ViewBag.submission = data;
            //ViewBag.processList = new SelectList((from c in _DbContext.Workflow select c).ToList(), "ProcessId", "ProcessName");

            return View(objForms);
        }


        public IActionResult Notification()
        {
            ViewBag.title = "List of Notifications";

            ViewBag.NotificationType = new SelectList(Vars.MailType.Keys.ToList());

            var notification = _DbContext.MailMessage.OrderByDescending(x => x.RecId).Where(x => x.ReferenceNo == _Vars.OperatorId).ToList();

            notification.ForEach((itm) =>
            {
                itm.MailBody = System.Web.HttpUtility.HtmlDecode(itm.MailBody);
            });

            return View(notification);
        }

        public JsonResult GetCompanyInfo(string formId)
        {
            CompanyDataHelper._DbContext = _DbContext;
            var data = CompanyDataHelper.GetData(_Vars);

            if (string.IsNullOrEmpty(data))
            {
                return Json(new { err = "error", isLoaded = false });
            }

            var list = data.Split(",");
            var keypair = new List<KeyPair>();
            foreach (var item in list)
            {
                if (!string.IsNullOrEmpty(item) && item.Contains(":"))
                {
                    var obj = item.Split(":");

                    keypair.Add(new KeyPair() { Name = obj[1], Value = obj[2] });
                }
            }


            return Json(new { data = keypair, isLoaded = true });
        }


        //public JsonResult GetCompanyInfo(string formId)
        //{
        //    var fields = _DbContext.Forms.FirstOrDefault(e => e.RecId == long.Parse(formId)).CompanyInfoFields;
        //    var checkOprInfo = _DbContext.CompanyDataSubmissions.FirstOrDefault(e => e.OrganizationId == _Vars.OperatorId);

        //    if (checkOprInfo != null)
        //    {
        //        var stringList = checkOprInfo.FormFieldsData.Split(",").ToList();
        //        List<KeyPair> keyPairs = new List<KeyPair>();
        //        stringList.ForEach((param) =>
        //        {
        //            if (!string.IsNullOrEmpty(param))
        //            {
        //                var array = param.Split(":");
        //                var id = array[0];
        //                var lblName = array[1];
        //                var value = array[2];
        //                //var array = param.Split(":");
        //                keyPairs.Add(new KeyPair { Name = lblName, Value = $"{id}:{value}" });
        //            }

        //        });

        //        var compDataFieldntries = keyPairs;

        //        return Json(new { data = compDataFieldntries, isLoaded = true });
        //    }
        //    else if (!string.IsNullOrEmpty(fields))
        //    {
        //        var companyInfo = _DbContext.SsoUser.Where(e => e.OrganizationId == _Vars.OperatorId).OrderByDescending(e => e.DateLogin).Select(sl => new
        //        {
        //            FullName = $"{sl.LastName} {sl.MiddleName} {sl.FirstName}",
        //            sl.OrganizationLongName,
        //            sl.OrganizationShortName,
        //            sl.MobileNumber,
        //            ContactEmail = sl.AppUserEmail
        //        }).FirstOrDefault();
        //        var _fields = fields.Split(",");

        //        var labels = new List<string>() { "Legal Name", "Telephone", "Email", "Operating or Tarde Name" };

        //        int i = 0;
        //        var fieldsData = new List<string>();
        //        labels.ForEach((param) =>
        //        {
        //            _fields.ToList().Where(e => e.Contains(param)).Select(sl => new
        //            {
        //                sl = i == 0 ? sl + "-" + companyInfo.OrganizationLongName : i == 1 ? sl + "-" + companyInfo.MobileNumber : sl + "-" + companyInfo.ContactEmail
        //            });

        //            if (i == 0)
        //            {
        //                var check = _fields.FirstOrDefault(e => e.Contains(param));
        //                fieldsData.Add(check + ":" + companyInfo.OrganizationLongName);
        //            }
        //            if (i == 1)
        //            {
        //                var check1 = _fields.FirstOrDefault(e => e.Contains(param));

        //                fieldsData.Add(check1 + ":" + companyInfo.MobileNumber);

        //            }
        //            if (i == 2)
        //            {
        //                var check2 = _fields.FirstOrDefault(e => e.Contains(param));
        //                fieldsData.Add(check2 + ":" + companyInfo.ContactEmail);
        //            }

        //            i++;
        //        });

        //        return Json(new { data = fieldsData, isLoaded = false });
        //    }



        //    return Json(new { });
        //}
        [HttpPost]
        public async Task<IActionResult> Index(IFormCollection response)
        {
            string sFormDetails = string.Empty;

            try
            {
                var frmId = long.Parse(response["FormId"].ToString());
                if (response.ContainsKey("EntryId"))
                {
                    var entrid = response["EntryId"].ToString();

                    if (!string.IsNullOrEmpty(entrid))
                    {
                        var check = _DbContext.FormsEntry.FirstOrDefault(x => x.EntryId == long.Parse(entrid));
                        if (check != null)
                        {
                            if (check.Status == "SUBMITTED" && response["ReviewStats"] != "REJECTED")
                            {
                                TempData["error"] = "Entry cannot be submitted twice";
                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                long id = long.Parse(response["EntryId"]);
                                if (_DbContext.FormsDetails.FirstOrDefault(m => m.EntryId == id) != null)
                                {
                                    _DbContext.FormsDetails.Remove(_DbContext.FormsDetails.FirstOrDefault(m => m.EntryId == id));
                                }
                                _DbContext.FormsReview.Remove(_DbContext.FormsReview.FirstOrDefault(m => m.EntryId == id));
                                _DbContext.FormsEntry.RemoveRange(from c in _DbContext.FormsEntry where c.EntryId == long.Parse(response["EntryId"]) select c);
                                _DbContext.SaveChanges(true);
                            }
                        }
                    }


                }

                //get form details into a variable
                foreach (var itm in response)
                {
                    string fieldId = itm.Key;
                    if (fieldId.ToLower() == "formdetails")
                    {
                        sFormDetails = itm.Value.ToString();
                        sFormDetails = sFormDetails.Replace("&nbsp;", " ");
                        break;
                    }
                }

                var formYr = _DbContext.Forms.FirstOrDefault(e => e.RecId == frmId).FormYear;
                //var time = DateTime.Now.Subtract(new TimeSpan(365, 6, 0, 0));
                //formYr = "2019";
                //time = time.Subtract(new TimeSpan(365, 6, 0, 0));

                int index = -1;
                string labels = response["dataLabels"];
                var companyData = string.Empty;
                string status = response["Status"];
                long EntryID = DateTime.Now.ToFileTime();
                List<FormsEntry> objEntries = new List<FormsEntry>();
                foreach (var itm in response)
                {
                    index++;
                    var fieldId = itm.Key;
                    if (itm.Key.ToLower() != "entryid" && itm.Key.ToLower() != "formid" && itm.Key.ToLower() != "status" && itm.Key.ToLower() != "formdetails" && itm.Key.ToLower() != "reviewstats")
                    {
                        string fieldLabel = GetFieldLabel2(sFormDetails, fieldId);

                        //if (labels.Split(":").Contains(fieldLabel))
                        //{
                        //    companyData = companyData == null ? $"{fieldId}:{fieldLabel}:{itm.Value}" : $"{companyData},{fieldId}:{fieldLabel}:{itm.Value}";
                        //}
                        var labelParts = string.Empty;

                        //if(fieldLabel.Any(char.IsNumber))
                        labelParts = fieldLabel.Split(" ").Last();

                        objEntries.Add(
                            new FormsEntry
                            {
                                FieldLabel = fieldLabel,
                                FieldName = itm.Key,
                                Response = itm.Value.ToString(),
                                UserId = _Vars.UserId,
                                OperatorId = _Vars.OperatorId,
                                FormId = long.Parse(response["FormId"]),
                                DateSubmitted = DateTime.Now,
                                EntryId = EntryID,
                                Status = status.ToUpper(),
                                FrmYear = formYr,
                                ValueYear = labelParts.All(char.IsNumber) ? labelParts : ""
                            }
                        );
                    }
                }

                //if (!string.IsNullOrEmpty(companyData))
                //{
                //    var compData = new CompanyDataSubmission()
                //    {
                //        FormFieldsData = companyData,
                //        FormId = long.Parse(response["FormId"]),
                //        OrganizationId = _Vars.OperatorId
                //    };

                //    var _compData = _DbContext.CompanyDataSubmissions.FirstOrDefault(e => e.OrganizationId == _Vars.OperatorId);

                //    if (_compData != null)
                //    {
                //        _compData.FormFieldsData = companyData;
                //        _compData.FormId = long.Parse(response["FormId"]);
                //        _DbContext.CompanyDataSubmissions.Update(_compData);
                //    }
                //    else
                //    {
                //        _DbContext.CompanyDataSubmissions.Add(compData);
                //    }
                //}

                var empty = objEntries.Where(e => e.FieldLabel == "" || e.FieldLabel == null).ToList();

                //for(var i = index--; index < index--; index--)
                //{
                //    var entry = objEntries.FirstOrDefault(r => empty.Select(e => e.FieldName).ToList().Contains(r.FieldName)).FieldName;

                //    var label = GetFieldLabel(sFormDetails, entry, index);
                //}

                foreach (var item in empty)
                {
                    var label = GetFieldLabel2(sFormDetails, item.FieldName);

                    if (label != null)
                    {
                        objEntries.FirstOrDefault(e => e.FieldName == item.FieldName).FieldLabel = label;
                    }
                }

                var empty2 = objEntries.Where(e => e.FieldLabel == "" || e.FieldLabel == null).ToList();

                _DbContext.FormsEntry.AddRange(objEntries);
                _DbContext.FormsDetails.Add(new FormsDetails
                {
                    OperatorId = _Vars.OperatorId,
                    FormId = long.Parse(response["FormId"]),
                    FormDetails = response["FormDetails"],
                    EntryId = EntryID
                });
                FormsReview _FormsReview = new FormsReview
                {
                    EntryId = EntryID,
                    Status = Vars.ReviewStatus["PENDING"]
                };
                _DbContext.FormsReview.Add(_FormsReview);

                _DbContext.SaveChanges(true);

                if (status != "SAVED")
                {
                    var submission = _DbContext.Forms.FirstOrDefault(x => x.RecId == long.Parse(response["FormId"]));

                    // // _notificationMgt.SendNotification(_Vars.OperatorId, Vars.MailType["SURVEY_NOTIFICATION"].ToString(), Vars.NotificationActionTypes["SUBMIT"].ToString(), submission.FormName);

                    var isSuccess = await TaskHelper.InitiateTask(int.Parse(response["FormId"]), EntryID);

                }

                TempData["message"] = "Your entry was captured successfully";
                return RedirectToAction(nameof(Index));


                //return RedirectToAction(nameof(Index), sFormDetails);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message + ": " + ex.InnerException;
                return RedirectToAction(nameof(Index));

                //return RedirectToAction(nameof(SurveyIndex), sFormDetails);
            }
        }

        private bool FormsEntryExists(long id)
        {
            return _DbContext.FormsEntry.Any(e => e.EntryId == id);
        }

        private bool FormsDetailsExists(long id)
        {
            return _DbContext.FormsDetails.Any(e => e.EntryId == id);
        }

        //private bool TariffHistoryExists(long id)
        //{
        //    return _DbContext.TariffHistory.Any(e => e.EntryIdMaster == id);
        //}
        private bool FormsReviewExists(long id)
        {
            return _DbContext.FormsReview.Any(e => e.EntryId == id);
        }

        public IActionResult Tariff()
        {
            ViewBag.operatorName = _Vars.OperatorName;
            ViewBag.operatorID = _Vars.OperatorId;

            ViewBag.forms = (from m in _DbContext.Forms select m).Where(m => m.Published && m.FormsType.ToLower() == "tariff_type").ToList();

            ViewBag.frmandentr = (from n in _DbContext.FormsAndEntry select n).Where(m => m.Published && m.FormsType.ToLower() == "tariff_type").ToList();

            var objForms = (from m in _DbContext.FormsOperators where m.OperatorId == _Vars.OperatorId select m).OrderByDescending(o => o.DateSubmitted).ToList();

            return View("Tariff", objForms);
        }

        [HttpGet]
        public IActionResult LoadMessages(long entryId)
        {
            var msgInDb = _DbContext.FormsMessage.Where(x => x.EntryId == entryId)
            .OrderBy(x => x.DateSent.TimeOfDay).ToList();

            var viewModel = new FormsMessage();
            if (msgInDb.Count != 0)
            {
                {
                    viewModel.Messages = msgInDb;
                    viewModel.OperatorUserId = _Vars.UserId;
                    viewModel.OperatorEmail = msgInDb.FirstOrDefault().OperatorEmail;
                    viewModel.OperatorId = msgInDb.FirstOrDefault().OperatorId;
                    viewModel.NccUserId = msgInDb.FirstOrDefault().NccUserId;
                    viewModel.ProductName = msgInDb.FirstOrDefault().ProductName;
                    viewModel.NccEmail = msgInDb.FirstOrDefault().NccEmail;
                    viewModel.EntryId = entryId;
                };
                return View("~/Views/Survey/MessagesOpp.cshtml", viewModel);
            }

            return Json(new { error = true });
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
            //ViewBag.successMsg = "Message sent successfully";

            return RedirectToAction("Tariff");
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
                                    select m).Where(m => m.EntryId == long.Parse(frm["entry"]) && m.IsFile != true).ToList();
                    var files = (from m in _DbContext.FormsEntry
                                 select m).Where(m => m.EntryId == long.Parse(frm["entry"]) && m.IsFile == true)
                                 .OrderBy(e => e.RecId).ToList();
                    var objPrev = _DbContext.FormsDetails.FirstOrDefault(m => m.EntryId == long.Parse(frm["entry"]));
                    string _temp = objPrev == null ? "" : objPrev.FormDetails;

                    return Json(new List<object> { new { data = objEntry, template = _temp, _files = files } }[0]);
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

        private void getFilePatch()
        {
            var uploadsFolderPath = Path.Combine(_webHostEnv.WebRootPath, "uploads");
        }

        [HttpPost]
        public async Task<IActionResult> Tariff(IFormCollection response)
        {
            try
            {
                long EntryId = 0;
                string sStatus = response["Status"].ToString().ToUpper();
                string appType = string.Empty;
                string reviewStat = response["reviewstatus"].ToString().ToUpper();
                string sAppType = response["AppType"].ToString().ToUpper();
                string fileName = "";
                var filePaths = new List<string>();
                List<IFormFile> files = new List<IFormFile>();
                var prevFile = new List<FormsEntry>();
                var formYr = _DbContext.Forms.FirstOrDefault(e => e.RecId == long.Parse(response["FormId"])).FormYear;

                //check if entry exists already
                if (response.ContainsKey("EntryId"))
                {
                    EntryId = long.Parse(response["EntryId"]);
                    var _FormsEntry = _DbContext.FormsEntry.Where(m => m.EntryId == EntryId).FirstOrDefault();
                    if (_FormsEntry.Status.ToUpper() == "SUBMITTED" && response.ContainsKey("reviewstatus") && reviewStat != "REJECTED")
                    {
                        //form submitted previously.  return error.
                        TempData["error"] = "Entry cannot be submitted twice";
                        return RedirectToAction(nameof(Tariff));
                    }
                    if (sStatus == "SAVED" || sStatus == "SUBMITTED" && reviewStat == "REJECTED")
                    {
                        prevFile.AddRange(_DbContext.FormsEntry.Where(m => m.EntryId == EntryId && m.IsFile == true).ToList());



                        _DbContext.FormsEntry.RemoveRange(_DbContext.FormsEntry.Where(m => m.EntryId == EntryId));
                    }
                }

                //if (EntryId == 0) EntryId = long.Parse(response["EntryId"]);

                long NewEntryId = DateTime.Now.ToFileTime();
                List<FormsEntry> objEntries = new List<FormsEntry>();
                //List<FormsEntry> objFileEntries = new List<FormsEntry>();
                long lFormId = long.Parse(response["FormId"].ToString());
                string sFormdetails = response["FormDetails"];
                string labelValue = response["labelValue"];
                string labels = response["labels"];
                long lMasterEntryId = 0;
                if (sAppType == "MODIFICATION")
                {
                    long.TryParse(response["MasterEntryId"].ToString(), out lMasterEntryId);
                }
                else if (sAppType == "REVALIDATION")
                {
                    long.TryParse(response["MasterEntryId"].ToString(), out lMasterEntryId);
                }
                else
                {
                    sAppType = "NEW";   //set AppType to NEW in case no selection was made by the user

                }



                //get form details into a variable
                string sFormDetails = string.Empty;
                foreach (var itm in response)
                {
                    string fieldId = itm.Key;
                    if (fieldId.ToLower() == "formdetails")
                    {
                        sFormDetails = itm.Value.ToString();
                        sFormDetails = sFormDetails.Replace("&nbsp;", " ");
                        break;
                    }
                }

                var uploadsFolderPath = Path.Combine(_webHostEnv.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolderPath))
                    Directory.CreateDirectory(uploadsFolderPath);

                //creates file paths
                if (response.Files.Count > 0)
                {
                    //iterate through new submission
                    foreach (IFormFile file in response.Files)
                    {
                        if (prevFile.Count > 0)
                        {
                            //iterate through prevfiles to be added to the new submission
                            var checkFile = prevFile.Where(e => e.FieldName == file.Name);
                            foreach (var item in prevFile)
                            {
                                //check field name of previous against present submission
                                //var checkField = prevFile.Where(e => e.FieldName == file.Name).FirstOrDefault();
                                //var check = response.Where(e => e.Key == item.FieldName).FirstOrDefault();

                                //check.ToString();
                                //if (item.FieldName == file.Name)
                                //if (file.FileName != null)
                                //{
                                //If exists, then check if the file changed and change the filename if no match and

                                //reset necessary fields and add it to the present submission
                                if (item.FieldName == file.Name)
                                {
                                    if (item.Response != file.FileName)
                                        item.Response = file.FileName;
                                }

                                item.EntryId = NewEntryId;
                                item.RecId = 0;
                                item.Status = sStatus;
                                item.DateSubmitted = DateTime.Now;

                                objEntries.Add(item);
                                //}
                            }
                        }
                        //if field name does not exist then it is a newly added file
                        var Extension = Path.GetExtension(file.FileName);

                        files.Add(file);
                        var filename = $"{_Vars.OperatorName}{NewEntryId}{file.FileName}";
                        var filePath = Path.Combine(uploadsFolderPath, filename);

                        filePaths.Add(filePath);

                        objEntries.Add(
                        new FormsEntry
                        {
                            FieldName = file.Name,
                            UserId = _Vars.UserId,
                            OperatorId = _Vars.OperatorId,
                            FormId = lFormId,
                            DateSubmitted = DateTime.Now,
                            EntryId = NewEntryId,
                            Status = sStatus,
                            Response = filename,
                            //Response = file.FileName,
                            IsFile = true,
                            FrmYear = formYr
                        });
                    }


                    //adds files to the upload folder
                    foreach (var path in filePaths)
                    {
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            // foreach (var file in files)
                            // {
                            //     file.CopyTo(stream);
                            // }

                            var file = files.FirstOrDefault(e => path.Contains(e.FileName));
                            file.CopyTo(stream);
                        }
                    }
                }
                else
                {
                    //no file added
                    //resets the previously uploaded file(s) to fit into the current submission batch
                    foreach (var item in prevFile)
                    {
                        var fieldCheck = response.Where(e => e.Key == item.FieldName);

                        if (fieldCheck.Any())
                        {
                            item.EntryId = NewEntryId;
                            item.RecId = 0;
                            item.Status = sStatus;

                            objEntries.Add(item);
                        }
                    }
                }

                _DbContext.FormsEntry.AddRange(objEntries);

                // index count
                int index = -1;
                objEntries = new List<FormsEntry>();
                foreach (var itm in response)
                {
                    index++;
                    //string Id = itm.Key;
                    string fieldId = itm.Key;// Regex.Replace(Id, @"[^0-9a-zA-Z-]+", "");
                    var check = !prevFile.Where(e => e.FieldName == fieldId).Any();

                    string val = itm.Value.ToString();
                    if (fieldId.ToLower() != "entryid" && fieldId.ToLower() != "formid" && fieldId.ToLower() != "status" &&
                        fieldId.ToLower() != "formdetails" && fieldId.ToLower() != "requestverificationtoken" &&
                        fieldId.ToLower() != "apptype" && fieldId.ToLower() != "masterentryid" && fieldId.ToLower() != "reviewstatus" && fieldId.ToLower() != "productname" && fieldId.ToLower() != "labelvalue" && fieldId.ToLower() != "labels" && fieldId.ToLower() != "productconcept" && !prevFile.Where(e => e.FieldName == fieldId).Any() && fieldId.ToLower() != "licensecat")
                    {
                        string fieldLabel = GetFieldLabel(sFormDetails, fieldId, index);

                        objEntries.Add(
                            new FormsEntry
                            {
                                FieldName = fieldId,
                                Response = val,
                                UserId = _Vars.UserId,
                                OperatorId = _Vars.OperatorId,
                                FormId = lFormId,
                                DateSubmitted = DateTime.Now,
                                EntryId = NewEntryId,
                                Status = sStatus,
                                FieldLabel = fieldLabel,
                                FrmYear = formYr
                            });
                    }
                }
                _DbContext.FormsEntry.AddRange(objEntries);

                //////////////-----Bodeniyi 2020-11-16--------//////////////////
                // insert/update record in form details
                if (FormsDetailsExists(EntryId))
                {
                    var _FormsDetails = _DbContext.FormsDetails.Where(m => m.EntryId == EntryId).FirstOrDefault();

                    //If sStatus is saved then the record is updated in formsdetails
                    //if (sStatus == "SAVED")
                    if (_FormsDetails.Status == "SAVED")
                    {
                        _FormsDetails.EntryId = NewEntryId;
                        _FormsDetails.FormDetails = sFormdetails;

                        if (reviewStat == "REJECTED")
                        {
                            if (sAppType == "MODIFICATION" || sAppType == "REVALIDATION")
                                appType = sAppType;
                            else
                                appType = "RE-FILLED";
                            _FormsDetails.AppType = appType;
                        }
                        else if (sAppType == "NEW")
                            _FormsDetails.AppType = sAppType;

                        //_FormsDetails.MasterEntryId = lMasterEntryId;
                        _FormsDetails.ProductName = response["productName"];
                        _FormsDetails.ProductConcept = response["productConcept"];
                        _FormsDetails.ExportDetails = labelValue;
                        _FormsDetails.ExportLabels = labels;
                        _FormsDetails.Status = sStatus;
                        _FormsDetails.LicenseCategory = response["licenseCat"];

                        _DbContext.FormsDetails.Update(_FormsDetails);
                    }
                    else
                    {
                        //If record has reviewstat as rejected then it is not a first time entry then two records are created, one new and one old.
                        if (reviewStat == "REJECTED")
                        {
                            //Old record is updated
                            //Check for the resubmission type and set the AppType appriopriately
                            if (sAppType == "MODIFICATION" || sAppType == "REVALIDATION")
                                appType = sAppType;
                            else
                                appType = "RE-FILLED";

                            _FormsDetails.AppType = appType;
                            _FormsDetails.OldEntryId = NewEntryId;    //Old record is link to new record
                            _DbContext.FormsDetails.Update(_FormsDetails);

                            //New record is created
                            _DbContext.FormsDetails.Add(new FormsDetails
                            {
                                OperatorId = _Vars.OperatorId,
                                FormId = long.Parse(response["formId"]),
                                FormDetails = sFormdetails,
                                EntryId = NewEntryId,
                                Status = sStatus,
                                AppType = sAppType,
                                MasterEntryId = lMasterEntryId,
                                ProductName = response["productName"],
                                ProductConcept = response["productConcept"],
                                LicenseCategory = response["licenseCat"],
                                ExportDetails = labelValue,
                                ExportLabels = labels
                            });
                        }
                        else
                        {
                            //Here it is not a Rejected submission thereby it is a first time submission
                            _DbContext.FormsDetails.Add(new FormsDetails
                            {
                                OperatorId = _Vars.OperatorId,
                                FormId = long.Parse(response["formId"]),
                                FormDetails = sFormdetails,
                                EntryId = NewEntryId,
                                Status = sStatus,
                                AppType = sAppType,
                                MasterEntryId = lMasterEntryId,
                                ProductName = response["productName"],
                                LicenseCategory = response["licenseCat"],
                                ProductConcept = response["productConcept"],
                                ExportDetails = labelValue,
                                ExportLabels = labels
                            });
                        }
                    }
                }
                // {
                //     var _FormsDetails = _DbContext.FormsDetails.Where(m => m.EntryId == EntryId).FirstOrDefault();

                //     //if it is a re-fill, update the old record and create a new record
                //     if (sStatus == "SUBMITTED" && reviewStat == "REJECTED")
                //     {
                //         appType = "RE-FILLED";
                //         _FormsDetails.AppType = appType;
                //         _FormsDetails.OldEntryId = NewEntryId;
                //         _DbContext.FormsDetails.Update(_FormsDetails);

                //         _DbContext.FormsDetails.Add(new FormsDetails
                //         {
                //             OperatorId = _Vars.OperatorId,
                //             FormId = long.Parse(response["formId"]),
                //             FormDetails = sFormdetails,
                //             EntryId = NewEntryId,
                //             Status = sStatus,
                //             AppType = sAppType,
                //             MasterEntryId = lMasterEntryId,
                //             ProductName = response["productName"],
                //             ProductConcept = response["productConcept"],
                //             ExportDetails = labelValue,
                //             ExportLabels = labels,
                //         });
                //     }
                //     else if (sStatus == "SAVED" || sStatus == "SUBMITTED" && reviewStat != "REJECTED")
                //     {
                //         _FormsDetails.EntryId = NewEntryId;
                //         _FormsDetails.FormDetails = sFormdetails;

                //         if (appType == "RE-FILLED")
                //             _FormsDetails.AppType = appType;
                //         else
                //             _FormsDetails.AppType = sAppType;

                //         _FormsDetails.MasterEntryId = lMasterEntryId;
                //         _FormsDetails.ProductName = response["productName"];
                //         _FormsDetails.ProductConcept = response["productConcept"];
                //         _FormsDetails.ExportDetails = labelValue;
                //         _FormsDetails.ExportLabels = labels;
                //         _FormsDetails.Status = sStatus;

                //         _DbContext.FormsDetails.Update(_FormsDetails);

                //         //_DbContext.FormsDetails.Add(new FormsDetails
                //         //{
                //         //    OperatorId = _Vars.OperatorId,
                //         //    FormId = long.Parse(response["formId"]),
                //         //    FormDetails = sFormdetails,
                //         //    EntryId = NewEntryId,
                //         //    Status = sStatus,
                //         //    AppType = sAppType,
                //         //    MasterEntryId = lMasterEntryId,
                //         //    ProductName = response["productName"],
                //         //    ProductConcept = response["productConcept"],
                //         //    ExportDetails = labelValue,
                //         //    ExportLabels = labels,
                //         //});
                //     }
                // }
                else
                {
                    //Here a new submission is created in the stead that it does not exist
                    _DbContext.FormsDetails.Add(new FormsDetails
                    {
                        OperatorId = _Vars.OperatorId,
                        FormId = long.Parse(response["formId"]),
                        FormDetails = sFormdetails,
                        EntryId = NewEntryId,
                        Status = sStatus,
                        AppType = sAppType,
                        MasterEntryId = lMasterEntryId,
                        ProductName = response["productName"],
                        LicenseCategory = response["licenseCat"],
                        ProductConcept = response["productConcept"],
                        ExportDetails = labelValue,
                        ExportLabels = labels,
                    });
                }
                //_DbContext.SaveChanges(true);

                FormsReview _FormsReview = new FormsReview();
                //Create/Update record for this submission in FormsReview table
                if (FormsReviewExists(EntryId))
                {
                    var _FrmsReview = _DbContext.FormsReview.Where(m => m.EntryId == EntryId).FirstOrDefault();

                    //if saved, only updates the record
                    if (sStatus == "SAVED")
                    {
                        //Updates the review status and changes the EntryId to the New one
                        _FrmsReview.EntryId = NewEntryId;
                        if (reviewStat == "REJECTED")
                            _FrmsReview.Status = reviewStat;
                        else
                            _FrmsReview.Status = Vars.ReviewStatus["PENDING"];

                        _DbContext.FormsReview.Update(_FrmsReview);
                    }
                    else
                    {
                        //If rejected then it has been submitted before
                        if (reviewStat == "REJECTED")
                        {
                            //Updates the old record
                            _FrmsReview.Status = reviewStat;
                            _DbContext.FormsReview.Update(_FrmsReview);

                            //Creates a new record
                            _FormsReview = new FormsReview
                            {
                                EntryId = NewEntryId,
                                Status = Vars.ReviewStatus["PENDING"]
                            };
                            _DbContext.FormsReview.Add(_FormsReview);
                        }
                        else
                        {
                            //First time submission - update EntryId
                            _FrmsReview.EntryId = NewEntryId;
                            _FrmsReview.Status = Vars.ReviewStatus["PENDING"];
                            _DbContext.FormsReview.Update(_FrmsReview);
                        }
                    }

                    // if (sStatus == "SUBMITTED" || sStatus == "SAVED" && reviewStat == "REJECTED")
                    // {
                    //     _FrmsReview.Status = reviewStat;
                    //     _DbContext.FormsReview.Update(_FrmsReview);

                    //     _FormsReview = new FormsReview
                    //     {
                    //         EntryId = NewEntryId,
                    //         Status = Vars.ReviewStatus["PENDING"]
                    //     };
                    //     _DbContext.FormsReview.Add(_FormsReview);
                    // }
                    // else if (sStatus == "SUBMITTED" || sStatus == "SAVED" && reviewStat != "REJECTED")
                    // {
                    //     _FrmsReview.EntryId = NewEntryId;

                    //     if (response["reviewstatus"] == "rejected")
                    //         _FrmsReview.Status = reviewStat;
                    //     else
                    //         _FrmsReview.Status = Vars.ReviewStatus["PENDING"];
                    //     _DbContext.FormsReview.Update(_FrmsReview);

                    //     //_FormsReview = new FormsReview
                    //     //{
                    //     //    EntryId = NewEntryId,
                    //     //    Status = Vars.ReviewStatus["PENDING"]
                    //     //};
                    //     //_DbContext.FormsReview.Add(_FormsReview);
                    // }
                }
                else
                {
                    //EntryId does not exist, creates a new record
                    _FormsReview = new FormsReview
                    {
                        EntryId = NewEntryId,
                        Status = Vars.ReviewStatus["PENDING"]
                    };
                    _DbContext.FormsReview.Add(_FormsReview);
                }


                /////////////--------------------------------////////////////////

                //if (sStatus != "SAVED")
                //{
                //    _DbContext.FormsEntry.RemoveRange(_DbContext.FormsEntry.Where(e => e.EntryId == EntryId).ToList());
                //}

                /*if (sStatus != "SAVED")
                {
                    var submission = _DbContext.Forms.FirstOrDefault(x => x.RecId == lFormId);
                    _notificationMgt.SendNotification(_Vars.OperatorId, Vars.MailType["TARIFF_REQUEST_NOTIFICATION"].ToString(), Vars.NotificationActionTypes["SUBMIT"].ToString(), submission.FormName);
                }*/
                TempData["message"] = "Your entry was captured successfully";
                if (sStatus == "SAVED" || sStatus == "SUBMITTED")
                {
                    _DbContext.FormsEntry.RemoveRange(_DbContext.FormsEntry.Where(e => e.EntryId == EntryId).ToList());
                }



                var entries = _DbContext.ChangeTracker.Entries();
                _DbContext.SaveChanges(true);

                if (sStatus != "SAVED")
                {
                    var isSuccess = await TaskHelper.InitiateTask(int.Parse(response["FormId"]), NewEntryId);
                }

                //save all changes
                _DbContext.SaveChanges(true);

                return RedirectToAction(nameof(Tariff));
            }

            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Tariff));
                //return RedirectToAction(nameof(Tariff));
            }
        }

        private string GetFieldLabel(string sFormDetails, string sKey, int index)
        {
            try
            {
                string sLabel = string.Empty;
                var document = new HtmlDocument();
                document.LoadHtml(sFormDetails);
                HtmlNodeCollection htmlnodes = document.DocumentNode.SelectNodes("//div[@class='f-field-group']");
                //foreach (HtmlNode node in htmlnodes)
                //{
                var Label = htmlnodes[index].SelectSingleNode(".//label[@for='" + sKey + "']");

                if (Label != null /*|| sLabel != ""*/)
                    return sLabel = Label.InnerText;
                else
                    return string.Empty;
                //}
                //return sLabel;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private string GetFieldLabel2(string sFormDetails, string sKey, int index = 0)
        {
            try
            {
                string sLabel = string.Empty;
                var document = new HtmlDocument();
                document.LoadHtml(sFormDetails);
                HtmlNodeCollection htmlnodes = document.DocumentNode.SelectNodes("//div[@class='f-field-group'] //label[@for='" + sKey + "']");
                //foreach (HtmlNode node in htmlnodes)
                //{
                if (index > 0)
                    sLabel = htmlnodes[index].SelectSingleNode(".//label[@for='" + sKey + "']").InnerText;
                else
                    sLabel = htmlnodes.FirstOrDefault().InnerText;

                if (sLabel != null || sLabel != "")
                    return sLabel;
                else
                    return sLabel;
                //}
                //return sLabel;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /*public IActionResult Tariff(IFormCollection response)
        {
            try
            {
                if (response.ContainsKey("EntryId"))
                {
                    var check = _DbContext.FormsEntry.FirstOrDefault(x => x.EntryId == long.Parse(response["EntryId"]));

                    if (check.Status == "SUBMITTED")
                    {
                        TempData["error"] = "Entry cannot be submitted twice";
                        return RedirectToAction(nameof(Tariff));
                    }
                    else
                    {
                        long id = long.Parse(response["EntryId"]);
                        if (_DbContext.FormsDetails.FirstOrDefault(m => m.EntryId == id) != null)
                        {
                            _DbContext.FormsDetails.Remove(_DbContext.FormsDetails.FirstOrDefault(m => m.EntryId == id));
                        }
                        _DbContext.FormsReview.Remove(_DbContext.FormsReview.FirstOrDefault(m => m.EntryId == id));
                        //_DbContext.TariffHistory.Remove(_DbContext.TariffHistory.FirstOrDefault(m => m.EntryIdMaster == id));
                        _DbContext.FormsEntry.RemoveRange(from c in _DbContext.FormsEntry where c.EntryId == long.Parse(response["EntryId"]) select c);
                        _DbContext.SaveChanges(true);
                    }
                }

                long entryID = DateTime.Now.ToFileTime();
                string status = response["Status"];
                List<FormsEntry> objEntries = new List<FormsEntry>();
                foreach (var itm in response)
                {
                    if (itm.Key.ToLower() != "entryid" && itm.Key.ToLower() != "formid" && itm.Key.ToLower() != "status" && itm.Key.ToLower() != "formdetails")
                    {
                        objEntries.Add(
                            new FormsEntry
                            {
                                FieldName = itm.Key,
                                Response = itm.Value.ToString(),
                                UserId = _Vars.UserId,
                                OperatorId = _Vars.OperatorId,
                                FormId = long.Parse(response["FormId"]),
                                DateSubmitted = DateTime.Now,
                                EntryId = entryID,
                                Status = status.ToUpper()
                            }
                        );
                    }
                }

                _DbContext.FormsEntry.AddRange(objEntries);
                _DbContext.FormsDetails.Add(new FormsDetails
                {
                    OperatorId = _Vars.OperatorId,
                    FormId = long.Parse(response["FormId"]),
                    FormDetails = response["FormDetails"],
                    EntryId = entryID
                });

                _DbContext.SaveChanges(true);

                //////////////-----Bodeniyi 2020-10-26--------//////////////////
                //Create record for this submission in FormsReview table
                FormsReview _FormsReview = new FormsReview
                {
                    EntryId = entryID,
                    Status = Vars.ReviewStatus["PENDING"]
                };
                _DbContext.FormsReview.Add(_FormsReview);
                _DbContext.SaveChangesAsync(true);
                /////////////--------------------------------////////////////////

                //////////////-----Bodeniyi 2020-10-16--------//////////////////
                //Create record for this submission in Tariff History table
                long lEntryIdSlave = 0;

                TariffHistory _TariffHistory = new TariffHistory
                {
                    EntryIdMaster = entryID,
                    EntryIdSlave = lEntryIdSlave
                };
                _DbContext.TariffHistory.Add(_TariffHistory);
                _DbContext.SaveChangesAsync(true);
                /////////////--------------------------------////////////////////

                if (status != "SAVED")
                {
                    var submission = _DbContext.Forms.FirstOrDefault(x => x.RecId == long.Parse(response["FormId"]));

                    _notificationMgt.SendNotification(_Vars.OperatorId, Vars.MailType["TARIFF_REQUEST_NOTIFICATION"].ToString(), Vars.NotificationActionTypes["SUBMIT"].ToString(), submission.FormName);
                }

                TempData["message"] = "Your entry was captured successfully";
                return RedirectToAction(nameof(Tariff));
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return BadRequest(ex);
            }
        }*/     //----removed by Bodeniyi 2020-11-16

        public JsonResult GetFormFields(long formId)
        {
            try
            {
                var objFields = _DbContext.Forms.Where(e => e.RecId == formId).ToList();
                //Hashtable fieldResp = new Hashtable();
                //foreach(var itm in objFields)
                //{
                //    fieldResp.Add(itm, itm.Response);
                //}
                var obj = objFields.FirstOrDefault().FormFields;
                return Json(new List<object> { new { data = obj } }[0]);
            }
            catch (Exception ex)
            {
                ViewBag.error = ex.Message + " |" + ex.InnerException;
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult Reset(IFormCollection frm)
        {
            try
            {
                _DbContext.FormsDetails.Remove(_DbContext.FormsDetails.FirstOrDefault(m => m.EntryId == long.Parse(frm["entry"])));
                _DbContext.SaveChanges();
                return Json("Message: " + "Data Reset was successful. You can now re-enter all details of the survey");
            }
            catch (Exception ex)
            {
                return Json("ERROR: " + ex.Message);
            }
        }

        public IActionResult Others()
        {
            ViewBag.operatorName = _Vars.OperatorName;
            ViewBag.operatorID = _Vars.OperatorId;

            ViewBag.forms = (from m in _DbContext.Forms select m).Where(m => m.Published && m.FormsType.ToLower() == "OTHER_SERVICE").ToList();

            var objForms = (from m in _DbContext.FormsOperators where m.OperatorId == _Vars.OperatorId select m).ToList();

            ViewBag.review = _DbContext.FormsReview.Where(e => objForms.Select(f => f.EntryId).Contains(e.EntryId)).ToList();

            return View(objForms);
        }

        //private bool FormsDetailsExists(long id)
        //{
        //    return _DbContext.FormsDetails.Any(e => e.EntryId == id);
        //}
        [HttpPost]
        public async Task<IActionResult> Others(IFormCollection response, string status, string formdetails, long EntryId, long FormId)
        {
            try
            {
                //if (EntryId == 0)
                //{
                //    if(response.ContainsKey("EntryId"))
                //        EntryId = long.Parse(response["EntryId"]);
                //}

                if (EntryId > 0)
                {
                    var checkEntr = _DbContext.FormsEntry.FirstOrDefault(x => x.EntryId == long.Parse(response["EntryId"]));
                    if (response["entryId"] != "" && checkEntr != null && checkEntr.Status == "SUBMITTED" && response["status"] == "SUBMITTED")
                    {
                        TempData["error"] = "Sorry Submission has already been made for this entry! FAILED";
                        return BadRequest();
                    }

                    _DbContext.FormsEntry.RemoveRange(_DbContext.FormsEntry.Where(m => m.EntryId == long.Parse(response["entryId"])));
                }
                long EntryID = DateTime.Now.ToFileTime();

                List<FormsEntry> objEntries = new List<FormsEntry>();
                List<FormsDetails> objFormDetails = new List<FormsDetails>();

                FormId = long.Parse(response["FormId"]);
                var sts = response["ReviewStatus"].ToString();

                if (sts.ToLower() == "rejected")
                {
                    sts = "RE-FILLED";
                }
                foreach (var itm in response)
                {
                    string Id = itm.Key.Replace("response", "");
                    string fieldId = Regex.Replace(Id, @"[^0-9a-zA-Z-]+", "");
                    string val = itm.Value.ToString();

                    if (itm.Key.ToLower() != "entryid" && itm.Key.ToLower() != "formid" && itm.Key.ToLower() != "reviewstatus" && itm.Key.ToLower() != "status" && itm.Key.ToLower() != "formdetails" && fieldId.ToLower() != "requestverificationtoken")
                    {
                        objEntries.Add(
                            new FormsEntry
                            {
                                FieldName = fieldId,
                                Response = val,
                                UserId = _Vars.UserId,
                                OperatorId = _Vars.OperatorId,
                                FormId = FormId,
                                DateSubmitted = DateTime.Now,
                                EntryId = EntryID,
                                Status = status.ToUpper()
                            }
                        );
                    }
                }

                if (response.Files != null)
                {
                    foreach (var file in response.Files.Where(w => w.FileName.ToLower() != "file"))
                    {
                        /*
                        string sAppContentPath = _webHostEnv.ContentRootPath;
                        string sStoragePath = Path.Combine(_webHostEnv.WebRootPath, "uploads");
                        _FrameworkCore._FileAccess _file = new _FrameworkCore._FileAccess(file, _HttpContext.HttpContext);
                        string sFileUrl = await _file.UploadFile(sAppContentPath, sStoragePath, file.FileName);
                        */
                        var uploadsFolderPath = Path.Combine(_webHostEnv.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolderPath))
                            Directory.CreateDirectory(uploadsFolderPath);
                        var Extension = Path.GetExtension(file.FileName);
                        var fileName = EntryID + Extension;
                        var filePath = Path.Combine(uploadsFolderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        objEntries.Add(
                                new FormsEntry
                                {
                                    FieldName = file.Name,
                                    Response = fileName,
                                    UserId = _Vars.UserId,
                                    OperatorId = _Vars.OperatorId,
                                    FormId = long.Parse(response["FormId"]),
                                    DateSubmitted = DateTime.Now,
                                    EntryId = EntryID,
                                    Status = status.ToUpper(),
                                    IsFile = true
                                }
                            );
                    }
                }
                _DbContext.FormsEntry.AddRange(objEntries);
                _DbContext.SaveChanges(true);

                var check = _DbContext.FormsDetails.Where(e => e.EntryId == EntryId);
                if (check != null && EntryId <= 0)
                {
                    _DbContext.FormsDetails.Add(new FormsDetails
                    {
                        OperatorId = _Vars.OperatorId,
                        FormId = long.Parse(response["FormId"]),
                        FormDetails = formdetails,
                        EntryId = EntryID,
                        Status = status
                    });
                    _DbContext.SaveChanges(true);
                }
                else
                {
                    _DbContext.FormsDetails.RemoveRange(check);
                    _DbContext.FormsDetails.Add(new FormsDetails
                    {
                        OperatorId = _Vars.OperatorId,
                        FormId = long.Parse(response["FormId"]),
                        FormDetails = formdetails,
                        EntryId = EntryID,
                        Status = status
                    });
                    _DbContext.SaveChanges(true);
                }

                //////////////-----Bodeniyi 2020-10-16--------//////////////////
                //Create record for this submission in FormsReview table
                FormsReview _FormsReview = new FormsReview
                {
                    EntryId = EntryID,
                    Status = Vars.ReviewStatus["PENDING"]
                };
                _DbContext.FormsReview.Add(_FormsReview);
                await _DbContext.SaveChangesAsync(true);
                /////////////--------------------------------////////////////////

                if (status.ToUpper() != "SAVED")
                {
                    var isSuccess = await TaskHelper.InitiateTask(int.Parse(response["FormId"]), EntryID);
                }

                if (status != "SAVED")
                {
                    var submission = _DbContext.Forms.FirstOrDefault(x => x.RecId == long.Parse(response["FormId"]));

                    // // _notificationMgt.SendNotification(_Vars.OperatorId, Vars.MailType["OTHER_SERVICE_NOTIFICATION"].ToString(), Vars.NotificationActionTypes["SUBMIT"].ToString(), submission.FormName);
                }

                TempData["message"] = "Your entry was captured successfully";
                return RedirectToAction(nameof(Others));
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Others));
            }
        }

        /// <summary>
        /// Using the operator identification, fetches the previously field Tariff requests made by the operator in question
        /// </summary>
        /// <param name="operatorId"></param>
        /// <returns>A Json of the all the Tariff Request forms previously filled</returns>
        public JsonResult ListPrevForms(string operatorId, string id)
        {
            try
            {
                var frmCat = _DbContext.Forms.FirstOrDefault(x => x.RecId == long.Parse(id)).FormsTypeCategory;
                var _TariffHist = _DbContext.TariffHistories.Where(e => e.OperatorId == operatorId && e.FormsTypeCategory.ToLower() == frmCat && e.Status == "APPROVED");
                if (_TariffHist != null)
                {
                    var results = _TariffHist.Where(w => w.FormDetails != "N/A").Select(e => new
                    {
                        value = e.EntryIdMaster.ToString(),
                        text = e.ProductName + " (" + e.AppType + ") - " + e.DateCreated
                    }).ToList();

                    return new JsonResult(results);
                }
                return new JsonResult("Error");
            }
            catch (Exception e)
            {
                ViewBag.err = e + "Could not fetch previously filled forms";
                return new JsonResult("Error");
            }
        }

        //[HttpPost]
        //public JsonResult SDetails(IFormCollection frm)
        //{
        //    try
        //    {
        //        string ErrMsg = string.Empty;
        //        string sCondition = string.Empty;

        //        if (frm != null)
        //        {

        //            var objEntry = (from m in _DbContext.FormsEntry
        //                            select m).Where(m => m.EntryId == long.Parse(frm["entry"])).ToList();

        //            var objPrev = _DbContext.FormsDetails.FirstOrDefault(m => m.EntryId == long.Parse(frm["entry"]));
        //            string _temp = objPrev == null ? "" : objPrev.FormDetails;

        //            return Json(new List<object> { new { data = objEntry, template = _temp } }[0]);
        //        }
        //        else
        //        {
        //            return Json("");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.error = ex.Message;
        //        return Json(ex.Message);
        //    }
        //}

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

    }
}