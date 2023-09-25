using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using Helper.Data;
using Helper.Hub_Config;
using Helper.Interface;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using static Helper.Model.ChangeCustomerSettingsClsDeclarations;
//using Excel = Microsoft.Office.Interop.Excel;

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

        public void ReadExcel(ChangeCustomerSettingsSummary objCustomerData)
        {
            
            string filePath = this._Configuration.GetSection("ReadMasterData")["FilePath"]; // Replace with your Excel file path

            using (var package = new ExcelPackage(new System.IO.FileInfo(filePath)))
            {
                foreach (var worksheet in package.Workbook.Worksheets)
                {
                    ExcelWorksheet worksheet1 = package.Workbook.Worksheets[worksheet.Index];

                    // Define your search criteria (for example, searching for a specific value in a specific column)
                    int targetColumnIndex = 2; // Replace with the index of the column you want to search in (1-based index)
                    string targetValue = objCustomerData.templateCustomerName; // Replace with the value you want to search for

                    // Loop through the rows to find the row that matches your criteria
                    int rowCount = worksheet.Dimension.Rows;
                    for (int row = 1; row <= rowCount; row++)
                    {
                        string cellValue = worksheet.Cells[row, targetColumnIndex].Text;
                        if (cellValue.Trim() == targetValue.Trim())
                        {
                            // Replace the entire row with new data
                            ReplaceRowData(worksheet, row,objCustomerData.AS400UserId,objCustomerData.customerEmailId , DateTime.Now);

                            // You can break the loop if you only want to replace the first matching row
                            // break;
                        }
                    }

                    // Save the changes back to the Excel file
                    package.Save();
                }
            }
        
        }

        static void ReplaceRowData(ExcelWorksheet worksheet, int rowIndex, string AS400USERId , string customeremailid, DateTime dt)
        {
            // Replace the data in the row with your new data
            // For example, if you want to set new values for columns A, B, and C:
           // worksheet.Cells[rowIndex, 1].Value = "PK_CUSTOMER_MAPPING_ID";
            worksheet.Cells[rowIndex, 4].Value = AS400USERId;
            worksheet.Cells[rowIndex, 5].Value = customeremailid;
            worksheet.Cells[rowIndex, 6].Value = dt.ToString("dd-MMM-yyyy hh:mm:ss tt");
        }
    }

}
