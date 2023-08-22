using Helper.Data;
using Helper.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.IO;
using System.Net.Mail;
using Helper.Model;

namespace Helper.Report
{
   public class QuotationSubmitReport : IQuotationSubmitReport
    {
        #region
        public DataConnection _datacontext;
        private IConfiguration _Configuration;
        #endregion

        #region

        public QuotationSubmitReport(DataConnection datacontext, IConfiguration configuration)
        {
            _datacontext = datacontext;
            _Configuration = configuration;
        }
        #endregion
        public List<QuotationReportSummary> GetQuotationDaily_Report(DailyReport objDailyReport)
        {
            #region
            List<QuotationReportSummary> lstQuotationReportSummary = new List<QuotationReportSummary>();
            #endregion

            #region
            DateTime toDate;
            DateTime fromDate;
            string Todate = "";
            bool isExcelCreate = false;
            #endregion

            #region
            string time = this._Configuration.GetSection("ReportSettings")["Time"];
            #endregion

            #region
            if (!string.IsNullOrEmpty(objDailyReport.fromDate) && !string.IsNullOrEmpty(objDailyReport.toDate))
            {
                Todate = Convert.ToDateTime(objDailyReport.toDate).ToString("yyyy-MM-dd");

                toDate = Convert.ToDateTime(objDailyReport.toDate + " " + Convert.ToDateTime(time).ToString("HH:mm:ss"));
                fromDate = Convert.ToDateTime(objDailyReport.fromDate + " " + Convert.ToDateTime(time).ToString("HH:mm:ss"));
            }
            else
            {
                isExcelCreate = true;
                Todate = DateTime.Today.ToString("yyyy-MM-dd");

                toDate = Convert.ToDateTime(Todate + " " + Convert.ToDateTime(time).ToString("HH:mm:ss"));
                fromDate = toDate.AddHours(-24);
            }
            #endregion

            #region
            var Tasklist = (from hdr in _datacontext.TMSD_ENQUIRY_HDRTable
                            where hdr.STATUS != 8 
                            select hdr).OrderBy(x => x.CREATED_DATE).ToList();

            var Tasklist1 =  Tasklist.Where(x => ParseStringToDate.ParseStringtoDatetime(x.QUOTATION_RECIVED_AT) >= fromDate && ParseStringToDate.ParseStringtoDatetime(x.QUOTATION_RECIVED_AT) <= toDate);
            foreach (var item in Tasklist1)
            {
                QuotationReportSummary objQuotationReportSummary = new QuotationReportSummary
                {

                    enquiryId = item.PK_MSDENQUIRY_HDR_ID,
                    enqrefNo = item.ENQREF_NO,
                    quotationNo = item.QUOTATION_NO,

                    quotationReceivedat = item.QUOTATION_RECIVED_AT,//.ToString("dd-MMM-yyyy hh:mm:ss tt"),
                    quotationSubmitedat = item.QUATATION_SUBMIT_DATE,

                };
                lstQuotationReportSummary.Add(objQuotationReportSummary);
            }
            #endregion
            #region
            if (lstQuotationReportSummary.Count != 0 && isExcelCreate)
            {

                CreateExel(lstQuotationReportSummary, Todate, toDate, fromDate);

            }
            else
            {
                Send_MailForEnquiryNotRecived();
            }
            #endregion
            return lstQuotationReportSummary.OrderByDescending(x => x.enquiryId).ToList();
        }


