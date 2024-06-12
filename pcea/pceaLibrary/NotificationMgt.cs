using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using _FrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace pceaLibrary
{
    public class NotificationMgt: _Mail
    {
        private readonly IHostingEnvironment hosting;

        public NotificationMgt(IHostingEnvironment hosting)
        {
            this.hosting = hosting;
        }
        public bool SendNotification(string sOperatorId, string sNotificationType, string sNotificationActionType, string Submission)
        {
            try
            {
                //get user details
                string sEmail = string.Empty;
                string sFullname = string.Empty;
                string sOrgName = string.Empty;
                using (SqlCommand objCmd = new SqlCommand())
                {
                    objCmd.CommandType = CommandType.Text;
                    objCmd.CommandText = "SELECT * FROM [UserProfile] WHERE [OrganizationId]=@OrganizationId ";
                    objCmd.Parameters.Clear();
                    objCmd.Parameters.AddWithValue("@OrganizationId", sOperatorId);
                    using (var _dt = ExecuteDataTable(objCmd))
                    {
                        if (_dt == null)
                        {
                            FriendlyErrorMessage += "Invalid Operator Id.  Operation aborted.";
                            return false;
                        }
                        sEmail = _dt.Rows[0]["Email"].ToString();
                        sFullname = _dt.Rows[0]["Fullname"].ToString();
                        sOrgName = _dt.Rows[0]["OrganizationName"].ToString();
                    }
                }

                //get mail message html
                string sFilename = "templates/" + sNotificationType + "_" + sNotificationActionType + ".html";
                string sBody = string.Empty;

                
               // var fs = new FileStream(sFilename, FileMode.Open, FileAccess.Read);

                using (StreamReader reader = new StreamReader(Path.Combine(hosting.WebRootPath, sFilename)))
                {
                    sBody = reader.ReadToEnd();
                }
                sBody = sBody.Replace("{FULLNAME}", sFullname);   
                sBody = sBody.Replace("{OPERATORNAME}", sOrgName);
                sBody = sBody.Replace("{YEAR}", DateTime.Now.Year.ToString());
                sBody = sBody.Replace("{SUBMISSION}", Submission);

                //send mail
                _Mail _mail = new _Mail();
                _mail.MailTo = sEmail;
                _mail.MailSubject = sNotificationType;
                _mail.MailBody = sBody;
                _mail.ReferenceNumber = sOperatorId;
                _mail.MailType = sNotificationType;
                _mail.SaveCopyToDatabase = true;
                if (_mail.SendMail() == false)
                {
                    FriendlyErrorMessage = "Unable to send mail.  Contact System Administrator";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                TechnicalErrorMessage = ex.Message;
                return false;
            }
        }



    }
}
