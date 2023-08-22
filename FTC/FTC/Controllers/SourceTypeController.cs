using Helper.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Helper.Model.SourceTypeClassDeclarations;

namespace FTC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SourceTypeController : ControllerBase
    {
        static readonly log4net.ILog _log4net = log4net.LogManager.GetLogger(typeof(MESPASEventController));
        private readonly ILogger<SourceTypeController> _logger;
        ISourceType _EnquiryDetails;
        IUser _userDetails;


        public SourceTypeController(ISourceType EnquiryDetails, IUser userDetails)
        {
            _EnquiryDetails = EnquiryDetails;
            _userDetails = userDetails;
        }
        [Route("GetSourceTypeData")]
        [HttpGet]
        public async Task<ActionResult> GetSourceTypeData()
        {
            List<SourceTypeSummary> lstSourceTypeSummary = new List<SourceTypeSummary>();
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var getEnquiryheaderTask = Task.Run(() => { lstSourceTypeSummary = _EnquiryDetails.GeSourceTypeData(); });
                    await Task.WhenAll(getEnquiryheaderTask);

                    if (lstSourceTypeSummary != null)
                    {
                        return Ok(lstSourceTypeSummary);
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
    }
}
