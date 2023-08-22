using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using static Helper.Model.TaskClass;

using Helper.Interface;

namespace FTC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        
        static readonly log4net.ILog _log4net = log4net.LogManager.GetLogger(typeof(Task));
        private readonly ILogger<TaskController> _logger;
        ITaskDtl _CountDetails;

        public TaskController(ITaskDtl CountDetails)
        {
            _CountDetails = CountDetails;
        }


        [HttpPost]
        [Route("GetStatusCountForMSD")]
        public async Task<ActionResult> GetStatusCountMSD(searchdata searchdata)
        {
           
            try
            {
                StatusMSD objStatus = new StatusMSD();

                if (User.Identity.IsAuthenticated)
                {
                    var getstatuscountTask = Task.Run(() => { objStatus = _CountDetails.GetStatusCountForMSD(searchdata.FromDate, searchdata.ToDate); });
                    await Task.WhenAll(getstatuscountTask);
                    
                    if (getstatuscountTask != null)
                    {
                        return Ok(objStatus);
                    }
                    else
                    {
                        _log4net.Info("Data Not Available");
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


        [HttpPost]
        [Route("GetStatusCountForSNQ")]
        public async Task<ActionResult> GetStatusCountSNQ(searchdata searchdata)
        {
            StatusSNQ objStatus = new StatusSNQ();
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var getstatuscountTask = Task.Run(() => { objStatus = _CountDetails.GetStatusCountForSNQ(searchdata.FromDate, searchdata.ToDate); });
                    await Task.WhenAll(getstatuscountTask);
                   
                    if (getstatuscountTask != null)
                    {
                        return Ok(objStatus);
                    }
                    else
                    {
                        _log4net.Info("Data Not Available");
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
