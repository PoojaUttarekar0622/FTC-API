using Helper.Data;
using Helper.Interface;
using Helper.Model;
using Helper.Report;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FTC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        static readonly log4net.ILog _log4net = log4net.LogManager.GetLogger(typeof(SNQReport));
        private readonly ILogger<ReportController> _logger;
       
        IUser _userDetails;
        ISNQReport _snqReport;
        IMSDReport _msdReport;
        IQuotationSubmitReport _quotationSubmitReport;
        public ReportController(ISNQReport snqReport, IUser userDetails, IMSDReport msdReport , IQuotationSubmitReport quotationSubmitReport)
        {
            _snqReport = snqReport;
            _userDetails = userDetails;
            _msdReport = msdReport;
            _quotationSubmitReport = quotationSubmitReport;
        }

        //Daily quotation report for SNQ Enquiry
        [Route("GetSNQDaily_Report")]
        [HttpPost]
        public async Task<ActionResult> GetSNQDaily_Report(DailyReport objDailyReport)
        {

            List<SNQReportSummary> lstSNQReportSummary = new List<SNQReportSummary>();
            try
            {

                var getEnquiryheaderTask = Task.Run(() => { lstSNQReportSummary = _snqReport.GetSNQDaily_Report(objDailyReport); });
                await Task.WhenAll(getEnquiryheaderTask);
                if (lstSNQReportSummary.Count > 0)
                {
                    return Ok(lstSNQReportSummary);
                }
                else
                {
                    _log4net.Info("Enquiry  data not found for Report");
                    return Ok("Enquiry data not found for Report");
                }


            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }



        //Daily quotation report for MSD Enquiry
        [Route("GetMSDDaily_Report")]
        [HttpPost]
        public async Task<ActionResult> GetMSDDaily_Report(DailyReport objDailyReport)
        {
            
        List<MSDReportSummary> lstSNQReportSummary = new List<MSDReportSummary>();

            
            try
            {

                var getEnquiryheaderTask = Task.Run(() => { lstSNQReportSummary = _msdReport.GetMSDDaily_Report(objDailyReport); });
                await Task.WhenAll(getEnquiryheaderTask);
                if (lstSNQReportSummary.Count > 0)
                {
                    return Ok(lstSNQReportSummary);
                   
                }
                else
                {
                    _log4net.Info("Enquiry  data not found for Report");
                    return Ok("Enquiry data not found for Report");
                }


            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }



        // daily quotation submit report for MSD Enquiry
        [Route("GetQuotationSubmitDaily_Report")]
        [HttpPost]
        public async Task<ActionResult> GetQuotationSubmitDaily_Report(DailyReport objDailyReport)
        {

            List<QuotationReportSummary> lstQuotationReportSummary = new List<QuotationReportSummary>();
            try
            {

                var getEnquiryheaderTask = Task.Run(() => { lstQuotationReportSummary = _quotationSubmitReport.GetQuotationDaily_Report(objDailyReport); });
                await Task.WhenAll(getEnquiryheaderTask);
                if (lstQuotationReportSummary.Count > 0)
                {
                    return Ok(lstQuotationReportSummary);
                }
                else
                {
                    _log4net.Info("Enquiry  data not found for Report");
                    return Ok("Enquiry data not found for Report");
                }


            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }

    }
}
