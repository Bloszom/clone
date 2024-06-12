using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using _FrameworkCore;
using Newtonsoft.Json;

namespace pceaLibrary
{
    public class UserMgt : _Database
    {
        public string GetUserAssignedPrivileges(string sRoleId)
        {
            try
            {
                using (SqlCommand objCmd = new SqlCommand())
                {
                    objCmd.CommandType = CommandType.StoredProcedure;
                    objCmd.CommandText = "sp_getuser_privileges";
                    objCmd.Parameters.Clear();
                    objCmd.Parameters.AddWithValue("@role_id", sRoleId);
                    using (var _dt = ExecuteDataTable(objCmd))
                    {
                        if (_dt != null)
                        {
                            return DataTableToJsonWithJsonNet(_dt);
                        }

                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                FriendlyErrorMessage += "Unable to generate User Privileges";
                TechnicalErrorMessage += ex.Message;
                return string.Empty;
            }
        }

        public int SaveUserAssignedPrivileges(string sRoleId, string sPrivileges)
        {
            try
            {
                string sDeleteSQL = "DELETE FROM [dbo].[UserPrivilege] WHERE [RoleId]=@RoleId ";
                string sInsertSQL = "INSERT INTO [dbo].[UserPrivilege] ([RoleId], [PrivilegeId]) ";
                string sValueClause = string.Empty;
                string[] _Privileges = sPrivileges.Split('-');
                int i = 0;
                using (SqlCommand objCmd = new SqlCommand())
                {
                    objCmd.CommandType = CommandType.Text;
                    objCmd.CommandText = sDeleteSQL;
                    objCmd.Parameters.Clear();
                    objCmd.Parameters.AddWithValue("@RoleId", sRoleId);
                    if (ExecuteNonQuery(objCmd) == false)
                    {
                        FriendlyErrorMessage += "Unable to delete previously assigned privileges. Contact DB Administrator.";
                        return 0;
                    }

                    objCmd.Parameters.Clear();
                    for (i = 0; i < _Privileges.Length; i++)
                    {
                        sValueClause += " VALUES(@RoleId, @PrivilegeId" + i.ToString() + ")";
                        objCmd.Parameters.AddWithValue("@PrivilegeId" + i.ToString(), _Privileges[i]);
                    }
                    objCmd.Parameters.AddWithValue("@RoleId", sRoleId);
                    objCmd.CommandText = sInsertSQL + sValueClause;
                    if (ExecuteNonQuery(objCmd)==false)
                    {
                        FriendlyErrorMessage += "Unable to insert newly assigned privileges. Contact DB Administrator.";
                        return 0;
                    }
                }
                return i;
            }
            catch (Exception ex)
            {
                FriendlyErrorMessage += "Unable to save Privileges";
                TechnicalErrorMessage += ex.Message;
                return 0;
            }
        }

        private string DataTableToJsonWithJsonNet(DataTable table)
        {
            string jsonString = string.Empty;
            jsonString = JsonConvert.SerializeObject(table);
            return jsonString;
        }

        public string GetNewUserId(string sPrefix)
        {
            try
            {
                string sUserId = sPrefix.ToUpper() + DateTime.Now.ToFileTime().ToString();
                using (SqlCommand objCmd = new SqlCommand())
                {
                    objCmd.CommandType = CommandType.Text;
                    objCmd.CommandText = "SELECT ISNULL(COUNT(*),0) AS total FROM [UserProfile] WHERE [UserId]=@UserId ";
                    objCmd.Parameters.Clear();
                    objCmd.Parameters.AddWithValue("@UserId", sUserId);
                    using (var _dt = ExecuteDataTable(objCmd))
                    {
                        if (int.Parse(_dt.Rows[0]["total"].ToString()) > 0)
                        {
                            sUserId = GetNewUserId(sPrefix);
                        }
                        return sUserId;
                    }
                }
            }
            catch (Exception ex)
            {
                FriendlyErrorMessage += "Unable to generate new User Id";
                TechnicalErrorMessage += ex.Message;
                return string.Empty;
            }
        }




    }
}
