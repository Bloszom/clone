using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using _FrameworkCore;
using Newtonsoft.Json.Linq;

namespace pceaLibrary
{
    public class EntryList : _Database
    {

        /// <summary>
        /// Collect the Record Id of the form whose entries are to be tabulated
        /// </summary>
        public EntryList(long? FormId, bool fecthComplexList = false)
        {
            if (FormId > 0)
            {
                _formId = FormId;
                //if (fecthComplexList)
                //{
                LoadData();
                //} else
                //{
                //    LoadList();
                //}
            }
        }
        public bool ReportIsLoaded = false;
        public EntryList(string ReportID)
        {
            if (int.Parse(ReportID) > 0)
            {
                _formId = int.Parse(ReportID);
                ReportIsLoaded = LoadReport();
            }
        }

        public string ErrorMsg = "";

        long? _formId = 0;
        public IDictionary<string, string> FormFields { get; internal set; }
        public IEnumerable<object> FormEntry { get; internal set; }
        public DataTable FormEntryTable { get; internal set; }

        //to fetch simple submission list
        //bool LoadList()
        //{
        //    try
        //    {
        //        using (SqlCommand objCmd = new SqlCommand())
        //        {
        //            objCmd.CommandText = "select u.OrganizationId as Operator, f.*, e.* from Forms f " +
        //                "left join FormsOperators e on f.RecId = e.FormId " +
        //                "Left Join UserProfile u on e.OperatorId = u.OrganizationId WHERE e.FormId = @formid";
        //            objCmd.Parameters.Clear();
        //            objCmd.Parameters.AddWithValue("@formid", _formId);
        //            using (var _ds = ExecuteDataSet(objCmd))
        //            {
        //                if (_ds != null)
        //                {
        //                    if (_ds.Tables.Count > 0) FormEntryTable = _ds.Tables[0];
        //                }
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorMsg += ex.Message;
        //        return false;
        //    }
        //}

