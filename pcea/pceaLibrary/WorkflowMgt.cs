using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using _FrameworkCore;

namespace pceaLibrary
{
    public class WorkflowMgt: _Database
    {
        public string GetNewTaskId(string sProcessId)
        {
            using (SqlCommand objCmd = new SqlCommand())
            {
                string sNewTaskId = DateTime.Now.ToString("yyyyMMddHHmmssff");
                try
                {
                    objCmd.CommandType = CommandType.StoredProcedure;
                    objCmd.CommandText = "sp_getnew_taskid";
                    objCmd.Parameters.Clear();
                    objCmd.Parameters.AddWithValue("@process_id", sProcessId);
                    using (var _dt = ExecuteDataTable(objCmd))
                    {
                        if (_dt != null)
                        {
                             sNewTaskId = _dt.Rows[0][0].ToString();
                        }
                        return sNewTaskId;
                    }
                }   
                catch (Exception ex)
                {
                    return sNewTaskId;
                }
            }
        }

        public string TaskId;
        public string ProcessId;
        public string UserId;
        public string UserType;
        public string OrganizationName;
        public string RoleId;
        public string Telephone;
        public string Email;
        public bool VerifyTask()
        {
            try
            {
                using (SqlCommand objCmd = new SqlCommand())
                {
                    objCmd.CommandType = CommandType.Text;
                    objCmd.CommandText = "SELECT * FROM [TaskId] WHERE [TaskId]=@TaskId ";
                    objCmd.Parameters.Clear();
                    objCmd.Parameters.AddWithValue("@TaskId", TaskId);
                    using (var _dt = ExecuteDataTable(objCmd))
                    {
                        if (_dt == null)
                        {
                            FriendlyErrorMessage += "Invalid Task Id.  Operation aborted.";
                            return false;
                        }
                        if(_dt.Rows[0]["ProcessId"].ToString() != ProcessId)
                        {
                            FriendlyErrorMessage += "Invalid Process Id.  Operation aborted.";
                            return false;
                        }
                    }
                    objCmd.CommandText = "SELECT * FROM [UserProfile] WHERE [UserId]=@UserId ";
                    objCmd.Parameters.Clear();
                    objCmd.Parameters.AddWithValue("@UserId", UserId);
                    using (var _dt = ExecuteDataTable(objCmd))
                    {
                        if (_dt == null)
                        {
                            FriendlyErrorMessage += "Invalid Destination User selected.  Operation aborted.";
                            return false;
                        }
                        UserType = _dt.Rows[0]["UserType"].ToString();
                        RoleId = _dt.Rows[0]["RoleId"].ToString();
                        Email = _dt.Rows[0]["Email"].ToString();
                        Telephone = _dt.Rows[0]["Telephone"].ToString();
                        OrganizationName = _dt.Rows[0]["OrganizationName"].ToString();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                FriendlyErrorMessage += "Unable to validate Task";
                TechnicalErrorMessage += ex.Message;
                return false;
            }
        }







        public int GetPendingTasksCount(string sUserId)
        {
            try
            {
                using (SqlCommand objCmd = new SqlCommand())
                {
                    objCmd.CommandType = CommandType.StoredProcedure;
                    objCmd.CommandText = "sp_getnew_taskid";
                    objCmd.Parameters.Clear();
                    objCmd.Parameters.AddWithValue("@process_id", "");
                    using (var _dt = ExecuteDataTable(objCmd))
                    {
                        if (_dt != null)
                        {
                            string sNewTaskId = _dt.Rows[0][0].ToString();
                            if (int.Parse(sNewTaskId) < 1)  //returns 0 if unable to generate new TaskId
                            {
                                FriendlyErrorMessage += "Unable to generate Task Id";
                                return 0;
                            }
                            else
                            {
                                return 1;
                            }
                        }
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                FriendlyErrorMessage += "Unable to generate Task Id";
                TechnicalErrorMessage += ex.Message;
                return 0;
            }
        }




    }
}
