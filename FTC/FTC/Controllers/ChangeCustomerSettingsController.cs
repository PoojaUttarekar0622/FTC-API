using Helper.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static Helper.Model.SourceTypeClassDeclarations;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Helper.Model;
using static Helper.Model.ChangeCustomerSettingsClsDeclarations;
using static Helper.Model.MLClassDeclarations;

namespace FTC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChangeCustomerSettingsController : ControllerBase
    {
        static readonly log4net.ILog _log4net = log4net.LogManager.GetLogger(typeof(ChangeCustomerSettingsController));
        private readonly ILogger<ChangeCustomerSettingsController> _logger;
        IChangeCustomerSettings _CustomerSettingsDetails;
        IUser _userDetails;

        public ChangeCustomerSettingsController(IChangeCustomerSettings CustomerSettingsDetails, IUser userDetails)
        {
            _CustomerSettingsDetails = CustomerSettingsDetails;
            _userDetails = userDetails;
        }
        [Route("GeCustomerData")]
        [HttpGet]
        public async Task<ActionResult> GeCustomerData()
        {
            List<ChangeCustomerSettingsSummary> lstChangeCustomerSettings = new List<ChangeCustomerSettingsSummary>();
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var getEnquiryheaderTask = Task.Run(() => { lstChangeCustomerSettings = _CustomerSettingsDetails.GeCustomerData(); });
                    await Task.WhenAll(getEnquiryheaderTask);

                    if (lstChangeCustomerSettings != null)
                    {
                        return Ok(lstChangeCustomerSettings);
                    }
                    else
                    {
                        _log4net.Info("Enquiry header data not found");
                        return Ok("Data Not avaialable");
                    }
                }
                else return Unauthorized();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }
        [Route("UpdateCustomerData")]
        [HttpPost]
        public async Task<ActionResult> UpdateCustomerData(ChangeCustomerSettingsSummary objCustomerdata)
        {

            Message objData = new Message();
            try
            {

                if (User.Identity.IsAuthenticated)
                {

                    var getAllUserTask = Task.Run(() => { objData = _CustomerSettingsDetails.UpdateCustomerData(objCustomerdata); });
                    await Task.WhenAll(getAllUserTask);
                    return Ok(objData);
                }
                else return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }
    }
}
