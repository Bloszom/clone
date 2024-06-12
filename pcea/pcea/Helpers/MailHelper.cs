using System;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Data;
using System.Data.SqlClient;
using System.Drawing.Printing;

namespace pcea.Helpers
{
    public class MailHelper
    {
        private string SmtpServer;
        private int SmtpPort;
        private bool SslSupport;
        private string SmtpUsername;
        private string SmtpPassword;
        private string MailFrom;

        public bool SaveCopyToDatabase = false;
        public bool IsHtml = true;
        public string MailTo;
        public string MailCc;
        public string MailBcc;
        public string MailSubject;
        public string MailBody;
        public string ReferenceNumber = "Unspecified";
        public string MailType = "Unspecified";
        MailMessage ObjMailMessage = new MailMessage();
        SmtpClient ObjSmtpClient;

        /// <summary>
        /// Instatiate the mail system using information stored in the mail server configuration table
        /// </summary>
        public MailHelper()
        {
            try
            {
                InitializeMailServer();
            }
            catch (Exception ex)
            {
                TechnicalErrorMessage += ex.Message;
                FriendlyErrorMessage += "Error initializing mail system.";
                throw;
            }
        }

        /// <summary>
        /// Initialize the mail system using user provided SMTP credentials
        /// </summary>
        /// <param name="sSmtpServer">SMTP Server</param>
        /// <param name="iSmtpPort">SMTP Port</param>
        /// <param name="bSslSupport">True if server supports SSL</param>
        /// <param name="sUsername">SMTP username e.g. your email address</param>
        /// <param name="sPassword">SMTP password e.g. your email password</param>
        public MailHelper(string sSmtpServer, int iSmtpPort, bool bSslSupport, string sUsername, string sPassword)
        {
            try
            {
                //ObjSmtpClient = new SmtpClient(sSmtpServer, iSmtpPort)
                //{
                //    UseDefaultCredentials = false
                //};
                if (iSmtpPort == 0)
                {
                    ObjSmtpClient = new SmtpClient(sSmtpServer)
                    {
                        UseDefaultCredentials = true
                    };
                }
                else
                {
                    ObjSmtpClient = new SmtpClient(sSmtpServer, iSmtpPort)
                    {
                        UseDefaultCredentials = false
                    };
                }
                NetworkCredential Credentials = new NetworkCredential(sUsername, sPassword);
                ObjSmtpClient.Credentials = Credentials;
                ObjSmtpClient.EnableSsl = bSslSupport;

                ObjMailMessage.BodyEncoding = Encoding.Default;
                ObjMailMessage.Priority = MailPriority.High;
                ObjMailMessage.IsBodyHtml = IsHtml;

                MailFrom = sUsername;
            }
            catch (Exception ex)
            {
                TechnicalErrorMessage += ex.Message;
                FriendlyErrorMessage += "Error initializing mail system.";
                throw;
            }
        }

