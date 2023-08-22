using Helper.Data;
using Helper.Interface;
using Microsoft;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Linq;
using Helper.Model;
using Microsoft.Extensions.Configuration;

namespace Helper.Report
{
    public class MSDReport : IMSDReport
    {
        #region
        public DataConnection _datacontext;
        private IConfiguration _Configuration;
        #endregion


        #region
        public MSDReport(DataConnection datacontext, IConfiguration configuration)
        {
            _datacontext = datacontext;
            _Configuration = configuration;
        }
        #endregion

        public List<MSDReportSummary> GetMSDDaily_Report(DailyReport objDailyReport)
        {

            #region
            List<MSDReportSummary> lstMSDReportSummary = new List<MSDReportSummary>();
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
                            where hdr.STATUS != 8 && hdr.CREATED_DATE >= fromDate && hdr.CREATED_DATE <= toDate
                            select hdr).OrderBy(x => x.CREATED_DATE).ToList();
            foreach (var item in Tasklist)
            {
                MSDReportSummary objMSDReportSummary = new MSDReportSummary
                {

                    PK_MSDENQUIRY_HDR_ID = item.PK_MSDENQUIRY_HDR_ID,
                    enquiryDate = item.ENQUIRY_DATE,
                    owner = item.OWNER,
                    enqrefNo = item.ENQREF_NO,
                    quotationNo = item.QUOTATION_NO,
                    quotationCreatedat = item.QUOTATION_CREATED_AT,
                    emailReceivedat = item.EMAIL_RECEIVED_AT,
                    emailProcessedat = item.EMAIL_PROCESSED_AT,
                    inErrorat = item.IN_ERROR_AT,
                    verifiedAt = item.VERIFIED_AT,
                 
                    shipName = item.SHIP_NAME,
                    sourceType = item.SOURCE_TYPE,
                  
                    detailCount = (from dtl in _datacontext.TMSD_ENQUIRY_DTLTable where dtl.FK_MSDENQUIRY_HDR_ID == item.PK_MSDENQUIRY_HDR_ID select dtl).ToList().Count(),
                    errorDetailCount = (from dtl in _datacontext.TMSD_ENQUIRY_DTLTable where (dtl.FK_MSDENQUIRY_HDR_ID == item.PK_MSDENQUIRY_HDR_ID && dtl.STATUS == 5) select dtl).ToList().Count(),
                    VerifiedBy = (from dtl in _datacontext.MstUserTable where (dtl.PK_USER_ID == Convert.ToInt64(item.VERIFIED_BY)) select dtl.USER_NAME).SingleOrDefault(),

                    CorrectedBy = (from dtl in _datacontext.MstUserTable where (dtl.PK_USER_ID == Convert.ToInt64(item.CORRECTED_BY)) select dtl.USER_NAME).SingleOrDefault(),


                    status = (from statusmst in _datacontext.M_STATUS_CODE where item.STATUS == statusmst.STATUS_CODE select statusmst.STATUS_DESCRIPTION).SingleOrDefault(),
                };
                lstMSDReportSummary.Add(objMSDReportSummary);
            }
            #endregion

            #region
            if (lstMSDReportSummary.Count != 0 && isExcelCreate)
            {

                CreateExel(lstMSDReportSummary, Todate, toDate, fromDate);

            }
            else
            {
                Send_MailForEnquiryNotRecived();
            }
            #endregion
            return lstMSDReportSummary.OrderByDescending(x => x.emailReceivedat).ToList();
        }

