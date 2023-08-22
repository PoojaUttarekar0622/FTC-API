using Helper.Hub_Config;
using Helper.Interface;
using Helper.TimerFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FTC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignalRController : ControllerBase
    {
        private readonly IHubContext<SignalRHub> _hub;
        ITaskDtl _CountDetails;

        public SignalRController(IHubContext<SignalRHub> hub, ITaskDtl CountDetails)
        {
            _hub = hub;
            _CountDetails = CountDetails;
        }

        public IActionResult Get()
        {
            var timerManager = new TimerManager(() => _hub.Clients.All.SendAsync("transferstatusdata", _CountDetails.GetStatusCountForMSD(DateTime.Now.ToString(), DateTime.Now.ToString())));

            return Ok(new { Message = "Data Request Completed" });
        }
    }
}