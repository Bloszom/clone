using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using _FrameworkCore;
using Microsoft.AspNetCore.Http;

namespace pceaLibrary
{
    public class Vars : _Database
    {
        HttpContext _httpContext;
        private readonly string SystemConfigSQL = "SELECT * FROM [dbo].[SystemConfig]";

        public string SurveyReviewBase { get; set; }
        public string TariffReviewBase { get; set; }
        public Vars(HttpContext httpContext)
        {
            _httpContext = httpContext;
        }

        public enum MsgType
        {
            Error = 0,
            Success = 1,
            Warning = 2
        }

        public enum DefaultTaskAction
        {
            REVIEW
        }

        public enum UserRole
        {
            DEFAULT
        }

        public enum Organization
        {
            NCC,
            OPR
        }

        public enum MetaData
        {
            WORKFLOW_ACTION
        }
        
        public enum FormTypes
        {
            TARIFF_TYPE,
            OPERATOR_TYPE,
            OTHER_SERVICE,
            ARCHIVE
        }
        
        public enum AdminUsersType
        {
            NCC
        }

        public static Dictionary<string, string> UserTypes = new Dictionary<string, string>
        {
            { "OPERATOR", "PUBLIC" },
            { "ADMIN", "PRIVATE" }
        };
        
        public static Dictionary<string, string> AdminControllers = new Dictionary<string, string>
        {
            { "FORMS", "FORMS" },
            { "APPROLES", "APPROLES" },
            { "MEMOS", "MEMOS" },
            { "METADATAS", "METADATAS" },
            { "REPORT", "REPORT" },
            { "USERPROFILES", "USERPROFILES" },
            { "WORKFLOWS", "WORKFLOWS" },
            { "WORKFLOWMANAGER", "WORKFLOWMANAGER" }
        };

        public static Dictionary<string, string> Status = new Dictionary<string, string>
        {
            { "ACTIVE", "ACTIVE" },
            { "INACTIVE", "INACTIVE" },
            { "SUSPENDED", "SUSPENDED" }
        };

        public static Dictionary<string, string> UserStatus = new Dictionary<string, string>
        {
            { "INVITED", "INVITED" },
            { "ENABLED", "ENABLED" },
            { "DISABLED", "DISABLED" },
            { "SUSPENDED", "SUSPENDED" },
            { "UNDETERMINED", "UNDETERMINED" }
        };

        public static Dictionary<string, string> MailType = new Dictionary<string, string>
        {
            { "SURVEY_NOTIFICATION", "SURVEY_NOTIFICATION" },
            { "WORKFLOW_NOTIFICATION", "WORKFLOW_NOTIFICATION" },
            { "OTHER_SERVICE_NOTIFICATION", "OTHER_SERVICE_NOTIFICATION" },
            { "NEW_TASK_NOTIFICATION", "TASK_NOTIFICATION" },
            { "TARIFF_REQUEST_NOTIFICATION", "TARIFF_REQUEST_NOTIFICATION" },
            { "NEW_TASK_BROADCAST", "TASK_BROADCAST" }
        };

        public static Dictionary<string, string> TaskTypes = new Dictionary<string, string>
        {
            { "NEW", "NEW" },
            { "PENDING", "PENDING" },
            { "COMPLETED", "COMPLETED" },
            { "UNASSIGNED", "UNASSIGNED" }
        };

        public static Dictionary<string, string> MetaDataTypes = new Dictionary<string, string>
        {
            { "MEMO", "MEMO_TYPE" }
        };

        public static Dictionary<string, string> ReviewStatus = new Dictionary<string, string>
        {
            { "APPROVED", "APPROVED" },
            { "REJECTED", "REJECTED" },
            { "PENDING", "PENDING" }
        };

        public static Dictionary<string, string> NotificationActionTypes = new Dictionary<string, string>
        {
            { "SUBMIT", "SUBMIT" },
            { "REJECT", "REJECT" },
            { "APPROVE", "APPROVE" }
        };