        public void CreateExel(List<QuotationReportSummary> lstQuotationReportSummary, string today, DateTime FROMDATE, DateTime TODATE)
        {
            try
            {
                #region
                string outputPath = this._Configuration.GetSection("ReportSettings")["QuotationSubmitOutputPath"];
                #endregion

                using (ExcelPackage excel = new ExcelPackage())
                {
                    ExcelWorksheet sheet = excel.Workbook.Worksheets.Add("Sheet1");


                    #region
                    var range = sheet.Cells["A1:E1"];
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                    range.Style.Font.Color.SetColor(Color.White);
                    #endregion

                    #region
                    sheet.Cells[1, 1].Value = "Sr No";
                    sheet.Cells[1, 2].Value = "Customer Enquiry No";
                    sheet.Cells[1, 3].Value = "Quotation No";
                    sheet.Cells[1, 4].Value = "Quotation Received Date/ Time";
                    sheet.Cells[1, 5].Value = "Quotation Submited Date / Time";
                    #endregion

                    int srNo = 1;
                    int i = 2;

                    #region
                    foreach (var item in lstQuotationReportSummary)
                    {

                        int j = 1;

                        sheet.Cells[i, j++].Value = srNo;
                        sheet.Cells[i, j++].Value = item.enqrefNo;
                      
                        sheet.Cells[i, j++].Value = item.quotationNo;
                       
                        if (item.quotationReceivedat == null || item.quotationReceivedat == "")
                        {
                            sheet.Cells[i, j++].Value = item.quotationReceivedat = "";
                        }
                        else
                        {
                            DateTime verifydt = Convert.ToDateTime(item.quotationReceivedat);
                            sheet.Cells[i, j++].Value = item.quotationReceivedat = verifydt.ToString("dd-MMM-yyyy hh:mm:ss tt");
                        }

                        if (item.quotationSubmitedat == null)
                        {
                            sheet.Cells[i, j++].Value = item.quotationSubmitedat = "";
                        }
                        else
                        {
                            DateTime verifydt = Convert.ToDateTime(item.quotationSubmitedat);
                            sheet.Cells[i, j++].Value = item.quotationSubmitedat = verifydt.ToString("dd-MMM-yyyy hh:mm:ss tt");
                        }

                        srNo = srNo + 1;
                        i = i + 1;


                    }
                    #endregion

                    sheet.Cells["A:E"].AutoFitColumns();
                    FileInfo fileInfo = new FileInfo(outputPath + "\\Quotation_" + today + ".xlsx");
                    excel.SaveAs(fileInfo);
                    excel.Stream.Close();

                    string filepath = outputPath + "\\Quotation_" + today;

                    #region
                    Send_Mail(filepath);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        public void Send_Mail(string filepath)
        {
            try
            {
                dynamic dtlBody = "";

                #region
                string reportEnv = this._Configuration.GetSection("QuotationSubmitReport")["Enviornment"];

                MailMessage mail = new MailMessage();
                
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                mail.From = new MailAddress(this._Configuration.GetSection("QuotationSubmitReport")["FromMail"]);
                mail.To.Add(this._Configuration.GetSection("QuotationSubmitReport")["ToMail"]);
                mail.CC.Add(this._Configuration.GetSection("QuotationSubmitReport")["CCMail"]);


                mail.Subject = "Daily " + reportEnv + " MSD quotation submit report";
                mail.Body = "<p>Hello Team,<br /><br />Please find report of Daily Quotations submitted by BOT.<br/><br/>" +
                    "<span style='color: red;'>This is a system generated email, do not reply to this email id.</span><br/><br/>" +
                    "Thanks & Regards,<br />" +
                    "RPA BOT</p>";
                mail.IsBodyHtml = true;
                mail.Attachments.Add(new Attachment(filepath + ".xlsx"));


                SmtpServer.Port = 587;
                SmtpServer.UseDefaultCredentials = true;
                SmtpServer.Credentials = new System.Net.NetworkCredential(this._Configuration.GetSection("QuotationSubmitReport")["Username"], this._Configuration.GetSection("QuotationSubmitReport")["Password"]);
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
                #endregion
            }
            catch (Exception ex)
            {

               
            }
        }

        public void Send_MailForEnquiryNotRecived()
        {
            try
            {
                dynamic dtlBody = "";
                #region
                string reportEnv = this._Configuration.GetSection("QuotationSubmitReport")["Enviornment"];

                MailMessage mail = new MailMessage();
               
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                mail.From = new MailAddress(this._Configuration.GetSection("QuotationSubmitReport")["FromMail"]);
                mail.To.Add(this._Configuration.GetSection("QuotationSubmitReport")["ToMail"]);
                mail.CC.Add(this._Configuration.GetSection("QuotationSubmitReport")["CCMail"]);


                mail.Subject = "Daily " + reportEnv + " MSD quotation submit report";
                mail.Body = "<p>Hello Team,<br /><br />No Quotation received today.<br/><br/>" +
                    "<span style='color: red;'>This is a system generated email, do not reply to this email id.</span><br/><br/>" +
                    "Thanks & Regards,<br />" +
                    "RPA BOT</p>";
                mail.IsBodyHtml = true;
               
                SmtpServer.Port = 587;
                SmtpServer.UseDefaultCredentials = true;
                SmtpServer.Credentials = new System.Net.NetworkCredential(this._Configuration.GetSection("QuotationSubmitReport")["Username"], this._Configuration.GetSection("QuotationSubmitReport")["Password"]);
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);

                #endregion
            }
            catch (Exception ex)
            {

            }
        }

    }

    #region
    public class QuotationReportSummary
    {
      
        public long enquiryId { get; set; }
        public string enqrefNo { get; set; }

        public string quotationNo { get; set; }
       
        public string quotationReceivedat { get; set; }

        public string quotationSubmitedat { get; set; }
    }
    #endregion
}
