using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using Helper.Data;
using Helper.Hub_Config;
using Helper.Interface;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
namespace Helper.Model
{
    public class Notification
    {
        private IConfiguration _Configuration;
        #region
        public Notification(IConfiguration configuration)
        {
            _Configuration = configuration;
        }
        #endregion
        #region
        public void SendPendingRFQNotification(string rfqNo, long totalitemsCount, long totalinputeditemscount)
        {
            try
            {
                string enviornment = this._Configuration.GetSection("PendingRFQNotification")["Name"];
                dynamic dtlBody = "";
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                mail.From = new MailAddress(this._Configuration.GetSection("PendingRFQNotification")["FromMail"]);
                mail.To.Add(this._Configuration.GetSection("PendingRFQNotification")["ToMail"]);
                mail.CC.Add(this._Configuration.GetSection("PendingRFQNotification")["CCMail"]);
                if (rfqNo != "")
                {
                    mail.Subject = enviornment + " " + "MSD Quotation Creation Process Incomplete/ RFQ No : " + rfqNo;
                    mail.Body = "<p>Hello Team,<br /><br />finished incomplete inputting description as below :<br/><br/></p>" +
                         "<p> RFQ No: " + rfqNo + " <br/>" +
                         "<p> Total No Of Items: " + totalitemsCount + " <br/>" +
                         "<p> Total Inputted Items: " + totalinputeditemscount + " <br/><br/><br/>" +
                         "<span style='color: red;'>This is a system generated email, do not reply to this email id.</span><br/><br/>" +
                        "Thanks & Regards,<br />" +
                        "RPA BOT</p>";
                }
                mail.IsBodyHtml = true;
                SmtpServer.Port = 587;
                SmtpServer.UseDefaultCredentials = true;
                SmtpServer.Credentials = new System.Net.NetworkCredential(this._Configuration.GetSection("PendingRFQNotification")["Username"], this._Configuration.GetSection("PendingRFQNotification")["Password"]);
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }
        #endregion
    }
}
