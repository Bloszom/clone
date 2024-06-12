using Microsoft.AspNetCore.Mvc;
using pcea.Models;
using pceaLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pcea.Helpers
{
    public class ReportHelper
    {
        private readonly PceaDbContext _DbContext;
        Report _Report;


        public ReportHelper(PceaDbContext dbContext)
        {
            _DbContext = dbContext;
            _Report = new Report();
        }

        /// <summary>
        /// Gets the report data from the database
        /// </summary>
        /// <param name="reportId"></param>
        /// <param name="formId"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns>A dictionary of a DataTable loaded with the required report data as key and a string array of the report template fields as value</returns>
        private Dictionary<DataTable, string[]> GetDataTable(long reportId, int formId, DateTime from, DateTime to)
        {
            string msg = "";
            Dictionary<DataTable, string[]> returnVal = new Dictionary<DataTable, string[]>();
            string fields = " AND ";
            string groupBy = " Group By dbo.FormsEntry.FieldLabel, dbo.FormsAndEntry.OrganizationName,dbo.FormsEntry.FieldName,dbo.FormsEntry.Response,dbo.FormsEntry.ValueYear,dbo.FormsEntry.FrmYear";
            string reportSql = "SELECT OrganizationName, FieldLabel, Response, ValueYear, FrmYear FROM dbo.FormsAndEntry LEFT JOIN [dbo].[FormsEntry] ON[dbo].[FormsEntry].EntryId = dbo.FormsAndEntry.EntryId WHERE dbo.FormsAndEntry.Status = 'SUBMITTED' AND FormsType = 'OPERATOR_TYPE' AND AppType = 'NEW'";
            string OrderBy = "ORDER BY FrmYear DESC";
            string form = "";

            //Populating a DataTable from database.
            var dt = new DataTable();

            if (formId > 0)
                form += " AND dbo.FormsAndEntry.RecId = " + formId;
            try
            {
                var report = _DbContext.ReportTemplate.Where(x => x.ReportId == reportId).FirstOrDefault();
                //var test = report.FormFields.Replace("\r\n", string.Empty).Replace("\n", "").Trim(char.Parse(", ")).Replace(", ", ",").Split(",");
                var templateFields = report.FormFields.Replace("\r\n", string.Empty).Replace("\n", "").Trim().Split(",");
                int Y = 0;
                foreach (var field in templateFields)
                {
                    if (Y <= 0)
                        fields += "dbo.FormsEntry.FieldLabel = " + "'" + field.Trim() + "'";
                    else
                        fields += " OR dbo.FormsEntry.FieldLabel = " + "'" + field.Trim() + "'";
                    Y++;
                }

                string dateFrom = from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                string dateTo = to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                if (report == null)
                {
                    return returnVal;
                }

                if (from != DateTime.Parse("01/01/0001 00:00:00") && to != DateTime.Parse("01/01/0001 00:00:00"))
                {
                    dt = _Report.ExecuteDataTable(reportSql + " AND " + " DateSubmitted >= " + " '" + dateFrom + "' " + " AND " + " DateSubmitted <= " + " '" + dateTo + "' " + OrderBy);
                }
                else
                {
                    dt = _Report.ExecuteDataTable(reportSql + form + fields + groupBy);
                    //dt = _DbContext.FormsEntry.FromSqlRaw(report.ReportSQL)
                }

                if(dt != null)
                    returnVal.Add(dt, templateFields);
            }
            catch (Exception ex)
            {
                msg += "Error: " + ex.Message;
            }
            return returnVal;

        }

        /// <summary>
        /// Defines the first report template. 
        /// </summary>
        /// <param name="reportId"></param>
        /// <param name="formId"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns>returns an html table loaded with the report data. Generates a table with the input labels as the columns and operator name with values on the rows</returns>
        public string ReportTemp1(long reportId, int formId, DateTime from, DateTime to)
        {
            string msg = "";
            StringBuilder html = new StringBuilder();
            var rpt = _DbContext.ReportTemplate.FirstOrDefault(e => e.ReportId == reportId);

            try
            {
                Dictionary<DataTable, string[]> _data = GetDataTable(reportId, formId, from, to);
                List<string> years = new List<string>();
                //Building an HTML string.
                if (_data.Count > 0)
                {

                    DataTable dt = _data.Keys.FirstOrDefault();
                    var templateFields = _data.Values.FirstOrDefault();

                    //extract data from datatable
                    Dictionary<string, string> body = new Dictionary<string, string>();
                    var operators = new List<string>();

                    //extract all the form fields from datatabble to be made datatable headers
                    //also extract the values into a collection to form the row
                    int t = -1;
                    foreach (DataRow item in dt.Rows)
                    {
                        t++;
                        var label = dt.Rows[t]["FieldLabel"].ToString();
                        var operatorId = dt.Rows[t]["OrganizationName"].ToString();
                        var Value = dt.Rows[t]["Response"].ToString();
                        var year = dt.Rows[t]["ValueYear"].ToString();
                        if (!years.Contains(year)) years.Add(year);
                        operators.Add(!operators.Contains(operatorId) ? operatorId : "");
                        if (templateFields.Any(e => e.Contains(label)))
                        {
                            if (!body.Keys.Contains(label))
                                if(rpt.TableType == 1)
                                    body.Add(label, operatorId + "," + (string.IsNullOrEmpty(Value) ? "N/A" : Value));
                                else
                                    body.Add(label, operatorId + "," + (string.IsNullOrEmpty(Value) ? "0" : Value));
                            else
                                if (rpt.TableType == 1)
                                    body.Add(label + "%" + t, operatorId + "," + (string.IsNullOrEmpty(Value) ? "N/A" : Value));
                                else
                                    body.Add(label + "%" + t, operatorId + "," + (string.IsNullOrEmpty(Value) ? "0" : Value));
                        }
                    }
                    operators.Remove("");
                    dt.Columns.Clear();
                    dt.Rows.Clear();
                    operators = operators.Where(e => !string.IsNullOrEmpty(e.ToString())).ToList();
                    //foreach (DataRow item in dt.Rows)
                    //{
                    //    //if(item.ColumnName.ToLower() == "fieldlabel")
                    //    //{
                    //        foreach (var itm in report.FormFields.Split(","))
                    //        {
                    //            //var value = dt.Rows[item.ColumnName]

                    //        }
                    //    //}
                    //}

                    //Table start.
                    //html.Append("<table id='tblExport' class='table table-striped table-hover'>");
                    html.Append("<table id='indexDataTableBasic' class='table text-center no-wrap table-border table-sm table-hover'>");

                    //Building the Header row.
                    html.Append("<thead>");
                    html.Append("<tr>");
                    List<string> hds = new List<string>();
                    //foreach (var item in report.FormFields.Split(","))
                    //{
                    //    hds.Add(item);
                    //    //foreach(DataColumn column in dt.Columns)
                    //    //{
                    //    //    if (column.ColumnName.Contains(item))
                    //    //        hds.Add(column);
                    //    //}
                    //}
                    int o = 0;
                    //if type is 1 then use normal table presentation
                    if (rpt.TableType == 1)
                    {
                        foreach (var column in templateFields)
                        {

                            if (o <= 0)
                            {
                                DataColumn data1 = new DataColumn();
                                data1.ColumnName = " ";
                                dt.Columns.Add(data1);
                                html.Append("<th class='border'></th>");
                                o++;
                            }
                            DataColumn data = new DataColumn();
                            data.ColumnName = column;
                            dt.Columns.Add(data);
                            html.Append("<th class='border'>");
                            html.Append(column);
                            html.Append("</th>");
                        }
                        html.Append("</tr>");
                        html.Append("</thead>");

                        foreach (var item in operators)
                        {
                            DataRow rw = dt.NewRow();
                            dt.Rows.Add(rw);
                        }
                        int r = 0;

                        foreach (var item in operators)
                        {
                            int c = 0;
                            foreach (DataColumn dataColumn in dt.Columns)
                            {

                                var val = body.FirstOrDefault(e => e.Key.StartsWith(dataColumn.ColumnName) && e.Value.StartsWith(item)).Value;
                                if (c <= 0)
                                {
                                    dt.Rows[r][dataColumn.ColumnName] = item;
                                    c++;
                                }
                                else
                                    dt.Rows[r][dataColumn.ColumnName] = string.IsNullOrEmpty(val) ? "N/A" : val.Substring(val.IndexOf(",") + 1);
                            }
                            r++;
                        }
                        html.Append("<tbody>");

                        foreach (DataRow row in dt.Rows)
                        {
                            html.Append("<tr>");
                            foreach (DataColumn column in dt.Columns)
                            {
                                html.Append("<td class='border'>");
                                html.Append(row[column.ColumnName]);
                                html.Append("</td>");
                            }
                            html.Append("</tr>");
                        }
                    }
                    //if type is 2 or 3 then use second table presentation, switches columns with rows
                    else if(rpt.TableType <= 4)
                    {
                        int index = 0;
                        foreach (var column in years)
                        {
                            if(index <= 0)
                            {
                                DataColumn column1 = new DataColumn();
                                column1.ColumnName = "";
                                dt.Columns.Add(column1);
                                html.Append("<th class='border'>");
                                html.Append("");
                                html.Append("</th>");
                                index++;
                            }
                            if (!string.IsNullOrEmpty(column))
                            {
                                DataColumn col = new DataColumn();
                                col.ColumnName = column;
                                dt.Columns.Add(col);
                                html.Append("<th class='border'>");
                                html.Append(column);
                                html.Append("</th>");
                            }
                        }

                        if(rpt.TableType == 3)
                        {
                            DataColumn col = new DataColumn();
                            col.ColumnName = "Average";
                            dt.Columns.Add(col);
                            html.Append("<th class='border'>");
                            html.Append("Average");
                            html.Append("</th>");
                        }
                        if(rpt.TableType == 4)
                        {
                            DataColumn col = new DataColumn();
                            col.ColumnName = "TOTAL";
                            dt.Columns.Add(col);
                            html.Append("<th class='border'>");
                            html.Append("TOTAL");
                            html.Append("</th>");
                        }
                        html.Append("</tr>");
                        html.Append("</thead>");

                        foreach (var item in operators)
                        {
                            DataRow rw = dt.NewRow();
                            dt.Rows.Add(rw);
                        }

                        int r = 0;
                        
                        foreach (var item in operators)
                        {
                            int c = 0;
                            foreach (DataColumn dataColumn in dt.Columns)
                            {
                                var test = body.Where(e => e.Key.Contains(dataColumn.ColumnName) && e.Value.StartsWith(item)).Select(sl => long.Parse( sl.Value.Substring(sl.Value.IndexOf(",") + 1))).ToList().Sum();
                                if (c <= 0)
                                {
                                    dt.Rows[r][dataColumn.ColumnName] = item;
                                    c++;
                                }
                                else
                                    dt.Rows[r][dataColumn.ColumnName] = test;
                            }
                            r++;

                        }

                        long sumPrev = 0;
                        long prevCol = 0;
                        long currCol = 0;
                        long sumCurr = 0;
                        int sum = 1;

                        foreach (DataColumn dc in dt.Columns)
                        {
                            sum = 1;
                            int count = 0;
                            foreach (DataRow drow in dt.Rows)
                            {
                                count++;
                                prevCol = long.Parse(drow[sum].ToString());
                                currCol = long.Parse(drow[sum + 1].ToString());
                                sumPrev += long.Parse(drow[sum].ToString());
                                sumCurr += long.Parse(drow[sum + 1].ToString());

                                if (count == dt.Rows.Count && rpt.TableType == 2)
                                {
                                    dt.Rows.Add("TOTAL", sumPrev, sumCurr);
                                    break;
                                }
                                
                                
                                if(rpt.TableType == 3)
                                {
                                    var SUM = prevCol + currCol;
                                    drow[sum + 2] = SUM / templateFields.Length;
                                    prevCol = 0;
                                    currCol = 0;
                                }else if(rpt.TableType == 4)
                                {
                                    var SUM = prevCol + currCol;
                                    drow[sum + 2] = SUM;
                                    prevCol = 0;
                                    currCol = 0;
                                }

                                if (count == dt.Rows.Count && rpt.TableType == 4)
                                {
                                    dt.Rows.Add("TOTAL", sumPrev, sumCurr, sumPrev + sumCurr);
                                    break;
                                }
                            }
                            break;
                        }

                        html.Append("<tbody>");

                        foreach (DataRow row in dt.Rows)
                        {

                            html.Append("<tr>");
                            foreach (DataColumn column in dt.Columns)
                            {
                                html.Append("<td class='border'>");
                                html.Append(row[column.ColumnName]);
                                html.Append("</td>");
                            }
                            html.Append("</tr>");
                        }
                    }
                    else
                    {
                        int index = 0;
                        foreach (var column in operators)
                        {
                            if (index <= 0)
                            {
                                DataColumn column1 = new DataColumn();
                                column1.ColumnName = "";
                                dt.Columns.Add(column1);
                                html.Append("<th class='border'>");
                                html.Append("");
                                html.Append("</th>");
                                index++;
                            }
                            if (!string.IsNullOrEmpty(column))
                            {
                                DataColumn col = new DataColumn();
                                col.ColumnName = column;
                                dt.Columns.Add(col);
                                html.Append("<th class='border'>");
                                html.Append(column);
                                html.Append("</th>");
                            }
                        }

                            DataColumn colextra = new DataColumn();
                            colextra.ColumnName = "Average";
                            dt.Columns.Add(colextra);
                            html.Append("<th class='border'>");
                            html.Append("Average");
                            html.Append("</th>");
                        html.Append("</tr>");
                        html.Append("</thead>");

                        foreach (var item in templateFields)
                        {
                            DataRow rw = dt.NewRow();
                            dt.Rows.Add(rw);
                        }

                        int r = 0;

                        foreach (var item in templateFields)
                        {
                            int c = 0;
                            foreach (DataColumn dataColumn in dt.Columns)
                            {
                                var test = body.Where(e => e.Key.Contains(dataColumn.ColumnName) && e.Value.StartsWith(item)).Select(sl => long.Parse(sl.Value.Substring(sl.Value.IndexOf(",") + 1))).ToList().Sum();
                                if (c <= 0)
                                {
                                    dt.Rows[r][dataColumn.ColumnName] = item;
                                    c++;
                                }
                                else
                                    dt.Rows[r][dataColumn.ColumnName] = test;
                            }
                            r++;

                        }

                        long sumPrev = 0;
                        string prevCol = "";
                        string currCol = "";
                        long sumCurr = 0;
                        int sum = 1;

                        foreach (DataColumn dc in dt.Columns)
                        {
                            sum = 1;
                            int count = 0;
                            foreach (DataRow drow in dt.Rows)
                            {
                                count++;

                                sumPrev += long.Parse(drow[sum].ToString());
                                sumCurr += long.Parse(drow[sum + 1].ToString());

                                if (count == dt.Rows.Count && rpt.TableType == 2)
                                {
                                    dt.Rows.Add("TOTAL", sumPrev, sumCurr);
                                    break;
                                }
                                else if (rpt.TableType == 3)
                                {
                                    var SUM = sumPrev + sumCurr;
                                    drow[dt.Columns.Count + 2] = SUM / dt.Columns.Count - 2;
                                    sumPrev = 0;
                                    sumCurr = 0;
                                }
                            }
                            break;
                        }

                        html.Append("<tbody>");

                        foreach (DataRow row in dt.Rows)
                        {

                            html.Append("<tr>");
                            foreach (DataColumn column in dt.Columns)
                            {
                                html.Append("<td class='border'>");
                                html.Append(row[column.ColumnName]);
                                html.Append("</td>");
                            }
                            html.Append("</tr>");
                        }
                    }
                    
                    //html.Append("</tbody>");

                    //Table end.
                    html.Append("</table>");
                    html.Append("<div class='text-right mt-2 pt-2'>");
                    html.Append("<button class='btn btn-primary btn-sm chart' id='btnExport' type='button'>");
                    html.Append("<i class='fa fa-save'>");
                    html.Append("</i>");
                    html.Append(" Export Report");
                    html.Append("</button>");
                    html.Append("</div>");
                }
                else
                {
                    html.Append("No data...");
                    return html.ToString();
                }

            }
            catch (Exception ex)
            {
                msg += "Error: " + ex.Message;
            }
            return html.ToString();
        }
    }
}