        public void CreateExel(List<MSDReportSummary> lstSNQReportSummary, string today, DateTime FROMDATE, DateTime TODATE)
        {
            try
            {
                #region
                string outputPath = this._Configuration.GetSection("ReportSettings")["MSDOutputPath"];

                #endregion
                #region
                using (ExcelPackage excel = new ExcelPackage())
                {
                    ExcelWorksheet sheet = excel.Workbook.Worksheets.Add("Sheet1");

                    var range = sheet.Cells["A1:N1"];
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                    range.Style.Font.Color.SetColor(Color.White);

                    #region
                    sheet.Cells[1, 1].Value = "Sr No";
                    sheet.Cells[1, 2].Value = "Customer Enquiry No";
                    sheet.Cells[1, 3].Value = "Customer Name";
                    sheet.Cells[1, 4].Value = "Ship Name";
                    sheet.Cells[1, 5].Value = "Quotation No";
                    sheet.Cells[1, 6].Value = "Total No of Items";
                    sheet.Cells[1, 7].Value = "No of Items in Error";
                    sheet.Cells[1, 8].Value = "Email Received Date/ Time";
                    sheet.Cells[1, 9].Value = "Error Date/ Time";
                    sheet.Cells[1, 10].Value = "Quotation Created Date / Time";
                 
                    sheet.Cells[1, 11].Value = "Verified By";
                    sheet.Cells[1, 12].Value = "Corrected By";
                    sheet.Cells[1, 13].Value = "Source Type";
                    sheet.Cells[1, 14].Value = "Status";

                    #endregion
                    int srNo = 1;
                    int i = 2;

                    #region
                    foreach (var item in lstSNQReportSummary)
                    {

                        int j = 1;

                        sheet.Cells[i, j++].Value = srNo;
                        sheet.Cells[i, j++].Value = item.enqrefNo;
                        sheet.Cells[i, j++].Value = item.owner;
                        sheet.Cells[i, j++].Value = item.shipName;
                        sheet.Cells[i, j++].Value = item.quotationNo;
                        sheet.Cells[i, j++].Value = item.detailCount;
                        sheet.Cells[i, j++].Value = item.errorDetailCount;


                        if (item.emailReceivedat == null || item.emailReceivedat == "")
                        {
                            sheet.Cells[i, j++].Value = item.emailReceivedat = "";
                        }
                        else
                        {
                            DateTime verifydt = Convert.ToDateTime(item.emailReceivedat);
                            sheet.Cells[i, j++].Value = item.emailReceivedat = verifydt.ToString("dd-MMM-yyyy hh:mm:ss tt");
                        }
                        if (item.inErrorat == null || item.inErrorat == "")
                        {
                            sheet.Cells[i, j++].Value = item.inErrorat = "";
                        }
                        else
                        {
                            DateTime verifydt = Convert.ToDateTime(item.inErrorat);
                            sheet.Cells[i, j++].Value = item.inErrorat = verifydt.ToString("dd-MMM-yyyy hh:mm:ss tt");
                        }

                        if (item.quotationCreatedat == null || item.quotationCreatedat == "")
                        {
                            sheet.Cells[i, j++].Value = item.quotationCreatedat = "";
                        }
                        else
                        {
                            DateTime verifydt = Convert.ToDateTime(item.quotationCreatedat);
                            sheet.Cells[i, j++].Value = item.quotationCreatedat = verifydt.ToString("dd-MMM-yyyy hh:mm:ss tt");
                        }

                       

                        sheet.Cells[i, j++].Value = (item.VerifiedBy == null || item.VerifiedBy == "") ? "" : (item.VerifiedBy);
                        sheet.Cells[i, j++].Value = (item.CorrectedBy == null || item.CorrectedBy == "") ? "" : (item.CorrectedBy);
                        sheet.Cells[i, j++].Value = item.sourceType;

                        if (item.status == "Error")
                        {
                            item.status = "In Error Basket";
                            //item.STATUS;
                        }
                        else if (item.status == "NotStarted")
                        {
                            item.status = "Pending for Verification";
                        }
                        else if (item.status == "Success")
                        {
                            item.status = "Success";
                        }
                        else if (item.status == "Updated")
                        {
                            item.status = "InProcess";
                        }
                        else if (item.status == "ForwardToManual")
                        {
                            item.status = "Forwarded to Manual Process";
                        }
                        else if (item.status == "Manual Success")
                        {
                            item.status = "Manual Success";
                        }
                        else if (item.status == "System Exception")
                        {
                            item.status = "System Exception";
                        }

                        sheet.Cells[i, j++].Value = item.status;

                        srNo = srNo + 1;
                        i = i + 1;


                    }

                    #endregion

                    sheet.Cells["A:L"].AutoFitColumns();
                    FileInfo fileInfo = new FileInfo(outputPath + "\\Quotation_" + today + ".xlsx");
                    excel.SaveAs(fileInfo);
                    excel.Stream.Close();

                    string filepath = outputPath + "\\Quotation_" + today;

                    #region
                    Send_Mail(filepath);
                    #endregion

                }
                #endregion

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
                string reportEnv = this._Configuration.GetSection("MSDMailSettings")["Enviornment"];

                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                mail.From = new MailAddress(this._Configuration.GetSection("MSDMailSettings")["FromMail"]);
                mail.To.Add(this._Configuration.GetSection("MSDMailSettings")["ToMail"]);
                 mail.CC.Add(this._Configuration.GetSection("MSDMailSettings")["CCMail"]);


                mail.Subject = "Daily " + reportEnv +" MSD quotation creation report";
                mail.Body = "<p>Hello Team,<br /><br />Please find attachment.<br/><br/>" +
                    "<span style='color: red;'>This is a system generated email, do not reply to this email id.</span><br/><br/>" +
                    "Thanks & Regards,<br />" +
                    "RPA BOT</p>";
                mail.IsBodyHtml = true;
                mail.Attachments.Add(new Attachment(filepath + ".xlsx"));


                SmtpServer.Port = 587;
                SmtpServer.UseDefaultCredentials = true;
                SmtpServer.Credentials = new System.Net.NetworkCredential(this._Configuration.GetSection("MSDMailSettings")["Username"], this._Configuration.GetSection("MSDMailSettings")["Password"]);
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
                string reportEnv = this._Configuration.GetSection("MSDMailSettings")["Enviornment"];
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                mail.From = new MailAddress(this._Configuration.GetSection("MSDMailSettings")["FromMail"]);
                mail.To.Add(this._Configuration.GetSection("MSDMailSettings")["ToMail"]);
                mail.CC.Add(this._Configuration.GetSection("MSDMailSettings")["CCMail"]);
                mail.Subject = "Daily " + reportEnv + " MSD quotation creation report";
                mail.Body = "<p>Hello Team,<br /><br />No Enquiry received today.<br/><br/>" +
                    "<span style='color: red;'>This is a system generated email, do not reply to this email id.</span><br/><br/>" +
                    "Thanks & Regards,<br />" +
                    "RPA BOT</p>";
                mail.IsBodyHtml = true;
             
                SmtpServer.Port = 587;
                SmtpServer.UseDefaultCredentials = true;
                SmtpServer.Credentials = new System.Net.NetworkCredential(this._Configuration.GetSection("MSDMailSettings")["Username"], this._Configuration.GetSection("MSDMailSettings")["Password"]);
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
                #endregion
            }
            catch (Exception ex)
            {

                //MessageBox.Show(ex.ToString());
            }
        }


    }
    #region
    public class MSDReportSummary
    {
        public long PK_MSDENQUIRY_HDR_ID { get; set; }
        public string enquiryDate { get; set; }
        public string owner { get; set; }

        public string shipName { get; set; }
        public string enqrefNo { get; set; }

        public string status { get; set; }
        public string quotationNo { get; set; }
        public string emailReceivedat { get; set; }
        public string emailProcessedat { get; set; }
        public string inErrorat { get; set; }
        public string verifiedAt { get; set; }
        public string port { get; set; }
        public string quotationCreatedat { get; set; }

        public int detailCount { get; set; }

        public int errorDetailCount { get; set; }

        public string VerifiedBy { get; set; }

        public string CorrectedBy { get; set; }

        public string sourceType { get; set; }

    
    }
    #endregion

}