        /// <summary>
        /// Setup environment and log session variables for the operators
        /// </summary>
        /// <param name="sUserId"></param>
        /// <param name="sFullname"></param>
        /// <returns>True if successful. False if error</returns>
        public bool LogOperator(string sUserId, string sFullname)   //non-SSO login
        {
            try
            {
                using (SqlCommand objCmd = new SqlCommand())
                {
                    DataTable dt = new DataTable();
                   
                    //get SystemConfig into Session Variables
                    objCmd.CommandText = SystemConfigSQL;
                    dt = ExecuteDataTable(objCmd);
                    if (dt == null)
                    {
                        FriendlyErrorMessage = "Error getting system configuration.  Contact Administrator.";
                        return false;
                    }
                    string sDocPath = dt.Rows[0]["DocumentPath"].ToString();
                    string sImagePath = dt.Rows[0]["ImagePath"].ToString();
                    string sSsoAppHostMaster = dt.Rows[0]["SsoAppHost"].ToString();

                    //load user profile
                    dt = null;
                    objCmd.CommandText = "SELECT * FROM UserProfile WHERE UserId=@UserId";
                    objCmd.Parameters.Clear();
                    objCmd.Parameters.AddWithValue("@UserId", sUserId);
                    objCmd.CommandTimeout = 120;
                    dt = ExecuteDataTable(objCmd);
                    if (dt == null)
                    {
                        FriendlyErrorMessage = "Error getting user information.  Contact Administrator.";
                        return false;
                    }
                    string sOperatorId = dt.Rows[0]["OrganizationId"].ToString();
                    string sOperatorName = dt.Rows[0]["OrganizationName"].ToString();
                    string sRoleId = dt.Rows[0]["RoleId"].ToString();
                    
                    DocumentPath = sDocPath;
                    ImagePath = sImagePath;
                    SSOAppHostMaster = sSsoAppHostMaster;
                    UserType = UserTypes["OPERATOR"];
                    UserId = sUserId;
                    FullName = sFullname;
                    OperatorId = sOperatorId;
                    OperatorName = sOperatorName;
                    RoleId = sRoleId;
                }
                return true;
            }
            catch (Exception ex)
            {
                FriendlyErrorMessage = "Error getting or setting system configuration.  Contact Administrator.";
                TechnicalErrorMessage = ex.Message + "\r\n" + ex.InnerException;
                return false;
            }
        }
        public bool LogOperator(string sUserId, string sFullname, string sOperatorId, string sOperatorName, string sRoleId, string sImageUrl)   //SSO login
        {
            try
            {
                using (SqlCommand objCmd = new SqlCommand())
                {
                    DataTable dt = new DataTable();

                    //get SystemConfig into Session Variables
                    objCmd.CommandText = SystemConfigSQL;
                    dt = ExecuteDataTable(objCmd);
                    if (dt == null)
                    {
                        FriendlyErrorMessage = "Error getting system configuration.  Contact Administrator.";
                        return false;
                    }
                    string sDocPath = dt.Rows[0]["DocumentPath"].ToString();
                    string sImagePath = dt.Rows[0]["ImagePath"].ToString();
                    string sSsoAppHostMaster = dt.Rows[0]["SsoAppHost"].ToString();
                    string _TariffBaseUrl = dt.Rows[0]["TariffReviewBase"].ToString();
                    string _SurveyBaseUrl = dt.Rows[0]["SurveyReviewBase"].ToString();

                    DocumentPath = sDocPath;
                    ImagePath = sImagePath;
                    UserType = UserTypes["OPERATOR"];
                    UserId = sUserId;
                    FullName = sFullname;
                    OperatorId = sOperatorId;
                    OperatorName = sOperatorName;
                    RoleId = sRoleId;
                    ImageUrl = sImageUrl;
                    SSOAppHostMaster = sSsoAppHostMaster;
                    SurveyReviewBase = _SurveyBaseUrl;
                    TariffReviewBase = _TariffBaseUrl;

                }
                return true;
            }
            catch (Exception ex)
            {
                FriendlyErrorMessage = "Error getting or setting system configuration.  Contact Administrator.";
                TechnicalErrorMessage = ex.Message + "\r\n" + ex.InnerException;
                return false;
            }
        }

