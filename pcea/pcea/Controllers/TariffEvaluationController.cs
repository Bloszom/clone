using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using pcea.Models;
using pceaLibrary;
using System.IO;
using System.Web;
using Microsoft.AspNetCore.Mvc.Abstractions;
using System.Text.RegularExpressions;

namespace pcea.Controllers
{
    public class TariffEvaluationController : Controller
    {
        private readonly PceaDbContext _DbContext;
        public TariffEvaluationController(PceaDbContext context)
        {
            _DbContext = context;
        }

        private bool TariffEvaluationExists(long id)
        {
            return _DbContext.TariffEvaluation.Any(e => e.EntryId == id);
        }
        SelectList TariffTypes()
        {
            return new SelectList(
                    (from m in _DbContext.MetaDataRef select m).Where(m => m.MetaDataType == "TARIFF_TYPE")
                    .OrderBy(m => m.ReferenceId), "ReferenceId", "ReferenceId");
        }

        public async Task<IActionResult> Evaluate(long id)
        {
            try
            {
                var _FormsAndEntry = await _DbContext.FormsAndEntry.FirstOrDefaultAsync(m => m.EntryId == id);
                if (_FormsAndEntry == null)
                {
                    //EntryId does not exist
                    ViewBag.Message = "Invalid request";
                    return View("Evaluate");
                }
                ViewBag.OperatorId = _FormsAndEntry.OperatorId;
                ViewBag.DateSubmitted = _FormsAndEntry.DateSubmitted;
                ViewBag.FormName = _FormsAndEntry.FormName;

                if (!TariffEvaluationExists(id))
                {
                    //first time tariff request is being evaluated
                    TariffEvaluation _newTariffEval = new TariffEvaluation
                    {
                        FormId = _FormsAndEntry.RecId,
                        EntryId = _FormsAndEntry.EntryId
                    };
                    _DbContext.Add(_newTariffEval);
                    await _DbContext.SaveChangesAsync();

                    var _TariffEval = await _DbContext.TariffEvaluation.FirstOrDefaultAsync(m => m.EntryId == id);
                    ViewBag.EntryId = _TariffEval.EntryId;
                    ViewBag.FormId = _TariffEval.FormId;
                    ViewBag.RecId = _TariffEval.RecId;

                    return View("Evaluate");
                }
                else
                {
                    var _TariffEval = await _DbContext.TariffEvaluation.FirstOrDefaultAsync(m => m.EntryId == id);
                    ViewBag.EntryId = _TariffEval.EntryId;
                    ViewBag.FormId = _TariffEval.FormId;
                    ViewBag.RecId = _TariffEval.RecId;

                    return View("Evaluate");
                }
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        [HttpPost]
        public JsonResult GetFormFields(long lFormId)
        {
            try
            {
                if (lFormId != 0)
                {
                    //var objOperators = string.Join(",", (from c in _DbContext.FormsAndEntry where c.EntryId == lEntryId select c.OperatorId).Distinct().ToArray().ToList());
                    //var _selDefenition = from m in _DbContext.TariffEvalDetail where m.EntryId == lEntryId select m.FormSelection;

                    EntryList objEntry = new EntryList(lFormId, true);
                    //return Json(new List<object> { new { fields = objEntry.FormFields, operators = objOperators, evalDef = _selDefenition } }[0]);
                    return Json(new List<object> { new { fields = objEntry.FormFields} }[0]);
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
        public async Task<IActionResult> GetMatchedParameters(long lEntryId)
        {
            try
            {
                var _TariffAnalysis = await _DbContext.TariffAnalysis.Where(m => m.EntryId == lEntryId).ToListAsync();
                return PartialView("TariffAnalysis", _TariffAnalysis);
            }
            catch (Exception)
            {
                ViewBag.Message = BadRequest() + " Tariff evaluation Parameters not yet defined.";
                return BadRequest();
            }
        }
        public async Task<IActionResult> SaveMatchedParameter(long lFormId, long lEntryId, string sField, string sParm)
        {
            try
            {
                //check if parameter has been added previously
                var _TariffAnal = await _DbContext.TariffAnalysis.FirstOrDefaultAsync(m => m.EntryId == lEntryId && m.ParameterId == sParm );
                if (_TariffAnal == null)
                {
                    //it's a new match record for this EntryId
                    string[] sFields = sField.Split('»');
                    TariffAnalysis _newTariffAnal = new TariffAnalysis
                    {
                        EntryId = lEntryId,
                        FormId = lFormId,
                        ParameterId = sParm,
                        FieldId = sFields[1],
                        FieldName = sFields[0]
                    };
                    _DbContext.Add(_newTariffAnal);
                    await _DbContext.SaveChangesAsync();
                }
                else
                {
                    ViewBag.Message = "This Parameter has been matched previously";
                }

                //retrieve collection
                ViewBag.Message = "Parameter matched successfully";
                return PartialView("TariffAnalysis", await _DbContext.TariffAnalysis.Where(m => m.EntryId == lEntryId).ToListAsync());
            }
            catch (DbUpdateConcurrencyException)
            {
                ViewBag.Message = BadRequest() + " Tariff Parameter matching failed.";
                return BadRequest();
            }
        }
        public async Task<IActionResult> DeleteMatchedParameter(long lRecId, long lEntryId)
        {
            try
            {
                var _TariffAnalysis = await _DbContext.TariffAnalysis.FindAsync(lRecId);
                _DbContext.TariffAnalysis.Remove(_TariffAnalysis);
                await _DbContext.SaveChangesAsync();

                //retrieve collection
                ViewBag.Message = "Parameter deleted";
                return PartialView("TariffAnalysis", await _DbContext.TariffAnalysis.Where(m => m.EntryId == lEntryId).ToListAsync());
            }
            catch (Exception)
            {
                ViewBag.Message = BadRequest() + " Tariff Parameter delete failed.";
                return BadRequest();
            }
        }
        public async Task<IActionResult> TariffAnalysisReport(long id)
        {
            try
            {
                var _FormsAndEntry = await _DbContext.FormsAndEntry.FirstOrDefaultAsync(m => m.EntryId == id);
                if (_FormsAndEntry == null)
                {
                    //EntryId does not exist
                    ViewBag.Message = "Invalid request";
                    return View("TariffAnalysisReport");
                }
                ViewBag.OperatorId = _FormsAndEntry.OperatorId;
                ViewBag.DateSubmitted = _FormsAndEntry.DateSubmitted;
                ViewBag.FormName = _FormsAndEntry.FormName;

                if (!TariffAnalysisReportExists(id))
                {
                    ViewBag.Message = "Tariff evaluation unavailable";
                    return View("TariffAnalysisReport");
                }
                else
                {
                    var _TariffAnalRep = await _DbContext.TariffAnalysisReport.Where(m => m.EntryId == id).ToListAsync();
                    return View("TariffAnalysisReport", _TariffAnalRep);
                }
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        private bool TariffAnalysisReportExists(long id)
        {
            return _DbContext.TariffAnalysisReport.Any(e => e.EntryId == id);
        }
        public JsonResult GetTariffAnalysisReport(long lEntryId)
        {
            try
            {
                if (!TariffAnalysisReportExists(lEntryId))
                {
                    return Json(new List<object> { new { EntryId = string.Empty } });
                }
                else
                {
                    return Json(new List<object> { new { EntryId = lEntryId } });
                }

                //if (frm != null)
                //{
                //    var objOperators = string.Join(",", (from c in _DbContext.FormsAndEntry where c.RecId == long.Parse(frm["entry"]) select c.OperatorId).Distinct().ToArray().ToList());

                //    var _selDefenition = from m in _DbContext.TariffEvalDetail where m.FormId == long.Parse(frm["entry"]) select m.FormSelection;

                //    /*
                //     //order content
                //    var orderedFields = objEntry.FormFields.ToList();
                //    orderedFields.Sort((x, y) => x.Key.CompareTo(y.Key));
                //     */
                //    EntryList objEntry = new EntryList(long.Parse(frm["entry"]), true);
                //    return Json(new List<object> { new { fields = objEntry.FormFields, operators = objOperators, evalDef = _selDefenition } }[0]);
                //}
                //else
                //{
                //    return Json("error");
                //}
            }
            catch (Exception ex)
            {
                ViewBag.error = ex.Message;
                return Json(ex.Message);
            }
        }
        public async Task<IActionResult> EvaluateTariffRequest(long lFormId, long lEntryId)
        {
            try
            {
                var _TariffAnal = await _DbContext.TariffAnalysis.Where(m => m.EntryId == lEntryId).ToListAsync();
                if (_TariffAnal == null)
                {
                    //Field matching has not been done
                    return BadRequest("Invalid Operation.  Evaluation fields has not been matched.");
                }
                var _TariffReq = await _DbContext.FormsEntry.Where(m => m.EntryId == lEntryId && m.FormId == lFormId).OrderBy(a => a.RecId).ToListAsync();
                if(_TariffReq == null)
                {
                    //EntryId does not exist
                    return BadRequest("Invalid Operation.  Specified Tariff Request does not exist");
                }

                //I will hard-code values for test purposes for now.  
                //Change this section to use dynamic values from the ENUM later.
                decimal[] dMainAccount = new decimal[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                decimal[] dDataAccount = new decimal[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                decimal[] dVoiceBonus = new decimal[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                decimal[] dDataBonus = new decimal[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                decimal[] dBurnRateMainAccount = new decimal[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                decimal[] dBurnRateDataAccount = new decimal[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                decimal[] dBurnRateVoiceBonus = new decimal[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                decimal[] dBurnRateDataBonus = new decimal[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                int iTotalMain = 0;

                foreach(var objValue in _TariffAnal)
                {
                    int i = 0;
                    decimal dValue = 0;
                    long lLastRecId = 0;
                    long lNewRecId = 0;
                    if (objValue.ParameterId == "MainAccount")
                    {
                        i = 0;
                        dValue = 0;
                        lLastRecId = 0;
                        lNewRecId = 0;
                        while (GetNextValue(lEntryId, objValue.FieldId, lLastRecId, ref lNewRecId, ref dValue))
                        {
                            dMainAccount[i] = dValue;
                            lLastRecId = lNewRecId;
                            dValue = 0;
                            i++;
                        }
                        iTotalMain = i;
                    }
                    if (objValue.ParameterId == "DataAccount")
                    {
                        i = 0;
                        dValue = 0;
                        lLastRecId = 0;
                        lNewRecId = 0;
                        while (GetNextValue(lEntryId, objValue.FieldId, lLastRecId, ref lNewRecId, ref dValue))
                        {
                            dDataAccount[i] = dValue;
                            lLastRecId = lNewRecId;
                            dValue = 0;
                            i++;
                        }
                        iTotalMain = i;
                    }
                    if (objValue.ParameterId == "VoiceBonus")
                    {
                        i = 0;
                        dValue = 0;
                        lLastRecId = 0;
                        lNewRecId = 0;
                        while (GetNextValue(lEntryId, objValue.FieldId, lLastRecId, ref lNewRecId, ref dValue))
                        {
                            dVoiceBonus[i] = dValue;
                            lLastRecId = lNewRecId;
                            dValue = 0;
                            i++;
                        }
                    }
                    if (objValue.ParameterId == "DataBonus")
                    {
                        i = 0;
                        dValue = 0;
                        lLastRecId = 0;
                        lNewRecId = 0;
                        while (GetNextValue(lEntryId, objValue.FieldId, lLastRecId, ref lNewRecId, ref dValue))
                        {
                            dDataBonus[i] = dValue;
                            lLastRecId = lNewRecId;
                            dValue = 0;
                            i++;
                        }
                    }

                    //get equivalent burn rates
                    if (objValue.ParameterId == "BurnRateMainAccount")
                    {
                        i = 0;
                        dValue = 0;
                        lLastRecId = 0;
                        lNewRecId = 0;
                        while (GetNextValue(lEntryId, objValue.FieldId, lLastRecId, ref lNewRecId, ref dValue))
                        {
                            dBurnRateMainAccount[i] = dValue;
                            lLastRecId = lNewRecId;
                            dValue = 0;
                            i++;
                        }
                    }
                    if (objValue.ParameterId == "BurnRateDataAccount")
                    {
                        i = 0;
                        dValue = 0;
                        lLastRecId = 0;
                        lNewRecId = 0;
                        while (GetNextValue(lEntryId, objValue.FieldId, lLastRecId, ref lNewRecId, ref dValue))
                        {
                            dBurnRateDataAccount[i] = dValue;
                            lLastRecId = lNewRecId;
                            dValue = 0;
                            i++;
                        }
                    }
                    if (objValue.ParameterId == "BurnRateVoiceBonus")
                    {
                        i = 0;
                        dValue = 0;
                        lLastRecId = 0;
                        lNewRecId = 0;
                        while (GetNextValue(lEntryId, objValue.FieldId, lLastRecId, ref lNewRecId, ref dValue))
                        {
                            dBurnRateVoiceBonus[i] = dValue;
                            lLastRecId = lNewRecId;
                            dValue = 0;
                            i++;
                        }
                    }
                    if (objValue.ParameterId == "BurnRateDataBonus")
                    {
                        i = 0;
                        dValue = 0;
                        lLastRecId = 0;
                        lNewRecId = 0;
                        while (GetNextValue(lEntryId, objValue.FieldId, lLastRecId, ref lNewRecId, ref dValue))
                        {
                            dBurnRateDataBonus[i] = dValue;
                            lLastRecId = lNewRecId;
                            dValue = 0;
                            i++;
                        }
                    }
                }

                foreach (var objValue in _TariffAnal)
                {

                    if (objValue.ParameterId == "MainAccount")
                    {
                        for (int j = 0; j < iTotalMain; j++)
                        {
                            decimal dEffTariff = 0;
                            if(dMainAccount[j]!=0 && dBurnRateMainAccount[j]!=0)
                            {
                                dEffTariff = dMainAccount[j] / dBurnRateMainAccount[j];
                            }
                            TariffAnalysisReport _TariffRep = new TariffAnalysisReport()
                            {
                                EntryId = lEntryId,
                                FormId = lFormId,
                                ParameterId = "MainAccount",
                                Amount = dMainAccount[j],
                                BurnRate = dBurnRateMainAccount[j],
                                EffectiveTariff = dEffTariff
                            };
                            _DbContext.Add(_TariffRep);
                        }
                        await _DbContext.SaveChangesAsync();
                    }
                    if (objValue.ParameterId == "DataAccount")
                    {
                        for (int j = 0; j < iTotalMain; j++)
                        {
                            decimal dEffTariff = 0;
                            if (dDataAccount[j] != 0 && dBurnRateDataAccount[j] != 0)
                            {
                                dEffTariff = dDataAccount[j] / dBurnRateDataAccount[j];
                            }
                            TariffAnalysisReport _TariffRep = new TariffAnalysisReport()
                            {
                                EntryId = lEntryId,
                                FormId = lFormId,
                                ParameterId = "DataAccount",
                                Amount = dDataAccount[j],
                                BurnRate = dBurnRateDataAccount[j],
                                EffectiveTariff = dEffTariff
                            };
                            _DbContext.Add(_TariffRep);
                        }
                        await _DbContext.SaveChangesAsync();
                    }
                    if (objValue.ParameterId == "VoiceBonus")
                    {
                        for (int j = 0; j < iTotalMain; j++)
                        {
                            decimal dEffTariff = 0;
                            if (dVoiceBonus[j] != 0 && dBurnRateVoiceBonus[j] != 0)
                            {
                                dEffTariff = dVoiceBonus[j] / dBurnRateVoiceBonus[j];
                            }
                            TariffAnalysisReport _TariffRep = new TariffAnalysisReport()
                            {
                                EntryId = lEntryId,
                                FormId = lFormId,
                                ParameterId = "VoiceBonus",
                                Amount = dVoiceBonus[j],
                                BurnRate = dBurnRateVoiceBonus[j],
                                EffectiveTariff = dEffTariff
                            };
                            _DbContext.Add(_TariffRep);
                        }
                        await _DbContext.SaveChangesAsync();
                    }
                    if (objValue.ParameterId == "DataBonus")
                    {
                        for (int j = 0; j < iTotalMain; j++)
                        {
                            decimal dEffTariff = 0;
                            if (dDataBonus[j] != 0 && dBurnRateDataBonus[j] != 0)
                            {
                                dEffTariff = dDataBonus[j] / dBurnRateDataBonus[j];
                            }
                            TariffAnalysisReport _TariffRep = new TariffAnalysisReport()
                            {
                                EntryId = lEntryId,
                                FormId = lFormId,
                                ParameterId = "DataBonus",
                                Amount = dDataBonus[j],
                                BurnRate = dBurnRateDataBonus[j],
                                EffectiveTariff = dEffTariff
                            };
                            _DbContext.Add(_TariffRep);
                        }
                        await _DbContext.SaveChangesAsync();
                    }
                }

                //update tariff evaluation table
                TariffEvaluation _TariffEval = _DbContext.TariffEvaluation.Where(m => m.EntryId == lEntryId).FirstOrDefault();
                _TariffEval.EndDate = DateTime.Now;
                _DbContext.Update(_TariffEval);
                await _DbContext.SaveChangesAsync();

                //computation completed
                return Ok();
                //return PartialView("TariffReport", await _DbContext.TariffAnalysis.Where(m => m.EntryId == lEntryId).ToListAsync());
            }
            catch (Exception ex)
            {
                ViewBag.Message = BadRequest() + " Invalid EntryId. ErrMsg:" + ex.Message;
                return BadRequest() + ViewBag.Message;
            }
        }
        private bool GetNextValue(long lEntryId, string sFieldId, long lLastRecId, ref long lNewRecId, ref decimal dValue)
        {
            try
            {
                var _TariffReq = _DbContext.FormsEntry.Where(m => m.EntryId == lEntryId && m.RecId > lLastRecId).OrderBy(a => a.RecId).ToList();
                if (_TariffReq == null)
                {
                    //no more value.  return code to exit the iteration
                    return false;
                }
                foreach(var obj in _TariffReq)
                {
                    if(obj.FieldName == sFieldId)
                    {
                        lNewRecId = obj.RecId;
                        decimal.TryParse(obj.Response, out dValue);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private bool GetNextBurnRate(long lEntryId, string sFieldId, long lLastRecId, ref long lNewRecId, ref decimal dRate)
        {
            try
            {
                var _TariffReq = _DbContext.FormsEntry.Where(m => m.EntryId == lEntryId && m.RecId > lLastRecId).OrderBy(a => a.RecId).ToList();
                if (_TariffReq == null)
                {
                    //no more value.  return code to exit the iteration
                    return false;
                }
                foreach (var obj in _TariffReq)
                {
                    if (obj.FieldName == sFieldId)
                    {
                        lNewRecId = obj.RecId;
                        decimal.TryParse(obj.Response, out dRate);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }





    }
}