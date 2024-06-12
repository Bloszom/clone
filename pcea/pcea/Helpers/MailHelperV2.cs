using Microsoft.EntityFrameworkCore;
using _FrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using MimeKit;
using Microsoft.AspNetCore.Http;
using System.IO;
using MailKit.Net.Smtp;
using System.Data.SqlClient;
using System.Net;

namespace pcea.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public class MailHelperV2 : _Database
    {
        private string SmtpServer;
        private int SmtpPort;
        private bool SslSupport;
        private string SmtpUsername;
        private string SmtpPassword;
        private string MailFrom;
        private string TechnicalErrorMessage;
        private string FriendlyErrorMessage;

        public bool SaveCopyToDatabase = false;
        public bool IsHtml = true;
        public string MailTo;
        public string MailCc;
        public string MailBcc;
        public string MailSubject;
        public string MailBody;
        public string ReferenceNumber = "Unspecified";
        public string MailType = "Unspecified";
        //MailMessage ObjMailMessage = new MailMessage();
        SmtpClient ObjSmtpClient;

        /// <summary>
        /// Instatiate the mail system using information stored in the mail server configuration table
        /// </summary>
        public MailHelperV2()
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
        public MailHelperV2(string sSmtpServer, int iSmtpPort, bool bSslSupport, string sUsername, string sPassword)
        {
            try
            {
                //ObjSmtpClient = new SmtpClient(sSmtpServer, iSmtpPort)
                //{
                //    UseDefaultCredentials = false
                //};
                //if (iSmtpPort == 0)
                //{
                //    ObjSmtpClient = new SmtpClient(sSmtpServer)
                //    {
                //        UseDefaultCredentials = true
                //    };
                //}
                //else
                //{
                //    ObjSmtpClient = new SmtpClient(sSmtpServer, iSmtpPort)
                //    {
                //        UseDefaultCredentials = false
                //    };
                //}
                //NetworkCredential Credentials = new NetworkCredential(sUsername, sPassword);
                //ObjSmtpClient.Credentials = Credentials;
                //ObjSmtpClient.EnableSsl = bSslSupport;

                //ObjMailMessage.BodyEncoding = Encoding.Default;
                //ObjMailMessage.Priority = MailPriority.High;
                //ObjMailMessage.IsBodyHtml = IsHtml;

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

                //var sel = "SELECT * FROM MailConfig ORDER BY SmtpServer";
                DataTable dt = ExecuteDataTable(objCmd);
                SmtpServer = dt.Rows[0]["SmtpServer"].ToString();
                SmtpPort = int.Parse(dt.Rows[0]["SmtpPort"].ToString());
                if (int.Parse(dt.Rows[0]["SslSupport"].ToString()) == 0) SslSupport = false;
                else SslSupport = true;

                SmtpUsername = dt.Rows[0]["SmtpUsername"].ToString();
                SmtpPassword = dt.Rows[0]["SmtpPassword"].ToString();
                MailFrom = dt.Rows[0]["DefaultEmail"].ToString();

                //if (SmtpPort == 0)
                //{
                //    ObjSmtpClient = new SmtpClient(SmtpServer)
                //    {
                //        UseDefaultCredentials = true
                //    };
                //}
                //else
                //{
                //    ObjSmtpClient = new SmtpClient(SmtpServer, SmtpPort)
                //    {
                //        UseDefaultCredentials = false
                //    };
                //}
                //NetworkCredential Credentials = new NetworkCredential(SmtpUsername, SmtpPassword);
                //ObjSmtpClient.Credentials = Credentials;
                //ObjSmtpClient.EnableSsl = SslSupport;

                //ObjMailMessage.BodyEncoding = Encoding.Default;
                //ObjMailMessage.Priority = MailPriority.High;
                //ObjMailMessage.IsBodyHtml = IsHtml;
                return true;
            }
            catch (Exception ex)
            {
                TechnicalErrorMessage += ex.Message;
                FriendlyErrorMessage = "Error initializing mail server configuration.";
                return false;
            }
        }
        //public bool SendMail()
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(MailTo) == true)
        //        {
        //            FriendlyErrorMessage = "Please, specify destination email addresses in MailTo property";
        //            return false;
        //        }
        //        if (string.IsNullOrEmpty(MailSubject) == true)
        //        {
        //            FriendlyErrorMessage = "Please, specify destination mail subject/title in MailSubject property";
        //            return false;
        //        }
        //        if (string.IsNullOrEmpty(MailBody) == true)
        //        {
        //            FriendlyErrorMessage = "Please, specify destination mail body in MailBody property";
        //            return false;
        //        }
        //        if (string.IsNullOrEmpty(MailFrom) == true)
        //        {
        //            FriendlyErrorMessage = "Please, specify the originating email address";
        //            return false;
        //        }
        //        //build destination addresses
        //        if (string.IsNullOrEmpty(MailTo) == false)
        //        {
        //            string[] _AddressTo = MailTo.Split(';');
        //            for (int i = 0; i < _AddressTo.Length; i++)
        //            {
        //                ObjMailMessage.To.Add(_AddressTo[i]);
        //            }
        //        }
        //        if (string.IsNullOrEmpty(MailCc) == false)
        //        {
        //            string[] _AddressCc = MailCc.Split(';');
        //            for (int i = 0; i < _AddressCc.Length; i++)
        //            {
        //                ObjMailMessage.CC.Add(_AddressCc[i]);
        //            }
        //        }
        //        if (string.IsNullOrEmpty(MailBcc) == false)
        //        {
        //            string[] _AddressBcc = MailBcc.Split(';');
        //            for (int i = 0; i < _AddressBcc.Length; i++)
        //            {
        //                ObjMailMessage.Bcc.Add(_AddressBcc[i]);
        //            }
        //        }

        //        ObjMailMessage.Subject = MailSubject;
        //        ObjMailMessage.From = new MailAddress(MailFrom);
        //        ObjMailMessage.Body = WebUtility.HtmlDecode(MailBody);
        //        ObjSmtpClient.Send(ObjMailMessage);

        //        if (SaveCopyToDatabase == true)
        //        {
        //            if (LogMail() == false)
        //            {
        //                FriendlyErrorMessage = "Mail sent successfully but a copy could not be saved to the mail log.  Please, ensure the mail log database is properly setup.";
        //                return false;
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        TechnicalErrorMessage += ex.Message;
        //        FriendlyErrorMessage = "Unable to send mail";
        //        return false;
        //    }
        //}

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
                //sSQL.Replace("@ReferenceNo", ReferenceNumber);
                //sSQL.Replace("@MailFrom", MailFrom);
                //sSQL.Replace("@MailTo", MailTo);
                //sSQL.Replace("@MailSubject", MailSubject);
                //sSQL.Replace("@MailBody", sBody);
                //sSQL.Replace("@MailType", MailType);

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

        public class MailRequest
        {
            //public string ToEmail { get; set; }
            public string Subject { get; set; }
            //public string Body { get; set; }
            public List<IFormFile> Attachments { get; set; }
        }

        public async Task<bool> SendEmailAsync(MailRequest mailRequest)
        {
            try
            {
                var email = new MimeMessage();


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
                        email.To.Add(MailboxAddress.Parse(_AddressTo[i]));
                    }
                }
                if (string.IsNullOrEmpty(MailCc) == false)
                {
                    string[] _AddressCc = MailCc.Split(';');
                    for (int i = 0; i < _AddressCc.Length; i++)
                    {
                        email.Cc.Add(MailboxAddress.Parse(_AddressCc[i]));
                    }
                }
                if (string.IsNullOrEmpty(MailBcc) == false)
                {
                    string[] _AddressBcc = MailBcc.Split(';');
                    for (int i = 0; i < _AddressBcc.Length; i++)
                    {
                        email.Bcc.Add(MailboxAddress.Parse(_AddressBcc[i]));
                    }
                }
                email.Sender = MailboxAddress.Parse(MailFrom);

                email.Subject = mailRequest.Subject;
                var builder = new BodyBuilder();
                if (mailRequest.Attachments != null)
                {
                    byte[] fileBytes;
                    foreach (var file in mailRequest.Attachments)
                    {
                        if (file.Length > 0)
                        {
                            using (var ms = new MemoryStream())
                            {
                                file.CopyTo(ms);
                                fileBytes = ms.ToArray();
                            }
                            builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                        }
                    }
                }

                if(IsHtml)
                    builder.HtmlBody = MailBody;
                else
                    builder.TextBody = MailBody;

                email.Body = builder.ToMessageBody();
                using var smtp = new SmtpClient();
                smtp.Connect(SmtpServer, SmtpPort, SslSupport);
                smtp.Authenticate(SmtpUsername, SmtpPassword);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);

                if (SaveCopyToDatabase == true)
                {
                    if (LogMail() == false)
                    {
                        FriendlyErrorMessage = "Mail sent successfully but a copy could not be saved to the mail log. Please, ensure the mail log database is properly setup.";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                TechnicalErrorMessage = ex.Message;
                return false;
            }


            return true;
        }

        //public bool ExecuteNonQuery(string sql)
        //{
        //   var count = _DbContext.Database.ExecuteSqlRaw(sql);
        //    return count > 0;
        //}

        //public DataTable ExecuteDataTable(string sql)
        //{
        //    DataTable dt = new DataTable();
        //    //using (var connection = ContextFactory.GetNewContextGeneric(connectionString).Database.GetDbConnection())
        //    using (var connection = _DbContext.Database.GetDbConnection())
        //    {
        //        connection.Open();
        //        var command = connection.CreateCommand();
        //        command.CommandText = sql;

        //        Microsoft.Data.SqlClient.SqlDataAdapter da = new Microsoft.Data.SqlClient.SqlDataAdapter(command.ExecuteReader());

        //        da.Fill(dt);
        //    }

        //    return dt;
        //}
    }
}