        private bool InitializeMailServer()
        {
            try
            {
                SqlCommand objCmd = new SqlCommand
                {
                    CommandText = "SELECT * FROM MailConfig ORDER BY SmtpServer "
                    
                };
                DataTable dt = ExecuteDataTable(objCmd);
                SmtpServer = dt.Rows[0]["SmtpServer"].ToString();
                SmtpPort = int.Parse(dt.Rows[0]["SmtpPort"].ToString());
                if (int.Parse(dt.Rows[0]["SslSupport"].ToString()) == 0) SslSupport = false;
                else SslSupport = true;
                
                SmtpUsername = dt.Rows[0]["SmtpUsername"].ToString();
                SmtpPassword = dt.Rows[0]["SmtpPassword"].ToString();
                MailFrom = dt.Rows[0]["DefaultEmail"].ToString();

                if (SmtpPort == 0)
                {
                    ObjSmtpClient = new SmtpClient(SmtpServer)
                    {
                        UseDefaultCredentials = true
                    };
                }
                else
                {
                    ObjSmtpClient = new SmtpClient(SmtpServer, SmtpPort)
                    {
                        UseDefaultCredentials = false
                    };
                }
                NetworkCredential Credentials = new NetworkCredential(SmtpUsername, SmtpPassword);
                ObjSmtpClient.Credentials = Credentials;
                ObjSmtpClient.EnableSsl = SslSupport;

                ObjMailMessage.BodyEncoding = Encoding.Default;
                ObjMailMessage.Priority = MailPriority.High;
                ObjMailMessage.IsBodyHtml = IsHtml;
                return true;
            }
            catch (Exception ex)
            {
                TechnicalErrorMessage += ex.Message;
                FriendlyErrorMessage = "Error initializing mail server configuration.";
                return false;
            }
        }
        public bool SendMail()
        {
            try
            {
                if (string.IsNullOrEmpty(MailTo) == true)
                {
                    FriendlyErrorMessage = "Please, specify destination email addresses in MailTo property";
                    return false;
                }
                if (string.IsNullOrEmpty(MailSubject) == true)
                {
                    FriendlyErrorMessage = "Please, specify destination mail subject/title in MailSubject property";
                    return false;
                }
                if (string.IsNullOrEmpty(MailBody) == true)
                {
                    FriendlyErrorMessage = "Please, specify destination mail body in MailBody property";
                    return false;
                }
                if (string.IsNullOrEmpty(MailFrom) == true)
                {
                    FriendlyErrorMessage = "Please, specify the originating email address";
                    return false;
                }
                //build destination addresses
                if (string.IsNullOrEmpty(MailTo) == false)
                {
                    string[] _AddressTo = MailTo.Split(';');
                    for (int i = 0; i < _AddressTo.Length; i++)
                    {
                        ObjMailMessage.To.Add(_AddressTo[i]);
                    }
                }
                if (string.IsNullOrEmpty(MailCc) == false)
                {
                    string[] _AddressCc = MailCc.Split(';');
                    for (int i = 0; i < _AddressCc.Length; i++)
                    {
                        ObjMailMessage.CC.Add(_AddressCc[i]);
                    }
                }
                if (string.IsNullOrEmpty(MailBcc) == false)
                {
                    string[] _AddressBcc = MailBcc.Split(';');
                    for (int i = 0; i < _AddressBcc.Length; i++)
                    {
                        ObjMailMessage.Bcc.Add(_AddressBcc[i]);
                    }
                }

                ObjMailMessage.Subject = MailSubject;
                ObjMailMessage.From = new MailAddress(MailFrom);
                ObjMailMessage.Body = WebUtility.HtmlDecode(MailBody);
                ObjSmtpClient.Send(ObjMailMessage);

                if(SaveCopyToDatabase == true)
                {
                    if(LogMail()==false)
                    {
                        FriendlyErrorMessage = "Mail sent successfully but a copy could not be saved to the mail log.  Please, ensure the mail log database is properly setup.";
                        return false;
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                TechnicalErrorMessage += ex.Message;
                FriendlyErrorMessage = "Unable to send mail";
                return false;
            }
        }

        private bool LogMail()
        {
            try
            {
                SqlCommand objCmd = new SqlCommand();
                string sSQL = "INSERT INTO [MailMessage] ([ReferenceNo],[MailFrom],[MailTo],[MailSubject],[MailBody],[MailType]) " +
                    "VALUES(@ReferenceNo,@MailFrom,@MailTo,@MailSubject,@MailBody,@MailType) ";
                string sBody = WebUtility.HtmlEncode(MailBody);

                objCmd.Parameters.Clear();
                objCmd.CommandText = sSQL;
                objCmd.Parameters.AddWithValue("@ReferenceNo", ReferenceNumber);
                objCmd.Parameters.AddWithValue("@MailFrom", MailFrom);
                objCmd.Parameters.AddWithValue("@MailTo", MailTo);
                objCmd.Parameters.AddWithValue("@MailSubject", MailSubject);
                objCmd.Parameters.AddWithValue("@MailBody", sBody);
                objCmd.Parameters.AddWithValue("@MailType", MailType);

                if (ExecuteNonQuery(objCmd) == false)
                {
                    FriendlyErrorMessage += "Error saving mail message.";
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                FriendlyErrorMessage += "Error saving mail message.";
                TechnicalErrorMessage += ex.Message;
                return false;
            }
        }

    }
}
