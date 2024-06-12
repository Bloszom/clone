using System.Collections.Generic;
using System.Linq;
using pcea.Models;
using pceaLibrary;
using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;

namespace pcea.Helpers
{
    public class CompanyDataHelper
    {
        public static PceaDbContext _DbContext { get; set; }
        private static Vars _Vars;

        //public CompanyDataHelper()
        //{
        //    _DbContext = new PceaDbContext();
        //}

        /// <summary>
        /// A key value pair holding the field bindings for the survey forms
        /// </summary>
        public static Dictionary<string, string> CompanyData = new Dictionary<string, string>
        {
            { "Legal Name", "OrganizationLongName" },
            { "Operating Or Trade Name", "OrganizationShortName" },
            { "Company Address", "OtherInfoStreet" },
            { "Company City", "OtherInfoCity" },
            { "Company Telephone(s)", "OtherInfoTelephone" },
            { "Website", "OtherInfoWebsite" },
            { "Company Fax", "OtherInfoFax" },
            { "Company Email", "OtherInfoEmail" },
            { "Contact Telephone(s)", "Telephone" },
            { "Contact Name", "Fullname" },
            { "Contact Website", "OtherInfoWebsite" },
            { "Contact Designation", "JobTitle" },
            { "Contact Fax", "OtherInfoFax" },
            { "Contact Email", "Email" }
        };

        public static string FetchData (Vars vars)
        {
            _Vars = vars;

            try
            {
                var objForms = (from m in _DbContext.Forms select m).Where(m => m.Published && m.FormsType.ToLower() == "operator_type");

                //check for companny data existence
                var check = _DbContext.CompanyDataSubmissions.FirstOrDefault(e => e.OrganizationId == _Vars.OperatorId);
                var stringList = string.Empty;
                if (check != null)
                {
                    stringList = check.FormFieldsData;
                }

                return stringList;
            }
            catch (Exception Ex)
            {
                return "";
            }
        }

        public static string GetData(Vars vars)
        {
            _Vars = vars;

            try
            {
                var objForms = (from m in _DbContext.Forms select m).Where(m => m.Published && m.FormsType.ToLower() == "operator_type");

                //check for companny data existence
                var check = _DbContext.CompanyDataSubmissions.FirstOrDefault(e => e.OrganizationId == _Vars.OperatorId);
                var stringList = string.Empty;

                //extract the companny data template fields
                var fields = objForms.FirstOrDefault().CompanyInfoFields.Split(",").ToList();
                //fetch the organization details from profiles
                var profiles = _DbContext.AppUserProfileView.Where(e => e.OrganizationId == _Vars.OperatorId).Select(sl => new
                {
                    jObj = JObject.FromObject(sl),
                }).FirstOrDefault();
                //create a new string from the result of combining the fields with the data from profiles
                var index = -1;
                fields.ForEach((param) =>
                {
                    var field = param.Substring(param.IndexOf(":") + 1).Replace(" ", "");
                    var test = string.Empty;
                    //var _field = CompanyData.FirstOrDefault(e => field.ToLower().Contains(e.Key.ToLower().Replace(" ", ""))).Value;

                    foreach (var item in CompanyData)
                    {
                        test = Regex.Replace(item.Key, @"\s", "");

                        if (field.ToLower().Contains(test.ToLower()))
                        {
                            var objList = profiles.jObj.Children().ToList();
                            var value = string.Empty;


                            var objval = objList.FirstOrDefault(e => e.Path == item.Value);

                            if (!string.IsNullOrEmpty(item.Value))
                            {
                                index++;

                                if (stringList == "")
                                    stringList = $"{param}:{objval.Children().FirstOrDefault()}";
                                else
                                    stringList = $"{stringList},{param}:{objval.Children().FirstOrDefault()}";
                            }
                        }
                    }


                    //objList.ForEach((obj) =>
                    //{
                    //    value = obj.Children().FirstOrDefault().ToString();
                    //    if (obj.Path == _field && string.IsNullOrEmpty(stringList))
                    //        stringList = $"{param}:{value},";
                    //    //else
                    //    //    stringList = $"{stringList}:{obj.Value<string>(obj.Path)},";
                    //});
                });

                //if (check != null)
                //{
                //    //extract the formfields as a list
                //    var oldList = check.FormFieldsData.Split(",").ToList();
                //    var fieldVals = check.FormFieldsData;
                //    var newlist = stringList.Split(",").ToList();
                //    //Icompare formfields with the newly generatd stringlist and create a new list relacing the matched data
                //    oldList.ForEach((param) =>
                //    {
                //        if (!string.IsNullOrEmpty(param))
                //        {
                //            var data1 = param.Split(":");
                //            var data2 = newlist.FirstOrDefault(e => e.Split(":").Contains(data1[1])).Append(char.Parse(data1[2]));
                //            var newstring = string.Concat(data2);
                //            var split = fieldVals.Split(param);
                //            fieldVals = $"{split[0]} {newstring} {split[1]}";
                //        }

                //    });

                //check.FormFieldsData = fieldVals;

                //_DbContext.CompanyDataSubmissions.Update(check);
                //var success = _DbContext.SaveChanges(true);

                //if (success > 0)
                //{
                //    return true;
                //}
                //else
                //    return false;
                //}
                //else
                //{
                //    //first time record entry
                //    check = new CompanyDataSubmission();
                //    check.FormFieldsData = stringList;
                //    check.OrganizationId = _Vars.OperatorId;
                //    _DbContext.CompanyDataSubmissions.Add(check);
                //    var success = _DbContext.SaveChanges(true);

                //    if (success > 0)
                //    {
                //        return true;
                //    }
                //    else
                //        return false;
                //}
                return stringList;
                //check.FormFieldsData = stringList;

                //_DbContext.CompanyDataSubmissions.Update(check);
                //var checkSave = _DbContext.SaveChanges(true);

                //if (checkSave > 0)
                //    return true;
                //else
                //    return false;
                //return false;
            }
            catch (Exception Ex)
            {
                return "";
            }
        }
    }
}