        //to fetch complex submission list including all user's entry
        bool LoadData()
        {
            try
            {
                using (SqlCommand objCmd = new SqlCommand())
                {
                    //form fields
                    objCmd.CommandText = "SELECT * FROM Forms WHERE RecId = @formid ";
                    objCmd.Parameters.Clear();
                    objCmd.Parameters.AddWithValue("@formid", _formId);
                    using (DataTable dt = ExecuteDataTable(objCmd))
                    {
                        if (dt.Rows.Count > 0)
                        {
                            DataTable newDT = new DataTable();
                            Dictionary<string, string> fields = new Dictionary<string, string>();

                            dynamic curForm = JObject.Parse(dt.Rows[0]["FormFields"].ToString()) as JObject;
                            foreach (dynamic strFields in curForm.fields)
                            {
                                dynamic _strFields = strFields.First;
                                string tg = _strFields.tag;
                                if ("input,textarea,select".Split(',').ToList().Contains(tg))
                                {
                                    string lbl = ((dynamic)_strFields.config).label;
                                    if (lbl.ToLower() != "button")
                                    {
                                        string fId = "f-" + (_strFields.id == null ? lbl : _strFields.id);
                                        fields.Add(fId, lbl);
                                        newDT.Columns.Add(fId);
                                    }
                                }

                            }
                            fields.Add("EntryId", "EntryId");
                            newDT.Columns.Add("EntryId");

                            FormFields = fields;

                            //get entries
                            objCmd.CommandText = "SELECT * FROM FormsEntry WHERE FormId=@formid ";
                            objCmd.Parameters.Clear();
                            objCmd.Parameters.AddWithValue("@formid", _formId);
                            using (var _ds = ExecuteDataSet(objCmd))
                            {
                                if (_ds != null)
                                {
                                    DataTable ds = _ds.Tables[0];
                                    if (ds.Rows.Count > 0)
                                    {
                                        List<object> iData = new List<object>();
                                        //group entries into specific record by EntryID
                                        foreach (var entry in ds.AsEnumerable().GroupBy(r => r.Field<long>("EntryId")))
                                        {

                                            DataRow newRw = newDT.NewRow();
                                            Dictionary<string, string> iRow = new Dictionary<string, string>();

                                            //primary key for this record
                                            iRow.Add("EntryId", entry.Key.ToString());
                                            newRw["EntryId"] = entry.ToString();

                                            //add record to this row
                                            foreach (DataRow iEntry in entry)
                                            {
                                                string curColumn = iEntry["FieldName"].ToString();
                                                if (fields.ContainsKey(curColumn))
                                                {
                                                    iRow.Add(curColumn, iEntry["Response"].ToString());
                                                    newRw[curColumn] = iEntry["Response"].ToString();
                                                }
                                            }

                                            //add row to table/object
                                            iData.Add(iRow);
                                            newDT.Rows.Add(newRw);
                                        }
                                        FormEntry = iData;
                                        FormEntryTable = newDT;
                                    }
                                }
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorMsg += ex.Message + "";
                return false;
            }
        }
        string MonthField(string mth, bool inString = false)
        {
            try
            {
                if (inString)
                {
                    switch (mth.Trim())
                    {
                        case "1":
                            return "Jan";
                        case "2":
                            return "Feb";
                        case "3":
                            return "Mar";
                        case "4":
                            return "Apr";
                        case "5":
                            return "May";
                        case "6":
                            return "Jun";
                        case "7":
                            return "Jul";
                        case "8":
                            return "Aug";
                        case "9":
                            return "Sep";
                        case "10":
                            return "Oct";
                        case "11":
                            return "Nov";
                        case "12":
                            return "Dec";

                        default:
                            return "0";
                    }
                }
                else
                {
                    switch (mth.Trim().ToLower().Substring(0, 3))
                    {
                        case "jan":
                            return "1";
                        case "feb":
                            return "2";
                        case "mar":
                            return "3";
                        case "apr":
                            return "4";
                        case "may":
                            return "5";
                        case "jun":
                            return "6";
                        case "jul":
                            return "7";
                        case "aug":
                            return "8";
                        case "sep":
                            return "9";
                        case "oct":
                            return "10";
                        case "nov":
                            return "11";
                        case "dec":
                            return "12";

                        default:
                            return "0";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg += ex.Message;
                return "0";
            }
        }
        public IList<string> RowFields { get; internal set; }
        bool LoadReport()
        {
            try
            {
                using (SqlCommand objCmd = new SqlCommand())
                {
                    //form fields
                    objCmd.CommandText = "SELECT * FROM Reports WHERE RecId = @formid ";
                    objCmd.Parameters.Clear();
                    objCmd.Parameters.AddWithValue("@formid", _formId);
                    using (DataTable dt = ExecuteDataTable(objCmd))
                    {
                        if (dt.Rows.Count > 0)
                        {
                            //DataTable newDT = new DataTable();
                            Dictionary<string, string> fields = new Dictionary<string, string>();

                            //ReportColumnType  [monthly, yearly, quaterly, operator, operator_type]
                            string grpBy = dt.Rows[0]["ReportColumnType"].ToString();
                            var ColmFields = dt.Rows[0]["ReportColumnName"].ToString().Split(',').ToList();

                            RowFields = dt.Rows[0]["ReportRowName"].ToString().Split(',').ToList();
                            //get entries
                            //string sqlQry = "SELECT u.Fullname as OperatorName, a.Operator, a.OperatorType, a.ReportValue, Month(a.Reportdate) monthly, Year(a.Reportdate) yearly, a.Year FROM Analysis a left join [UserProfile] u on a.Operator = u.UserId  WHERE a.ReportId=@formid";

                            string sqlQry = "SELECT DISTINCT Operator, OperatorType, ReportValue, Month(Reportdate) monthly, Year(Reportdate) yearly, Year FROM Analysis WHERE ReportId=@formid";

                            objCmd.CommandText = sqlQry;
                            objCmd.Parameters.Clear();
                            objCmd.Parameters.AddWithValue("@formid", _formId);
                            using (var _ds = ExecuteDataSet(objCmd))
                            {
                                if (_ds != null)
                                {
                                    DataTable ds = _ds.Tables[0];
                                    //build table columns
                                    switch (grpBy.ToLower())
                                    {
                                        case "monthly":
                                            for (int i = DateTime.Today.Month; i >= 1; i--)
                                                fields.Add(i.ToString() + "'20", MonthField(i.ToString(), true) + "'20");
                                            break;
                                        case "yearly":
                                            foreach (var str in ColmFields)
                                            {
                                                if (int.TryParse(str.Trim(), out int j))
                                                    if (j > 0) fields.Add(str, str);
                                            }
                                            //for (int i = 0; i < ColmFields.Count; i++)
                                            //    fields.Add((2020 - i).ToString(), (2020 - i).ToString());
                                            break;
                                        case "quarterly":
                                            for (int i = 0; i < 4; i++)
                                            {
                                                int yr = 2020 - i;
                                                for (int j = 1; j < 5; j++)
                                                    fields.Add("Q" + j.ToString() + " " + yr.ToString(), "Q" + j.ToString() + " " + yr.ToString());
                                            }
                                            break;
                                        case "operator":
                                        case "operator_type":
                                            foreach (string txt in dt.Rows[0]["ReportColumnName"].ToString().Split(','))
                                                try
                                                {
                                                    fields.Add(txt, "");
                                                }
                                                catch (Exception e)
                                                {
                                                    ErrorMsg += e;
                                                }
                                            break;
                                        default:
                                            break;
                                    }
                                    FormFields = fields;

                                    if (ds.Rows.Count > 0)
                                    {
                                        IEnumerable<dynamic> gp = null;
                                        if (grpBy.ToLower() == "monthly")
                                            gp = ds.AsEnumerable().GroupBy(r => r.Field<int>("monthly"));
                                        else if (grpBy.ToLower() == "yearly")
                                            gp = ds.AsEnumerable().GroupBy(r => r.Field<int>("yearly"));
                                        else if (grpBy.ToLower() == "quarterly")
                                            gp = ds.AsEnumerable().GroupBy(r => r.Field<int>("monthly"));
                                        else if (grpBy.ToLower() == "operator")
                                            gp = ds.AsEnumerable().GroupBy(r => r.Field<string>("operator"));
                                        else if (grpBy.ToLower() == "operator_type")
                                            gp = ds.AsEnumerable().GroupBy(r => r.Field<string>("operator_type"));

                                        List<object> iData = new List<object>();
                                        //group entries into specific record by ReportColumnType
                                        foreach (var entry in gp)
                                        {
                                            //DataRow newRw = newDT.NewRow();
                                            Dictionary<string, string> iRow = new Dictionary<string, string>();
                                            string indexValue = "";
                                            //foreach (DataRow row in ds.Rows)
                                            //{
                                            //    if (i >= ds.Rows.Count) break;

                                            //    indexValue = row.ItemArray.ToList().ElementAt(index).ToString();

                                            //    ////primary key for this record
                                            //    //if (grpBy.ToLower() == "monthly")
                                            //    //{
                                            //    //    if (fields.ContainsKey(entry.Key.ToString() + "'20"))
                                            //    //        iRow.Add("entity - " + indexValue, fields[entry.Key.ToString() + "'20"]);
                                            //    //    else
                                            //    //        iRow.Add("entity - " + indexValue, MonthField(entry.Key.ToString(), true) + "'20");
                                            //    //}
                                            //    //else
                                            //    //    iRow.Add("entity - " + indexValue, entry.Key.ToString());

                                            //    i++;
                                            //}
                                            // primary key for this record
                                            if (grpBy.ToLower() == "monthly")
                                            {
                                                if (fields.ContainsKey(entry.Key.ToString() + "'20"))
                                                    iRow.Add("entity", fields[entry.Key.ToString() + "'20"]);
                                                else
                                                    iRow.Add("entity", MonthField(entry.Key.ToString(), true) + "'20");
                                            }
                                            else
                                                iRow.Add("entity", entry.Key.ToString());
                                            //newRw[grpBy.ToLower()] = entry.ToString();

                                            //add record to this row
                                            foreach (DataRow iEntry in entry)
                                            {
                                                indexValue = iEntry["Year"].ToString();
                                                if (!string.IsNullOrEmpty(indexValue))
                                                    indexValue = " - " + indexValue;

                                                string curColumn = "";
                                                if (iEntry.ItemArray.Contains(grpBy)) curColumn = iEntry[grpBy].ToString(); else curColumn = iEntry["Operator"].ToString();

                                                //if (RowFields.Contains(curColumn))
                                                //{
                                                //row header and value
                                                iRow.Add(curColumn + indexValue, iEntry["ReportValue"].ToString());

                                                //newRw[curColumn] = iEntry["ReportValue"].ToString();
                                                //}
                                            }

                                            //add row to table/object
                                            iData.Add(iRow);
                                            //newDT.Rows.Add(newRw);
                                        }
                                        FormEntry = iData;
                                        //FormEntryTable = newDT;
                                    }
                                }
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorMsg += "LoadReport() " + ex.Message + "";
                return false;
            }
        }

    }
}