        /// <summary>
        /// Setup environment and log session variables for the operators
        /// </summary>
        /// <param name="sUserId"></param>
        /// <param name="sFullname"></param>
        /// <returns>True if successful. False if error</returns>
        public bool LogAdministrator(string sUserId, string sFullname, string sRoleId)
        {
            try
            {
                using (SqlCommand objCmd = new SqlCommand())
                {
                    DataTable dt = new DataTable();

                    //get SystemConfig into Session Variables
                    objCmd.CommandText = SystemConfigSQL;
                    dt = ExecuteDataTable(objCmd);
                    if (dt == null)
                    {
                        FriendlyErrorMessage = "Error getting system configuration.  Contact Administrator.";
                        return false;
                    }
                    string sDocPath = dt.Rows[0]["DocumentPath"].ToString();
                    string sImagePath = dt.Rows[0]["ImagePath"].ToString();
                    string sSsoAppHostMaster = dt.Rows[0]["SsoAppHost"].ToString();
                    DocumentPath = sDocPath;
                    ImagePath = sImagePath;
                    UserType = UserTypes["ADMIN"];
                    UserId = sUserId;
                    FullName = sFullname;
                    RoleId = sRoleId;
                    SSOAppHostMaster = sSsoAppHostMaster;
                    PendingTaskCount = CountPendingTask(sUserId);
                }
                return true;
            }
            catch (Exception ex)
            {
                FriendlyErrorMessage = "Error getting or setting system configuration.  Contact Administrator.";
                TechnicalErrorMessage = ex.Message + "\r\n" + ex.InnerException;
                return false;
            }
        }
        public string CountPendingTask(string sUserId)
        {
            try
            {
                string sTaskCount = "0";
                using (SqlCommand objCmd = new SqlCommand())
                {
                    DataTable dt = new DataTable();

                    //get SystemConfig into Session Variables
                    objCmd.CommandText = "SELECT ISNULL(COUNT(*), '0') AS TaskCount FROM [dbo].[WorkflowManager] " +
                        "WHERE UserId=@UserId AND TaskType='PENDING' AND IsSource = 'false'";
                    objCmd.Parameters.Clear();
                    objCmd.Parameters.AddWithValue("@UserId", sUserId);
                    objCmd.CommandTimeout = 120;
                    dt = ExecuteDataTable(objCmd);
                    if (dt == null)
                    {
                        FriendlyErrorMessage = "Error getting system configuration.  Contact Administrator.";
                        return "0";
                    }
                    sTaskCount = dt.Rows[0]["TaskCount"].ToString();
                }
                return sTaskCount;
            }
            catch (Exception ex)
            {
                FriendlyErrorMessage = "Error getting pending tasks.  Contact Administrator.";
                TechnicalErrorMessage = ex.Message + "\r\n" + ex.InnerException;
                return "0";
            }
        }


