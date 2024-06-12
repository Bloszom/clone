using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using pceaLibrary;
using pcea.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Data;
using System.Text;
using System.Globalization;
using Newtonsoft.Json;
using pcea.Helpers;

namespace pcea.Controllers
{
    public class ReportController : Controller
    {
        private readonly PceaDbContext _DbContext;
        private readonly IHttpContextAccessor _HttpContext;
        private IConfiguration _Configuration;
        Vars _Vars;
        Report _Report;
        ReportHelper _ReportHelper;

        public ReportController(PceaDbContext context, IHttpContextAccessor httpContext, IConfiguration configuration)
        {
            _DbContext = context;
            _HttpContext = httpContext;
            _Configuration = configuration;
            _Vars = new Vars(_HttpContext.HttpContext);
            _Report = new Report();
            _ReportHelper = new ReportHelper(_DbContext);
        }
        //doit check the workflow again just to be sure
        public IActionResult Index(int? id)
        {
            //IQueryable objForm = null;
            var objForm = from m in _DbContext.Reports orderby m.RecId descending select m;
            string err = "";
            try
            {
                if (id > 0)
                {
                    var objRpt = _DbContext.Reports.Find(id);

                    //executes query to build the following
                    Dictionary<string, object> lst = new Dictionary<string, object>();
                    var objTable = new EntryList(id.ToString());
                    if (string.IsNullOrEmpty(objTable.ErrorMsg) && objTable.FormFields != null)
                    {
                        if (objTable.ReportIsLoaded)
                        {
                            string th = "<thead><tr><th>Labels</th>", tb = "", hd = "";
                            List<Dictionary<string, string>> rws = new List<Dictionary<string, string>>();
                            List<string> headers = new List<string>();
                            
                            //check if the column selected in the template is the operator names
                            if(objRpt.ReportColumnType != "operator" && objRpt.ReportColumnType != "operatorType")
                            {
                                foreach (Dictionary<string, string> entry in objTable.FormEntry)
                                {
                                    var test = entry["entity"];
                                    if (!objTable.FormFields.Keys.Any(entity => entity != entry["entity"])) objTable.FormFields.Add(entry["entity"], entry["entity"]);
                                    int i = 0;
                                    foreach (var rw in objTable.RowFields.ToHashSet())
                                    {
                                        var row = string.Empty;
                                        if (rw.Contains("]"))
                                            row = rw.Substring(rw.IndexOf("]") + 1).Trim();
                                        else
                                            row = rw;
                                        hd = "";
                                        Dictionary<string, string> _rw = new Dictionary<string, string>();
                                        int index = 0;
                                        if(i <= 0)
                                        {
                                            _rw.Add("rw", "<th>" + row.Replace(" ", "_") + "</th>");
                                            i++;
                                        }
                                        else
                                            _rw.Add("rw", "<th>" + row + "</th>");

                                        foreach (var hdn in objTable.FormFields)
                                        {
                                            var value = entry.FirstOrDefault(e => e.Key.Contains(hdn.Value) && e.Key.Contains(row));

                                            if (!string.IsNullOrEmpty(value.Key))
                                            {
                                                //if(objTable.RowFields.Contains(value.Key.Substring(0, value.Key.IndexOf("-")).Trim()))
                                                _rw.Add(hdn.Value == "" ? index.ToString() : hdn.Value, "<td>" + value.Value + "</td>");
                                                hd += "<th>" + hdn.Value + "</th>";
                                                headers.Add(hdn.Value == "" ? hdn.Key : hdn.Value);
                                            }

                                            else
                                                _rw.Add(hdn.Value == "" ? index.ToString() : hdn.Value, "<td>0</td>");

                                            index++;
                                            var check = hd.Replace("<th", "");
                                            string[] _check = check.Replace("</th", "").Split();
                                            if (!headers.Contains(hdn.Value))
                                                hd += "<th>" + hdn.Value + "</th>";
                                        }
                                        rws.Add(_rw);
                                    }
                                    //foreach (var clm in entry.Select(x => new { key = x.Key, value = x.Value }).ToList())
                                    //{
                                    //    var rw = rws.Find(x => x["rw"] == "<th>" + clm.key + "</th>");
                                    //    if (rw != null) rw[entry["entity"]] = "<td>" + clm.value + "</td>";

                                    //}
                                }
                                th += hd + "</tr></thead>";

                                tb += "<tbody>";
                                foreach (var rw in rws)
                                {
                                    tb += "<tr>" + rw["rw"];
                                    foreach (var fld in objTable.FormFields)
                                    {
                                        if (rw.Keys.Contains(fld.Value))
                                            tb += rw[fld.Value];
                                        else
                                            tb += "";
                                    }
                                    tb += "</tr>";
                                }
                                tb += "</tbody>";
                            }
                            else
                            {
                                Dictionary<string, string> _rw = new Dictionary<string, string>();
                                Dictionary<string,Dictionary<string, string>> hds = new Dictionary<string, Dictionary<string, string>>();
                                //group the values
                                foreach (Dictionary<string, string> entry in objTable.FormEntry)
                                {
                                    var hdn = entry["entity"];
                                    
                                    hd += "<th>" + hdn + "</th>";
                                    hds.Add(hdn, entry);
                                    

                                }
                                th += hd + "</tr></thead>";

                                tb += "<tbody>";
                                foreach (var _hd in hds)
                                {
                                    var values = _hd.Value.Where(e => !e.Key.Contains("entity")).ToList();
                                    foreach (var value in values)
                                    {
                                        var key = value.Key.Substring(value.Key.IndexOf("-") + 1).Trim();
                                        var val = value.Value;
                                        if (!_rw.ContainsKey(key))
                                            _rw.Add(key, val);
                                        else
                                        {
                                            var itm = _rw.FirstOrDefault(e => e.Key == key);
                                            var _key = itm.Key;
                                            var _value = itm.Value;
                                            _rw.Remove(_key);

                                            _rw.Add(_key, _value + "-" + val);
                                        }
                                    }
                                }

                                int i = 0;
                                foreach (var rw in _rw)
                                {
                                    if (i <= 0)
                                        tb += "<tr>";
                                    var valArray = rw.Value.Split("-");
                                    tb += "<td>" + rw.Key + "</td>";
                                    foreach (var item in valArray)
                                    {
                                        tb += "<td>" + item + "</td>";
                                    }
                                    //+ "<td>" + rw.Value.Substring(0, rw.Value.IndexOf("-") - 1) + "</td>" + "<td>" + rw.Value.Substring(rw.Value.IndexOf("-") + 1) + "</td>";

                                    tb += "</tr>";
                                }
                                //foreach (var rw in _rw)
                                //{
                                //    tb += "<tr>" + rw["rw"];
                                //    foreach (var fld in objTable.FormFields)
                                //    {
                                //        if (rw.Keys.Contains(fld.Value))
                                //            tb += rw[fld.Value];
                                //        else
                                //            tb += "";
                                //    }
                                //    tb += "</tr>";
                                //}
                                tb += "</tbody>";
                            }



                            //foreach (Dictionary<string, string> td in objTable.FormEntry)
                            //{
                            //    foreach (var clm in td.Select(x => new { key = x.Key, value = x.Value }).ToList())
                            //    {
                            //        var rw = rws.Find(x => x["rw"] == "<th>" + clm.key + "</th>");
                            //        if (rw != null) rw[td["entity"]] = "<td>" + clm.value + "</td>";
                            //        if (!objTable.FormFields.Keys.Contains(td["entity"])) objTable.FormFields.Add(td["entity"], td["entity"]);
                            //    }
                            //}

                            

                            //var tabl = "<table id=\"rptTable\" class=\"table table-stripped  table-bordered\">" + th + tb + "</table";
                            var tabl = th + tb;

                            lst.Add("table", tabl);
                            lst.Add("type", objRpt.ChartType);
                            lst.Add("title", objRpt.ReportName);
                            lst.Add("mDate", objRpt.DateMigrated);
                            lst.Add("category", objRpt.Category);
                            lst.Add("color", objRpt.ColumnColor);

                            //var mdl = lst.Select(x => new { key = x.Key, value = x.Value }).ToList()
                            ViewBag.report = lst;
                        }
                    }
                    else
                    {
                        err += "No data has been generated for this report. " + objTable.ErrorMsg;
                    }
                }
                
                //objForm = from m in _DbContext.Reports orderby m.RecId descending select m;
            }
            catch (Exception ex)
            {
                err += ex.Message;
            }
            if (!string.IsNullOrEmpty(err)) ViewBag.error = err.Trim();
            ViewBag.categoryList = GetCategory().ToList<ReportsCategory>();
            ViewBag.chartList = GetChart();

            return View(objForm);
        }

         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCategory(IFormCollection catg)
        {
            try
            {
                if (catg != null)
                {
                    var rpt = new ReportsCategory()
                    {
                        Title = catg["Title"],
                        Description = catg["Description"]
                    };
                    if (!string.IsNullOrEmpty(catg["RecId"]))
                    {
                        rpt.RecId = int.Parse(catg["RecId"]);
                        _DbContext.ReportsCategory.Update(rpt);
                    }
                    else
                    {
                        _DbContext.ReportsCategory.Add(rpt);
                    }
                    _DbContext.SaveChanges(true);
                    TempData["message"] = "Report Category Updated Successfully";
                }
                else
                {
                    TempData["error"] = "Form value not provided. Update failed";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] += ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        IEnumerable<ReportsCategory> GetCategory()
        {
            return (from m in _DbContext.ReportsCategory select m);
        }

        SelectList GetChart()
        {
            return new SelectList(
                new List<object> {
                    new { value = "area", text = "Area Chart" }, new { value = "bar", text = "Bar Chart" },
                    new { value = "line", text = "Line Graph" }, new { value = "pie", text = "Pie Chart" }
                }, "value", "text", "bar");
        }

        //SelectList GetSurveyList()
        //{
        //    return new SelectList(
        //        (from c in _DbContext.Forms select c).Where(m => m.Published == true && m.FormsType.ToLower() == "survey").ToList(),
        //        "RecId", "FormName");
        //}
        //SelectList GetTariffList()
        //{
        //    return new SelectList(
        //        (from c in _DbContext.Forms select c).Where(m => m.Published == true && m.FormsType.ToLower() == "tariff").ToList(),
        //        "RecId", "FormName");
        //}
        enum FormsType
        {
            SURVEY, TARIFF, OTHER_SERVICE
        }
        SelectList GetTypeList(FormsType fType)
        {
            string _fType = fType == FormsType.SURVEY ? "operator_type" : (fType == FormsType.TARIFF ? "tariff_type" : "other_service");
            return new SelectList(
                (from c in _DbContext.Forms select c).Where(m => m.Published == true && m.FormsType.ToLower() == _fType).ToList(), 
                "RecId", "FormName");
        }

        SelectList GetAggregator()
        {
            return new SelectList(
                new List<object> {
                    new { value = "sum", text = "Sum" }, new { value = "count", text = "Count" }
                }, "value", "text", "sum");
        }

        public async Task<IActionResult> Analysis(int? id)
        {
            try
            {
                string formId = string.Empty;
                Reports objForm = null;
                SelectList formlist = GetTypeList(FormsType.SURVEY);
                
                if (id != null)
                {

                    objForm = await _DbContext.Reports.FindAsync(id == null ? 0 : id);
                    if (id != null && objForm == null)
                    {

                        return NotFound();
                    }
                    formId = objForm.AnalysisFields.Substring(0, objForm.AnalysisFields.IndexOf(":"));
                    //ViewBag.id = id;


                    var selected = formlist.FirstOrDefault(f => f.Value == formId.ToString());
                    selected.Selected = true;
                }

                ViewBag.surveyList = formlist;

                var catg = GetCategory();
                ViewBag.chartList = GetChart();
                ViewBag.category = catg;
                ViewBag.aggregator = GetAggregator();
                ViewBag.categoryList = catg.ToList<ReportsCategory>();

                //ViewBag.operators = string.Join(",", _DbContext.UserProfile.Where(m => m.UserType == "PUBLIC").Select(c => c.OrganizationId).ToList());
                if(!string.IsNullOrEmpty(formId))
                    ViewBag.operators = string.Join(",", _DbContext.FormsAndEntry.Where(m => m.FormsType == Vars.FormTypes.OPERATOR_TYPE.ToString() && m.RecId == long.Parse(formId)).Select(c => c.OrganizationName).Distinct().ToList());

                return View(objForm);
            }
            catch (Exception ex)
            {
                TempData.Add("error", ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Analysis(Reports strPost)
        {
            try
            {
                //save analysis

                if (strPost.RecId > 0)
                {
                    strPost.DateUpdated = DateTime.Now;
                    _DbContext.Entry(strPost).State = EntityState.Modified;
                    _DbContext.Entry(strPost).Property("DateCreated").IsModified = false;
                    _DbContext.Entry(strPost).Property("DateMigrated").IsModified = false;
                    //_DbContext.Reports.Update(strPost);
                }
                else
                {
                    strPost.CreatedBy = _Vars.UserId;
                    _DbContext.Reports.Add(strPost);
                }

                _DbContext.SaveChanges(true);

                TempData["message"] = "Report analysis and update was successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                TempData["error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction(nameof(Analysis), strPost.RecId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MigrateData(IFormCollection frm)
        {
            string msg = "", err = "";
            try
            {
                var rpt = _DbContext.Reports.Find(int.Parse(frm["Report"]));
                if (rpt != null)
                {
                    //DateTime rptDate = DateTime.Now;
                    DateTime rptDateFm = string.IsNullOrEmpty(frm["migrateDateFrom"]) ? DateTime.Today.Subtract(new TimeSpan(60 * 60 * 24)) : DateTime.ParseExact(frm["migrateDateFrom"], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    DateTime rptDateTo = string.IsNullOrEmpty(frm["migrateDateTo"]) ? DateTime.Now : DateTime.ParseExact(frm["migrateDateTo"], "yyyy-MM-dd", CultureInfo.InvariantCulture);

                    //select all data of the survey >>> [AggregateFields] survery form id
                    string _form = rpt.AnalysisFields.Substring(0, rpt.AnalysisFields.IndexOf(':'));
                    dynamic _field = JObject.Parse(rpt.AnalysisFields.Substring(rpt.AnalysisFields.IndexOf(':') + 1).Trim()) as JObject;//array of fields

                    //extract field id
                    Dictionary<string, string> fields = new Dictionary<string, string>();
                    foreach (string txt in _field)
                        fields.Add(txt.Substring(0, txt.IndexOf("»")),txt.Substring(txt.IndexOf("»") + 1));
                   //Dictionary<string,string> fieldName = new Dictionary<string, string>();
                   // foreach (string item in _field)
                   // {
                   //     fieldName.Add(item.Substring(0, item.IndexOf("»")));
                   // }

                    //check if entry has previously been entered for this
                    if (_DbContext.Analysis.Where(c => c.ReportId == rpt.RecId && c.ReportDate >= rptDateFm && c.ReportDate <= rptDateTo).ToList().Count > 0)
                    {
                        err += "Report had previously been generated for the selected date. Kindly enter a new date for this migration.";
                    }
                    else
                    {
                        //contruct where clause picking only the records of listed fields
                        var c = fields.Select(dl => dl.Value.Trim()).ToArray();

                        string where = "(FormId == " + _form + ") and (DateSubmitted >= \"" + rptDateFm + "\" and DateSubmitted <= \"" + rptDateTo + "\")" +
                            (fields.Count > 0 ? " and (FieldName == \"" + string.Join("\" or FieldName == \"", c) + "\")" : "");


                        string fieldNamesQuery = fields.Count > 0 ? $"and (FieldName = '{string.Join("' or FieldName = '", c)}')" : "";


                        string Where = $"(FormId = {_form}) and (DateSubmitted >= '{rptDateFm.ToString("yyyy-MM-dd")}' and DateSubmitted <= '{rptDateTo.ToString("yyyy-MM-dd")}') {fieldNamesQuery}";

                        //var frmEntry = _DbContext.FormsEntry
                        //    .Where(where)
                        //    .GroupBy(g => new { g.FormId, g.OperatorId })
                        //    .Select(gr => new
                        //    {
                        //        gr.Key.FormId,
                        //        gr.Key.OperatorId,
                        //        OperatorType = "",
                        //        ReportDate = DateTime.Parse(rptDateTo), //gr.Max(x => x.DateSubmitted),
                        //        Value = rpt.AnalysisAggregator.ToLower().Trim() == "sum" ? gr.Sum(x => Convert.ToDecimal(string.IsNullOrEmpty(x.Response) ? "0" : x.Response)) : gr.Count()
                        //    }).ToList();
                        string[] operatorId = rpt.ReportColumnType == "operator" ? rpt.ReportColumnName.Split(",") : rpt.ReportRowType == "operator" ? rpt.ReportRowName.Split(",") : new string[233];

                        //var dateTo = DateTime.ParseExact(rptDateTo, "d", null);
                        //var dateFrom = DateTime.ParseExact(rptDateFm, "d", null);
                        var query = $"select * from [dbo].[FormsEntry] where {Where}";
                        var frment = _DbContext.FormsEntry.FromSqlRaw(query);
                        var frmEntry = _DbContext.FormsEntry.ToList().Where(e => e.FormId == long.Parse(_form) && e.DateSubmitted >= rptDateFm && e.DateSubmitted <= rptDateTo && fields.Values.Contains(e.FieldName))
                            .GroupBy(g => new { g.FormId, g.OperatorId, g.FrmYear })
                            .Select(sl =>
                                new
                                {
                                    sl.Key.FrmYear,
                                    sl.Key.FormId,
                                    sl.Key.OperatorId,
                                    OperatorType = "",
                                    ReportDate = rptDateTo,
                                    Value = sl.FirstOrDefault(d => fields.Values.Contains(d.FieldName)).Response,
                                    Year = "",
                                    
                                    //Value = rpt.AnalysisAggregator.ToLower().Trim() == "sum" ? sl.Sum(x => Convert.ToDecimal(string.IsNullOrEmpty(x.Response) ? "0" : x.Response)) : sl.Count(),
                                    Field = sl.FirstOrDefault(d => fields.Values.Contains(d.FieldName)).FieldName
                                }
                            ).ToList();

                        //var checkDynamic = _DbContext.FormsEntry.Where(e => frment.Any(d => d.FieldLabel == e.FieldLabel)).ToList();

                        //if(checkDynamic.Count > 0)
                        //{
                        //    var newcheck = checkDynamic.GroupBy(g => new { g.Response, g.FieldLabel, g.OperatorId }).Select(sl => new
                        //    {
                        //        sl.Key.FieldLabel,
                        //        sl.Key.OperatorId,
                        //        value = sl.Where(e => e.FieldLabel == sl.Key.FieldLabel && e.OperatorId == sl.Key.OperatorId).Select(d => d.Response).Sum(c => Convert.ToDouble(c)),
                        //    }).ToList();
                        //    newcheck.ForEach((param) =>
                        //    {
                        //        frment.FirstOrDefault(e => e.FieldLabel == param.FieldLabel && e.OperatorId == param.OperatorId).Response = newcheck.Where(g => g.OperatorId == param.OperatorId && g.FieldLabel == param.FieldLabel).ToList().Sum(s => s.value).ToString();
                        //    });
                        //}

                        //var kypair = new List<KeyPair>();

                        //checkDynamic.ForEach((param) =>
                        //{
                        //    kypair.Add(new KeyPair { Name = param.FieldLabel, Value = param.Response });
                        //});

                        //var year = _DbContext.Forms.FirstOrDefault(e => e.RecId == long.Parse(_form)).FormYear;
                        if (frmEntry.Count > 0)
                        {
                            var year = frment.FirstOrDefault().FrmYear;
                            List<string> years = new List<string>() {
                            year, (int.Parse(year) - 1).ToString()
                            };

                            var entries = frment.ToList().GroupBy(g => new { g.OperatorId, g.ValueYear, g.Response, g.FieldLabel, g.UserId }).Select(sl => new
                            {
                                OrgName = _DbContext.UserProfile.FirstOrDefault(e => e.UserId == sl.Key.UserId).OrganizationName,
                                sl.Key.Response,
                                sl.Key.ValueYear,
                                PrevYear = rpt.AnalysisAggregator.ToLower().Trim() == "sum" ? sl.Where(w => w.ValueYear == years.FirstOrDefault() || string.IsNullOrEmpty(w.ValueYear)).ToList().Sum(x => Convert.ToDecimal(string.IsNullOrEmpty(x.Response) ? "0" : x.Response)) : sl.Count()
                            });
                            var entry = _DbContext.FormsEntry.ToList().Where(e => e.FormId == long.Parse(_form) && e.DateSubmitted >= rptDateFm && e.DateSubmitted <= rptDateTo && fields.Values.Contains(e.FieldName))
                                .GroupBy(g => new { g.OperatorId, g.ValueYear, g.Response, g.FieldLabel, g.UserId })
                                .Select(sle => new
                                {
                                    sle.Key.FieldLabel,
                                    OrgName = _DbContext.UserProfile.FirstOrDefault(e => e.UserId == sle.Key.UserId).OrganizationName,
                                    sle.FirstOrDefault().Response,
                                    sle.FirstOrDefault().FieldName,
                                    sle.Key.OperatorId,
                                    sle.Key.ValueYear,
                                    ReportDate = rptDateTo,
                                    sle.FirstOrDefault().FormId,
                                    PrevYear = rpt.AnalysisAggregator.ToLower().Trim() == "sum" ? sle.Where(w => w.ValueYear == years.FirstOrDefault()).ToList().Sum(x => Convert.ToDecimal(string.IsNullOrEmpty(x.Response) ? "0" : x.Response)) : sle.Count(),
                                    Year = "",
                                    Label = fields.FirstOrDefault(e => e.Value.Contains(sle.FirstOrDefault().FieldName)).Key,
                                    OperatorType = ""
                                }).ToList();
                            
                            Dictionary<string, decimal> sumData = new Dictionary<string, decimal>();

                            //check for the year value
                            //move on
                            if(entries.All(e => string.IsNullOrEmpty(e.ValueYear)))
                            {
                                int k = 0;
                                foreach(var itm in operatorId.ToHashSet())
                                {
                                    //remove the year hardcoded
                                    var keyPrev = (string.IsNullOrEmpty(itm) ? itm + k : itm) + " - Curr - " + frment.FirstOrDefault().FrmYear;

                                    if (!sumData.ContainsKey(keyPrev))
                                    {
                                        sumData.Add(keyPrev, entries.Where(w => w.OrgName == itm).Sum(x => Convert.ToDecimal(string.IsNullOrEmpty(x.Response) ? "0" : x.Response)));
                                    }
                                    k++;
                                }
                            }

                            if (rpt.AnalysisAggregator.ToLower().Trim() == "sum" && !entries.All(e => string.IsNullOrEmpty(e.ValueYear)))
                            {
                                int j = 0;
                                foreach (var item in operatorId.ToHashSet())
                                {
                                    int i = 0;
                                    foreach (var yr in years)
                                    {
                                        if (i <= 0)
                                        {
                                            var keyPrev = (string.IsNullOrEmpty(item) ? item + j : item) + " - Prev - " + yr;

                                            if (!sumData.ContainsKey(keyPrev))
                                            {
                                                sumData.Add(keyPrev, entries.Where(e => e.OrgName == item && e.ValueYear == yr).Sum(x => Convert.ToDecimal(string.IsNullOrEmpty(x.Response) ? "0" : x.Response)));
                                            }
                                            i++;
                                        }
                                        else
                                        {
                                            var keyPrev = (string.IsNullOrEmpty(item) ? item + j : item) + " - Curr - " + yr;

                                            if (!sumData.ContainsKey(keyPrev))
                                            {
                                                sumData.Add((string.IsNullOrEmpty(item) ? item + j : item) + " - Curr - " + yr, entries.Where(e => e.OrgName == item && e.ValueYear == yr && !string.IsNullOrEmpty(e.ValueYear)).Sum(x => Convert.ToDecimal(string.IsNullOrEmpty(x.Response) ? "0" : x.Response)));
                                            }
                                        }
                                    }

                                    j++;
                                }
                            }

                            List<Analysis> objAnalysis = new List<Analysis>();

                                foreach (var itm in sumData)
                                {
                                    var _operator = itm.Key.Substring(0, itm.Key.IndexOf("-"));
                                    var _year = itm.Key.Substring(itm.Key.LastIndexOf("-") + 1);

                                    objAnalysis.Add(new Analysis
                                    {
                                        ReportId = rpt.RecId,
                                        Operator = _operator,
                                        OperatorType = "",
                                        ReportValue = itm.Value,
                                        ReportDate = rptDateTo,
                                        Analyst = _Vars.UserId,
                                        EntryDate = DateTime.Now,
                                        Year = _year.Trim()
                                    });
                                }
                                _DbContext.Analysis.AddRange(objAnalysis);
                                _DbContext.SaveChanges(true);
                                msg += "Report migration was successfully";
                        }
                        else
                        {
                            err += "Report upload failed! There are no form submissions yet for the selected form...";
                        }
                    }
                }
                else
                {
                    err += "Invalid Report. Update failed";
                }
            }
            catch (Exception ex)
            {
                err += ex.Message;
            }

            if (!string.IsNullOrEmpty(msg)) TempData.Add("message", msg);
            if (!string.IsNullOrEmpty(err)) TempData.Add("error", err);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public JsonResult ListFormFields(IFormCollection frm)
        {
            try
            {
                if (frm != null)
                {
                    var entries = (from c in _DbContext.FormsAndEntry where c.RecId == long.Parse(frm["entry"]) select c).Distinct().ToArray().ToList();
                    var objOperators = string.Join(",", entries.Select(sl => sl.OrganizationName).ToList());

                    var formyear = _DbContext.Forms.FirstOrDefault(f => f.RecId == long.Parse(frm["entry"])).FormYear;

                    var frmYrs = $"{formyear}";

                    for(int i = 1; i <= 2; i++)
                    {
                        var frmval = int.Parse(formyear);
                        formyear = (frmval - 1).ToString();
                        frmYrs += $",{formyear}";
                    }
                    /*
                     //order content
                    var orderedFields = objEntry.FormFields.ToList();
                    orderedFields.Sort((x, y) => x.Key.CompareTo(y.Key));
                     */
                    EntryList objEntry = new EntryList(long.Parse(frm["entry"]), true);
                    return Json(new List<object> { new { fields = objEntry.FormFields, operators = objOperators, years = frmYrs} }[0]);
                }
                else
                {
                    return Json("error");
                }
            }
            catch (Exception ex)
            {
                ViewBag.error = ex.Message;
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult ListLabels(IFormCollection frm)
        {
            try
            {
                if(frm != null)
                {
                    var data = _DbContext.FormsDetails.Where(x => x.EntryId == long.Parse(frm["entry"]) ).FirstOrDefault();

                    return Json(new List<object> { new { Labels = data.ExportLabels} }[0]);
                }
                else
                {
                    return Json("error");
                }
            }
            catch (Exception ex)
            {
                ViewBag.error = ex.Message;
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult ExportLabelValue( IFormCollection frm)
        {
            try
            {
                if (frm != null)
                {
                    var data = _DbContext.FormsDetails.Where(x => x.EntryId == long.Parse(frm["entry"])).FirstOrDefault();

                    return Json(new List<object> { new { LabelValue = data.ExportDetails } }[0]);
                }
                else
                {
                    return Json("error");
                }
            }
            catch (Exception ex)
            {
                ViewBag.error = ex.Message;
                return Json(ex.Message);
            }
        }

        [HttpGet]
        public string ExportTariffData(long lEntryId)
        {
            try
            {
                //create data table
                DataTable dt = new DataTable();
                DataTable dtCount = new DataTable();
                dt.Clear(); dtCount.Clear();

                var _Entries = _DbContext.FormsEntry.Where(m => m.EntryId == lEntryId).OrderBy(m => m.RecId);

                //loop thru entries and add column labels
                foreach (FormsEntry _Item in _Entries)
                {
                    bool bColumnAdded = false;
                    string sFieldLabel = _Item.FieldLabel;
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        string sColumnName = dt.Columns[i].ColumnName.ToString();
                        if (sColumnName == sFieldLabel) 
                        {
                            bColumnAdded = true;
                            break;
                        }
                    }
                    if(bColumnAdded == false)
                    {
                        dt.Columns.Add(sFieldLabel);
                        dtCount.Columns.Add(sFieldLabel);
                    }
                }

                //count number of rows for each column
                var _ItemCount = _DbContext.FormsEntryDTO.
                    FromSqlRaw("SELECT [FieldLabel], COUNT(*) AS [RecordCount] FROM [dbo].[FormsEntry] WHERE [EntryId] = " + 
                    lEntryId + " GROUP BY [FieldLabel] ORDER BY RecordCount DESC ").ToList();
                int iMaxRow = _ItemCount.FirstOrDefault().RecordCount;
                DataRow dr = dtCount.NewRow();
                foreach (FormsEntryDTO _Count in _ItemCount)
                {
                    dr[_Count.FieldLabel] = _Count.RecordCount;
                }

                //create max rows in datatable
                dr = null;
                for (int i = 0; i < iMaxRow; i++)
                {
                    dr = dt.NewRow();
                    dt.Rows.Add(dr);
                }

                //loop thru and add data value under each column name
                foreach (FormsEntry _Item in _Entries)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string sColumnValue = dt.Rows[i][_Item.FieldLabel].ToString();
                        if (string.IsNullOrEmpty(sColumnValue))
                        {
                            dt.Rows[i][_Item.FieldLabel] = _Item.Response;
                        }
                    }
                }

                string v = JsonConvert.SerializeObject(dt);
                return v;

            }
            catch (Exception ex)
            {
                string sMessage = ex.Message;
                return null;
            }
        }

        [HttpPost]
        public JsonResult LoadReport(IFormCollection frm)
        {
            try
            {
                if (frm != null)
                {
                    var objRpt = _DbContext.Reports.Find(int.Parse(frm["report"]));

                    //executes query to build the following
                    Dictionary<string, object> lst = new Dictionary<string, object>();
                    var objTable = new EntryList(frm["report"]);

                    //construct table,
                    //build chart
                    //and load: title, date
                    if (objTable.ReportIsLoaded)
                    {
                        string th = "<thead><tr><th></th>", tb = "", hd = "";

                        List<Dictionary<string, string>> rws = new List<Dictionary<string, string>>();
                        foreach (var rw in objTable.RowFields)
                        {
                            hd = "";
                            Dictionary<string, string> _rw = new Dictionary<string, string>();
                            _rw.Add("rw", "<th>" + rw + "</th>");
                            foreach (var hdn in objTable.FormFields)
                            {
                                _rw.Add(hdn.Value, "<td>0</td>");
                                hd += "<th>" + hdn.Value + "</th>";
                            }
                            rws.Add(_rw);
                        }
                        th += hd + "</tr></thead>";
                        foreach (Dictionary<string, string> td in objTable.FormEntry)
                        {
                            foreach (var clm in td.Select(x => new { key = x.Key, value = x.Value }).ToList())
                            {
                                var rw = rws.Find(x => x["rw"] == "<th>" + clm.key + "</th>");
                                if (rw != null) rw[td["entity"]] = "<td>" + clm.value + "</td>";
                            }
                        }

                        tb += "<tbody>";
                        foreach (var rw in rws)
                        {
                            tb += "<tr>" + rw["rw"];
                            foreach (var fld in objTable.FormFields)
                            {
                                tb += rw[fld.Value];
                            }
                            tb += "</tr>";
                        }
                        tb += "</tbody>";

                        var tabl = "<table id=\"rptTable\" class=\"table table-stripped  table-bordered\">" + th + tb + "</table";
                        //lst.Add(new { table = tabl, chart = cht, title = titl, mDate = lastMigrationdate });

                        lst.Add("table", tabl);
                        lst.Add("type", objRpt.ChartType);
                        lst.Add("title", objRpt.ReportName);
                        lst.Add("mDate", objRpt.DateMigrated);
                    }
                    else
                    {
                        lst.Add("error", objTable.ErrorMsg);
                    }

                    return Json(lst);
                }
                else
                {
                    return Json(new { error = "Invalid report" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = "Ctrl(): " + ex.Message });
            }
        }

        public IActionResult Reanalyze(int id)
        {
            try
            {
                _DbContext.Analysis.RemoveRange(_DbContext.Analysis.Where(m => m.ReportId == id));
                _DbContext.SaveChanges();

                TempData["message"] = "Analyzed data was refreshed successfully";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Delete(int id)
        {
            try
            {
                _DbContext.Reports.Remove(_DbContext.Reports.FirstOrDefault(m => m.RecId == id));
                _DbContext.Analysis.RemoveRange(_DbContext.Analysis.Where(m => m.ReportId == id));
                _DbContext.SaveChanges();

                TempData["message"] = "Report deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Export()
        {
            try
            {
                ViewBag.SurveyList = GetTypeList(FormsType.SURVEY); 
            }
            catch (Exception ex)
            {
                ViewBag.error += ex.Message;
            }
            return View();
        }
        //public IActionResult TariffExport()
        //{
        //    try
        //    {
        //        ViewBag.surveyList = GetTariffRequestList();

        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.error += ex.Message;
        //    }
        //    return View();
        //}

        [HttpGet]
        public JsonResult GetReportsByFrmId(string id)
        {
            //fetch reports by selected form
            var reports = _DbContext.ReportTemplate.Where(e => e.FormId == long.Parse(id)).Select(e => new SelectListItem{ 
                Value = e.ReportId.ToString(),
                Text = e.ReportName
            }).ToList();

            if (reports.Count <= 0)
            {
                var response = Json("");
                response.StatusCode = 500;
                return response;
            }

            var resp = Json(reports);
            resp.StatusCode = 200;
            return resp;
        }

        [HttpPost]
        public JsonResult Export(IFormCollection frm)
        {
            string msg = "";
            try
            {
                if (frm != null)
                {
                    string _rpt = frm["report"].ToString().Trim();
                    long rpt = long.Parse(_rpt.Substring(0, _rpt.IndexOf(':')));

                    List<string> _fields = new List<string>();
                    List<string> tableHeaders = new List<string>();
                    int i = 0;
                    foreach (var fld in _rpt.Substring(_rpt.IndexOf(':') + 1).Split('|').ToList())
                    {
                        _fields.Add(fld.Substring(fld.IndexOf(" »") + 2));
                        if (i > 0)
                            tableHeaders.Add("<th>" + fld.Substring(0, fld.IndexOf(" »")) + "</th>");
                        else
                        {
                            tableHeaders.Add("<th>Operator Name</th>");
                            tableHeaders.Add("<th>" + fld.Substring(0, fld.IndexOf(" »")) + "</th>");
                        }
                        i++;
                    }
                    var objTable = new EntryList(rpt, true);
                    List<string> rows = new List<string>();
                    if (objTable.FormEntry != null)
                    {
                        foreach (dynamic row in objTable.FormEntry)
                        {
                            int p = 0;

                            long entrId = long.Parse(row["EntryId"]);
                            var operatorName = _DbContext.FormsAndEntry.FirstOrDefault(e => e.EntryId == entrId).OrganizationName;
                            string tds = "";
                            foreach (var field in _fields)
                            {
                                if(p <= 0)
                                {
                                    tds += "<td>" + operatorName + "</td>";
                                    tds += "<td>" + row[field].ToString() + "</td>";
                                }
                                else
                                    tds += "<td>" + row[field].ToString() + "</td>";
                                p++;
                            }
                            rows.Add("<tr>" + tds + "</tr>");
                        }
                    }
                    var resp = new List<object> { new { header = tableHeaders, body = rows } }[0];
                    return Json(resp);
                }
                msg += "Error: Invalid report";
            }
            catch (Exception ex)
            {
                msg += "Error: " + ex.Message;
            }
            return Json(msg);
        }

        public IActionResult TariffExport()
        {
            try
            {
                var tariff = _DbContext.FormsDetails.Select(x => new 
                { 
                    Entry = x.EntryId,
                    Description = x.ProductName + " >> " + x.AppType + " >> " + x.DateCreated.Year.ToString()
                }).ToList();
               
                    ViewBag.TariffList = new SelectList( tariff, "Entry", "Description");


            }
            catch (Exception ex)
            {
                ViewBag.error += ex.Message + ":" + ex.InnerException;
            }
            return View();
        }

        public IActionResult TariffReport()
        {
            try
            {
                var Report = _DbContext.ReportTemplate.Where(x => x.ReportType == "TARIFF").Select(sl => new { sl.ReportId, sl.ReportName}).ToList();

                ViewBag.TariffList = new SelectList(Report, "ReportId", "ReportName");

                var operators = _DbContext.UserProfile.Where(w => w.UserType.ToLower() == "public").OrderBy(o => o.OrganizationName).Select(sl => new { sl.OrganizationId, sl.OrganizationName}).ToList();

                ViewBag.ops = new SelectList(operators, "OrganizationId", "OrganizationName");

                var Processor = _DbContext.UserProfile.Where(w => w.UserType.ToLower() == "private").OrderBy(o => o.Fullname).Select(sl => new { sl.UserId, sl.Fullname }).ToList();

                ViewBag.admins = new SelectList(Processor, "UserId", "Fullname");

                var PlanType = _DbContext.Forms.Where(w => w.FormsType.ToLower() == "tariff_type").Select(sl => new { sl.RecId, sl.FormsTypeCategory }).ToList();

                ViewBag.frmTypes = new SelectList(PlanType, "RecId", "FormsTypeCategory");
            }
            catch (Exception ex)
            {
                ViewBag.error += ex.Message + ":" + ex.InnerException;
            }
            return View();
        }
        
        public IActionResult SurveyReport()
        {
            try
            {
                //var survey = _DbContext.ReportTemplate.Where(x => x.ReportType == "SURVEY").ToList();
               
                var forms = _DbContext.Forms.Where(e => e.FormsType.ToLower() == "operator_type" && e.Published).Select(sl => new { sl.RecId, sl.FormName }).ToList();

                //var forms = _DbContext.Forms.Where(e => e.FormsType.ToLower() == "operator_type" && e.Published);

                //ViewBag.SurveyList = new SelectList( survey, "ReportId", "ReportName");
                ViewBag.FormList = new SelectList(forms, "RecId", "FormName");
            }
            catch (Exception ex)
            {
                ViewBag.error += ex.Message + ":" + ex.InnerException;
            }
            return View();
        }

        

        [HttpPost]
        public JsonResult ReportData(long reportId, string operatorId, DateTime from, DateTime to)
        {
            string msg = "";
            try
            {
                var report = _DbContext.ReportTemplate.Where(x => x.ReportId == reportId).FirstOrDefault();

                string dateFrom = from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                string dateTo = to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                //Populating a DataTable from database.
                var dt = new DataTable();
                if (from != DateTime.Parse("01/01/0001 00:00:00") && to != DateTime.Parse("01/01/0001 00:00:00"))
                {
                    dt = _Report.ExecuteDataTable(report.ReportSQL + " AND " + " DateSubmitted >= " + " '" + dateFrom + "' " + " AND " + " DateSubmitted <= " + " '" + dateTo + "' " + report.OrderBy);
                }
                else
                {
                    dt = _Report.ExecuteDataTable(report.ReportSQL +" "+ report.OrderBy);
                }

                //Building an HTML string.
                StringBuilder html = new StringBuilder();

                if(dt != null)
                {
                    //Table start.
                    //html.Append("<table id='tblExport' class='table table-striped table-hover'>");
                    html.Append("<div class='text-right mt-2 pt-2'>");
                    html.Append("<button class='btn btn-primary btn-sm chart' id='btnExport' type='button'>");
                    html.Append("<i class='fa fa-save'>");
                    html.Append("</i>");
                    html.Append(" Export Report");
                    html.Append("</button>");
                    html.Append("</div>");
                    html.Append("<table id='tblExport' class='table'>");

                    //Building the Header row.
                    html.Append("<tr>");
                    foreach (DataColumn column in dt.Columns)
                    {        
                        html.Append("<th>");
                        html.Append(column.ColumnName);
                        html.Append("</th>");
                    }
                    html.Append("</tr>");

                    //Building the Data rows.

                    if (!string.IsNullOrEmpty(operatorId))
                    {

                        foreach (DataRow row in dt.Rows)
                        {

                            if (row["OperatorId"].ToString() == operatorId)
                            {
                                html.Append("<tr>");
                                foreach (DataColumn column in dt.Columns)
                                {
                                    html.Append("<td>");
                                    html.Append(row[column.ColumnName]);
                                    html.Append("</td>");
                                }
                                html.Append("</tr>");
                            }
                        }
                    }
                    else
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            html.Append("<tr>");
                            foreach (DataColumn column in dt.Columns)
                            {
                                html.Append("<td>");
                                html.Append(row[column.ColumnName]);
                                html.Append("</td>");
                            }
                            html.Append("</tr>");
                        }
                    }

                    //Table end.
                    html.Append("</table>");

                    return Json(html.ToString());
                }
                else
                {
                    var message = "<div class='text-center mt-2 border-top border-bottom pt-2'> No data</div>";
                    return Json(message);
                }

            }
            catch (Exception ex)
            {
                msg += "Error: " + ex.Message;
            }
            return Json(msg);

        }        
        
        //[HttpPost]
        //public JsonResult SurveyReportData(long reportId, int formId, DateTime from, DateTime to)
        //{
        //    string msg = "";
        //    string form = "";
        //    string fields = " AND ";
        //    string groupBy = "Group By dbo.FormsEntry.FieldLabel, dbo.FormsAndEntry.OrganizationName,dbo.FormsEntry.FieldName,dbo.FormsEntry.Response,dbo.FormsEntry.ValueYear,dbo.FormsEntry.FrmYear";

        //    if (formId > 0)
        //        form += " AND dbo.FormsAndEntry.RecId = " + formId;
        //    try
        //    {
        //        var report = _DbContext.ReportTemplate.Where(x => x.ReportId == reportId).FirstOrDefault();

        //        var templateFields = report.FormFields.Replace("\r\n",string.Empty).Trim().Split(", ");
        //        int Y = 0;
        //        foreach (var field in templateFields)
        //        {
        //            if (Y <= 0)
        //                fields += "dbo.FormsEntry.FieldLabel = " + "'" + field.Trim() + "'";
        //            else
        //                fields += " OR dbo.FormsEntry.FieldLabel = " + "'" + field.Trim() + "'";
        //            Y++;
        //        }

        //        string dateFrom = from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        //        string dateTo = to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        //        if (report == null)
        //        {
        //            return Json("");
        //        }

        //        //Populating a DataTable from database.
        //        var dt = new DataTable();
        //        if (from != DateTime.Parse("01/01/0001 00:00:00") && to != DateTime.Parse("01/01/0001 00:00:00"))
        //        {
        //            dt = _Report.ExecuteDataTable(report.ReportSQL + " AND " + " DateSubmitted >= " + " '" + dateFrom + "' " + " AND " + " DateSubmitted <= " + " '" + dateTo + "' " + report.OrderBy);
        //        }
        //        else
        //        {
        //            dt = _Report.ExecuteDataTable(report.ReportSQL + form + fields + groupBy);
        //            //dt = _DbContext.FormsEntry.FromSqlRaw(report.ReportSQL)
        //        }

        //        //Building an HTML string.
        //        StringBuilder html = new StringBuilder();

        //        if(dt != null)
        //        {

        //            //extract data from datatable
        //            Dictionary<string, string> body = new Dictionary<string, string>();
        //            List<string> _hds = new List<string>();
        //            List<string> templateHeaders = new List<string>();
        //            List<string> values;
        //            List<string> labels = new List<string>();
        //            var operators = new List<string>();

        //            //extract all the form fields from datatabble to be made datatable headers
        //            //also extract the values into a collection to form the row
        //            int t = -1;
        //            foreach (DataRow item in dt.Rows)
        //            {
        //                t++;
        //                var label = dt.Rows[t]["FieldLabel"].ToString();
        //                var operatorId = dt.Rows[t]["OrganizationName"].ToString();
        //                var Value = dt.Rows[t]["Response"].ToString();
        //                var year = dt.Rows[t]["ValueYear"].ToString();
        //                operators.Add(!operators.Contains(operatorId) ? operatorId : "");
        //                if (templateFields.Contains(label))
        //                {
        //                    if(!body.Keys.Contains(label))
        //                        body.Add(label, operatorId + "," + (string.IsNullOrEmpty(Value) ? "N/A" : Value));
        //                    else
        //                        body.Add(label + "%" + t, operatorId + "," + (string.IsNullOrEmpty(Value) ? "N/A" : Value));
        //                }
        //            }
        //            operators.Remove("");
        //            dt.Columns.Clear();
        //            dt.Rows.Clear();
        //            operators.RemoveRange(operators.IndexOf(""), operators.Count - operators.IndexOf(""));

        //            int i = 0;
        //            //dt.Rows.Clear();
        //            //foreach (DataRow item in dt.Rows)
        //            //{
        //            //    //if(item.ColumnName.ToLower() == "fieldlabel")
        //            //    //{
        //            //        foreach (var itm in report.FormFields.Split(","))
        //            //        {
        //            //            //var value = dt.Rows[item.ColumnName]

        //            //        }
        //            //    //}
        //            //}

        //            //Table start.
        //            //html.Append("<table id='tblExport' class='table table-striped table-hover'>");
        //            html.Append("<table id='indexDataTable' class='table text-center table-sm table-hover'>");

        //            //Building the Header row.
        //            html.Append("<tr>");
        //            List<string> hds = new List<string>();
        //            //foreach (var item in report.FormFields.Split(","))
        //            //{
        //            //    hds.Add(item);
        //            //    //foreach(DataColumn column in dt.Columns)
        //            //    //{
        //            //    //    if (column.ColumnName.Contains(item))
        //            //    //        hds.Add(column);
        //            //    //}
        //            //}
        //            foreach (var column in templateFields)
        //            {
        //                DataColumn data = new DataColumn();
        //                data.ColumnName = column;
        //                dt.Columns.Add(data);

        //                html.Append("<th>");
        //                html.Append(column);
        //                html.Append("</th>");
        //            }
        //            html.Append("</tr>");

        //            //List<DataRow> rws = new List<DataRow>();
        //            //foreach (DataRow item in dt.Rows)
        //            //{
        //            //    foreach (DataColumn col in hds)
        //            //    {
        //            //      var field = item[col.ColumnName];
        //            //    }
        //            //}
        //            //Building the Data rows.
        //            //object[] operators = new object[500];

        //            //foreach (DataColumn item in dt.Columns)
        //            //{
        //            //    DataRow row = dt.NewRow();
        //            //    //foreach (var cell in body)
        //            //    //{

        //            //    //var opp = body.Where(e => e.Key.Contains(item.ColumnName)).Select(s => s.Key);
        //            //    //var val = body.Where(e => e.Key.Contains(item.ColumnName)).Select(s => s.Value);
        //            //    values = new List<string>();

        //            //    foreach (var rw in body)
        //            //    {
        //            //        values.Add()
        //            //        //var val = row.
        //            //        //row.SetField(item, val);
        //            //    }

        //            //    dt.Rows.Add(row,);
        //            //    //}
        //            //}
        //            foreach (var item in operators)
        //            {
        //                DataRow rw = dt.NewRow();
        //                dt.Rows.Add(rw);
        //            }
        //            int r = 0;

        //            foreach (var item in operators)
        //            {
        //                int c = 0;
        //                foreach (DataColumn dataColumn in dt.Columns)
        //                {

        //                    var val = body.FirstOrDefault(e => e.Key.StartsWith(dataColumn.ColumnName) && e.Value.StartsWith(item)).Value;
        //                    if (c <= 0)
        //                    {
        //                        dt.Rows[r][dataColumn.ColumnName] = item;
        //                        c++;
        //                    }
        //                    else
        //                        dt.Rows[r][dataColumn.ColumnName] = string.IsNullOrEmpty(val) ? "N/A" : val.Substring(val.IndexOf(",") + 1);
        //                }
        //                r++;

        //            }



        //            //foreach (var _operator in operators )
        //            //{
        //            //    int r = -1;
        //            //    DataRow dRow = dt.NewRow();
        //            //    List<string> vals = new List<string>();
        //            //    foreach (DataColumn col in dt.Columns)
        //            //    {
        //            //        r++;
        //            //        foreach (var item in body.Where(e => e.Key.StartsWith(col.ColumnName)))
        //            //        {
        //            //            int d = item.Value.IndexOf(",");
        //            //            if(r <= 0)
        //            //                vals.Add(_operator);
        //            //            if (item.Value.StartsWith(_operator))
        //            //                vals.Add(item.Value.Substring(item.Value.IndexOf(",") + 1));
        //            //        }
        //            //    }
        //            //    dRow.ItemArray = vals.ToArray();

        //            //}

        //            foreach (DataRow row in dt.Rows)
        //            {
        //                html.Append("<tr>");
        //                foreach (DataColumn column in dt.Columns)
        //                {
        //                    html.Append("<td>");
        //                    html.Append(row[column.ColumnName]);
        //                    html.Append("</td>");
        //                }
        //                html.Append("</tr>");
        //            }

        //            //Table end.
        //            html.Append("</table>");
        //            html.Append("<div class='text-right mt-2 position-fixed pt-2'>");
        //            html.Append("<button class='btn btn-primary btn-sm chart' id='btnExport' type='button'>");
        //            html.Append("<i class='fa fa-save'>");
        //            html.Append("</i>");
        //            html.Append(" Export Report");
        //            html.Append("</button>");
        //            html.Append("</div>");
        //            return Json(html.ToString() );
        //        }
        //        else
        //        {
        //            var message = "<div class='text-center mt-2 border-top border-bottom pt-2'> No data</div>";
        //            return Json(message);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        msg += "Error: " + ex.Message;
        //    }
        //    return Json(msg);

        //}


        [HttpPost]
        public async Task<IActionResult> CreateTemplate(IFormCollection frm)
        {
            var formfields = frm["RptFrmFields"].ToString();
            var formId = long.Parse(frm["FormId"].ToString());
            var rptTitle = frm["RptTitle"].ToString();
            var tableType = frm["tableType"].ToString();

            if(!string.IsNullOrEmpty(formfields) && formId > 0 && !string.IsNullOrEmpty(rptTitle) && !string.IsNullOrEmpty(tableType))
            {
                ReportTemplate template = new ReportTemplate()
                {
                    FormFields = formfields,
                    FormId = formId,
                    ReportName = rptTitle,
                    TableType = int.Parse(tableType),
                    ReportType = "SURVEY"
                };

                await _DbContext.ReportTemplate.AddAsync(template);
            }
            else
            {
                ViewData["err"] = "Report template Creation failed... Please Contact Administrator!";
                return View("Export");
            }
                
            await _DbContext.SaveChangesAsync(true);
            TempData["message"] = "Report Template Creation Successful...";
            ViewBag.SurveyList = GetTypeList(FormsType.SURVEY);

            return View("Export");
        }

        [HttpPost]
        public JsonResult SurveyReportData(long reportId, int formId, DateTime from, DateTime to)
        {
            string msg = "";
            try
            {
                string html = _ReportHelper.ReportTemp1(reportId, formId,from,to);

                if(html == null)
                {
                    var message = "<div class='text-center mt-2 border-top border-bottom pt-2'> No data</div>";

                    return Json(message);
                }
                return Json(html);
            }
            catch (Exception ex)
            {
                msg += ex.Message;
                return Json(msg);
            }

        }


        [HttpPost]
        public JsonResult TariffExport(IFormCollection frm)
        {
            string msg = "";
            try
            {
                if (frm != null)
                {
                    string _rpt = frm["report"].ToString().Trim();
                    long rpt = long.Parse(_rpt.Substring(0, _rpt.IndexOf(':')));

                    List<string> _fields = new List<string>();
                    List<string> tableHeaders = new List<string>();
                    foreach (var fld in _rpt.Substring(_rpt.IndexOf(':') + 1).Split('|').ToList())
                    {
                        _fields.Add(fld.Substring(fld.IndexOf(" »") + 2));
                        tableHeaders.Add("<th>" + fld.Substring(0, fld.IndexOf(" »")) + "</th>");
                    }
                    var objTable = new EntryList(rpt, true);
                    List<string> rows = new List<string>();
                    if (objTable.FormEntry != null)
                    {
                        foreach (dynamic row in objTable.FormEntry)
                        {
                            string tds = "";
                            foreach (var field in _fields) tds += "<td>" + row[field].ToString() + "</td>";
                            rows.Add("<tr>" + tds + "</tr>");
                        }
                    }
                    var resp = new List<object> { new { header = tableHeaders, body = rows } }[0];
                    return Json(resp);
                }
                msg += "Error: Invalid report";
            }
            catch (Exception ex)
            {
                msg += "Error: " + ex.Message;
            }
            return Json(msg);
        }

        [HttpPost]
        public JsonResult TariffListFormFields(IFormCollection frm)
        {
            try
            {
                if (frm != null)
                {
                    var objOperators = string.Join(",", (from c in _DbContext.FormsAndEntry where c.RecId == long.Parse(frm["entry"]) select c.OperatorId).Distinct().ToArray().ToList());

                    /*
                     //order content
                    var orderedFields = objEntry.FormFields.ToList();
                    orderedFields.Sort((x, y) => x.Key.CompareTo(y.Key));
                     */
                    EntryList objEntry = new EntryList(long.Parse(frm["entry"]), true);
                    return Json(new List<object> { new { fields = objEntry.FormFields, operators = objOperators } }[0]);
                }
                else
                {
                    return Json("error");
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