        #region "Session Variable Properties"
        public string UserType
        {
            set
            {
                try
                {
                    LogSession(GetCurrentToken(_httpContext), "user_type", value);
                    _httpContext.Session.Set("user_type", Encoding.ASCII.GetBytes(value));
                }
                catch (Exception ex)
                {
                    TechnicalErrorMessage = ex.Message;
                }
            }
            get
            {
                string sValue;
                byte[] bValue;
                try
                {
                    _httpContext.Session.TryGetValue("user_type", out bValue);
                    sValue = Encoding.ASCII.GetString(bValue);
                    if (string.IsNullOrEmpty(sValue) == true)
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("user_type", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    return sValue;
                }
                catch (Exception)
                {
                    try
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("user_type", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    catch (Exception)
                    {
                        //HttpContext.Current.Response.Redirect("~/signin?session=U"); 
                        //redirect to error page
                    }
                    return string.Empty;
                }
            }
        }
        public string UserId
        {
            set
            {
                try
                {
                    LogSession(GetCurrentToken(_httpContext), "user_id", value);
                    _httpContext.Session.Set("user_id", Encoding.ASCII.GetBytes(value));
                }
                catch (Exception)
                {
                }
            }
            get
            {
                string sValue;
                byte[] bValue;
                try
                {
                    _httpContext.Session.TryGetValue("user_id", out bValue);
                    sValue = Encoding.ASCII.GetString(bValue);
                    if (string.IsNullOrEmpty(sValue) == true)
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("user_id", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    return sValue;
                }
                catch (Exception)
                {
                    try
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("user_id", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    catch (Exception)
                    {
                        //HttpContext.Current.Response.Redirect("~/signin?session=U"); 
                        //redirect to error page
                    }
                    return string.Empty;
                }
            }
        }
        public string RoleId
        {
            set
            {
                try
                {
                    LogSession(GetCurrentToken(_httpContext), "role_id", value);
                    _httpContext.Session.Set("role_id", Encoding.ASCII.GetBytes(value));
                }
                catch (Exception ex)
                {
                    TechnicalErrorMessage = ex.Message;
                }
            }
            get
            {
                string sValue;
                byte[] bValue;
                try
                {
                    _httpContext.Session.TryGetValue("role_id", out bValue);
                    sValue = Encoding.ASCII.GetString(bValue);
                    if (string.IsNullOrEmpty(sValue) == true)
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("role_id", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    return sValue;
                }
                catch (Exception)
                {
                    try
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("role_id", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    catch (Exception)
                    {
                        //HttpContext.Current.Response.Redirect("~/signin?session=U"); 
                        //redirect to error page
                    }
                    return string.Empty;
                }
            }
        }
        public string OperatorId
        {
            set
            {
                try
                {
                    LogSession(GetCurrentToken(_httpContext), "operator_id", value);
                    _httpContext.Session.Set("operator_id", Encoding.ASCII.GetBytes(value));
                }
                catch (Exception)
                {
                }
            }
            get
            {
                string sValue;
                byte[] bValue;
                try
                {
                    _httpContext.Session.TryGetValue("operator_id", out bValue);
                    sValue = Encoding.ASCII.GetString(bValue);
                    if (string.IsNullOrEmpty(sValue) == true)
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("operator_id", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    return sValue;
                }
                catch (Exception)
                {
                    try
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("operator_id", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    catch (Exception)
                    {
                        //HttpContext.Current.Response.Redirect("~/signin?session=U"); 
                        //redirect to error page
                    }
                    return string.Empty;
                }
            }
        }
        public string OperatorName
        {
            set
            {
                try
                {
                    LogSession(GetCurrentToken(_httpContext), "operator_name", value);
                    _httpContext.Session.Set("operator_name", Encoding.ASCII.GetBytes(value));
                }
                catch (Exception)
                {
                }
            }
            get
            {
                string sValue;
                byte[] bValue;
                try
                {
                    _httpContext.Session.TryGetValue("operator_name", out bValue);
                    sValue = Encoding.ASCII.GetString(bValue);
                    if (string.IsNullOrEmpty(sValue) == true)
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("operator_name", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    return sValue;
                }
                catch (Exception)
                {
                    try
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("operator_name", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    catch (Exception)
                    {
                        //HttpContext.Current.Response.Redirect("~/signin?session=U"); 
                        //redirect to error page
                    }
                    return string.Empty;
                }
            }
        }
        public string FullName
        {
            set
            {
                try
                {
                    LogSession(GetCurrentToken(_httpContext), "full_name", value);
                    _httpContext.Session.Set("full_name", Encoding.ASCII.GetBytes(value));
                }
                catch (Exception)
                {
                }
            }
            get
            {
                string sValue;
                byte[] bValue;
                try
                {
                    _httpContext.Session.TryGetValue("full_name", out bValue);
                    sValue = Encoding.ASCII.GetString(bValue);
                    if (string.IsNullOrEmpty(sValue) == true)
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("full_name", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    return sValue;
                }
                catch (Exception)
                {
                    try
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("full_name", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    catch (Exception)
                    {
                        //HttpContext.Current.Response.Redirect("~/signin?session=U"); 
                        //redirect to error page
                    }
                    return string.Empty;
                }
            }
        }
        public string ImageUrl
        {
            set
            {
                try
                {
                    LogSession(GetCurrentToken(_httpContext), "image_url", value);
                    _httpContext.Session.Set("image_url", Encoding.ASCII.GetBytes(value));
                }
                catch (Exception)
                {
                }
            }
            get
            {
                string sValue;
                byte[] bValue;
                try
                {
                    _httpContext.Session.TryGetValue("image_url", out bValue);
                    sValue = Encoding.ASCII.GetString(bValue);
                    if (string.IsNullOrEmpty(sValue) == true)
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("image_url", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    return sValue;
                }
                catch (Exception)
                {
                    try
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("image_url", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    catch (Exception)
                    {
                        //HttpContext.Current.Response.Redirect("~/signin?session=U"); 
                        //redirect to error page
                    }
                    return string.Empty;
                }
            }
        }
        public string DocumentPath
        {
            set
            {
                try
                {
                    LogSession(GetCurrentToken(_httpContext), "document_path", value);
                    _httpContext.Session.Set("document_path", Encoding.ASCII.GetBytes(value));
                }
                catch (Exception)
                {
                }
            }
            get
            {
                string sValue;
                byte[] bValue;
                try
                {
                    _httpContext.Session.TryGetValue("document_path", out bValue);
                    sValue = Encoding.ASCII.GetString(bValue);
                    if (string.IsNullOrEmpty(sValue) == true)
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("document_path", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    return sValue;
                }
                catch (Exception)
                {
                    try
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("document_path", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    catch (Exception)
                    {
                        //HttpContext.Current.Response.Redirect("~/signin?session=U"); 
                        //redirect to error page
                    }
                    return string.Empty;
                }
            }
        }
        public string ImagePath
        {
            set
            {
                try
                {
                    LogSession(GetCurrentToken(_httpContext), "image_path", value);
                    _httpContext.Session.Set("image_path", Encoding.ASCII.GetBytes(value));
                }
                catch (Exception)
                {
                }
            }
            get
            {
                string sValue;
                byte[] bValue;
                try
                {
                    _httpContext.Session.TryGetValue("image_path", out bValue);
                    sValue = Encoding.ASCII.GetString(bValue);
                    if (string.IsNullOrEmpty(sValue) == true)
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("image_path", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    return sValue;
                }
                catch (Exception)
                {
                    try
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("image_path", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    catch (Exception)
                    {
                        //HttpContext.Current.Response.Redirect("~/signin?session=U"); 
                        //redirect to error page
                    }
                    return string.Empty;
                }
            }
        }
        public string ProcessId
        {
            set
            {
                try
                {
                    LogSession(GetCurrentToken(_httpContext), "process_id", value);
                    _httpContext.Session.Set("process_id", Encoding.ASCII.GetBytes(value));
                }
                catch (Exception ex)
                {
                    TechnicalErrorMessage = ex.Message;
                }
            }
            get
            {
                string sValue;
                byte[] bValue;
                try
                {
                    _httpContext.Session.TryGetValue("process_id", out bValue);
                    sValue = Encoding.ASCII.GetString(bValue);
                    if (string.IsNullOrEmpty(sValue) == true)
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("process_id", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    return sValue;
                }
                catch (Exception)
                {
                    try
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("process_id", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    catch (Exception)
                    {
                        //HttpContext.Current.Response.Redirect("~/signin?session=U"); 
                        //redirect to error page
                    }
                    return string.Empty;
                }
            }
        }
        public string TaskId
        {
            set
            {
                try
                {
                    LogSession(GetCurrentToken(_httpContext), "task_id", value);
                    _httpContext.Session.Set("task_id", Encoding.ASCII.GetBytes(value));
                }
                catch (Exception ex)
                {
                    TechnicalErrorMessage = ex.Message;
                }
            }
            get
            {
                string sValue;
                byte[] bValue;
                try
                {
                    _httpContext.Session.TryGetValue("task_id", out bValue);
                    sValue = Encoding.ASCII.GetString(bValue);
                    if (string.IsNullOrEmpty(sValue) == true)
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("task_id", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    return sValue;
                }
                catch (Exception)
                {
                    try
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("task_id", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    catch (Exception)
                    {
                        //HttpContext.Current.Response.Redirect("~/signin?session=U"); 
                        //redirect to error page
                    }
                    return string.Empty;
                }
            }
        }
        public string PendingTaskCount
        {
            set
            {
                try
                {
                    LogSession(GetCurrentToken(_httpContext), "pending_task_count", value);
                    _httpContext.Session.Set("pending_task_count", Encoding.ASCII.GetBytes(value));
                }
                catch (Exception ex)
                {
                    TechnicalErrorMessage = ex.Message;
                }
            }
            get
            {
                string sValue;
                byte[] bValue;
                try
                {
                    _httpContext.Session.TryGetValue("pending_task_count", out bValue);
                    sValue = Encoding.ASCII.GetString(bValue);
                    if (string.IsNullOrEmpty(sValue) == true)
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("pending_task_count", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    return sValue;
                }
                catch (Exception)
                {
                    try
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("pending_task_count", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    catch (Exception)
                    {
                        //HttpContext.Current.Response.Redirect("~/signin?session=U"); 
                        //redirect to error page
                    }
                    return string.Empty;
                }
            }
        }
        public string SSOAppIds
        {
            set
            {
                try
                {
                    LogSession(GetCurrentToken(_httpContext), "app_id", value);
                    _httpContext.Session.Set("app_id", Encoding.ASCII.GetBytes(value));
                }
                catch (Exception)
                {
                }
            }
            get
            {
                string sValue;
                byte[] bValue;
                try
                {
                    _httpContext.Session.TryGetValue("app_id", out bValue);
                    sValue = Encoding.ASCII.GetString(bValue);
                    if (string.IsNullOrEmpty(sValue) == true)
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("app_id", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    return sValue;
                }
                catch (Exception)
                {
                    try
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("app_id", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    catch (Exception)
                    {
                        //HttpContext.Current.Response.Redirect("~/signin?session=U"); 
                        //redirect to error page
                    }
                    return string.Empty;
                }
            }
        }
        public string SSOAppNames
        {
            set
            {
                try
                {
                    LogSession(GetCurrentToken(_httpContext), "app_name", value);
                    _httpContext.Session.Set("app_name", Encoding.ASCII.GetBytes(value));
                }
                catch (Exception)
                {
                }
            }
            get
            {
                string sValue;
                byte[] bValue;
                try
                {
                    _httpContext.Session.TryGetValue("app_name", out bValue);
                    sValue = Encoding.ASCII.GetString(bValue);
                    if (string.IsNullOrEmpty(sValue) == true)
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("app_name", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    return sValue;
                }
                catch (Exception)
                {
                    try
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("app_name", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    catch (Exception)
                    {
                        //HttpContext.Current.Response.Redirect("~/signin?session=U"); 
                        //redirect to error page
                    }
                    return string.Empty;
                }
            }
        }
        public string SSOAppHosts
        {
            set
            {
                try
                {
                    LogSession(GetCurrentToken(_httpContext), "app_host", value);
                    _httpContext.Session.Set("app_host", Encoding.ASCII.GetBytes(value));
                }
                catch (Exception)
                {
                }
            }
            get
            {
                string sValue;
                byte[] bValue;
                try
                {
                    _httpContext.Session.TryGetValue("app_host", out bValue);
                    sValue = Encoding.ASCII.GetString(bValue);
                    if (string.IsNullOrEmpty(sValue) == true)
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("app_host", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    return sValue;
                }
                catch (Exception)
                {
                    try
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("app_host", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    catch (Exception)
                    {
                        //HttpContext.Current.Response.Redirect("~/signin?session=U"); 
                        //redirect to error page
                    }
                    return string.Empty;
                }
            }
        }
        public string SSOAppHostMaster
        {
            set
            {
                try
                {
                    LogSession(GetCurrentToken(_httpContext), "app_host_master", value);
                    _httpContext.Session.Set("app_host_master", Encoding.ASCII.GetBytes(value));
                }
                catch (Exception)
                {
                }
            }
            get
            {
                string sValue;
                byte[] bValue;
                try
                {
                    _httpContext.Session.TryGetValue("app_host_master", out bValue);
                    sValue = Encoding.ASCII.GetString(bValue);
                    if (string.IsNullOrEmpty(sValue) == true)
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("app_host_master", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    return sValue;
                }
                catch (Exception)
                {
                    try
                    {
                        LoadSession(GetCurrentToken(_httpContext), _httpContext);
                        _httpContext.Session.TryGetValue("app_host_master", out bValue);
                        sValue = Encoding.ASCII.GetString(bValue);
                    }
                    catch (Exception)
                    {
                        //HttpContext.Current.Response.Redirect("~/signin?session=U"); 
                        //redirect to error page
                    }
                    return string.Empty;
                }
            }
        }

        #endregion








    }

}
