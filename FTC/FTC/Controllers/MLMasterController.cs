using Helper.Interface;
using Helper.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Helper.Model.MLClassDeclarations;

namespace FTC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MLMasterController : ControllerBase
    {
        static readonly log4net.ILog _log4net = log4net.LogManager.GetLogger(typeof(MLMaster));
        private readonly ILogger<MLMasterController> _logger;
        IMLMaster _EnquiryDetails;
        IUser _userDetails;
      
        public MLMasterController(IMLMaster EnquiryDetails, IUser userDetails)
        {
            _EnquiryDetails = EnquiryDetails;
            _userDetails = userDetails;
        }

        [Route("GetMLMasterData")]
        [HttpPost]
        public async Task<ActionResult> GetMLMasterData(MLData objmldata)
        {

            List<MLMasterData1> lstMLData = new List<MLMasterData1>();
            try
            {

                if (User.Identity.IsAuthenticated)
                {

                    var getAllUserTask = Task.Run(() => { lstMLData = _EnquiryDetails.GetMLMasterData(objmldata); });
                    await Task.WhenAll(getAllUserTask);
                    return Ok(lstMLData);
                }
                else return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }

        [Route("GetIDwiseMLList")]
        [HttpPost]
        public async Task<ActionResult> GetIDwiseMLList(MLData objmldata)
        {

            List<MLData2> lstMLData = new List<MLData2>();
            try
            {

                if (User.Identity.IsAuthenticated)
                {

                    var getAllUserTask = Task.Run(() => { lstMLData = _EnquiryDetails.GetIDwiseMLList(objmldata); });
                    await Task.WhenAll(getAllUserTask);
                    return Ok(lstMLData);
                }
                else return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }


        [HttpPost]
        [Route("DeleteMlMaster")]
        public async Task<ActionResult> DeleteMlMaster(List<MLData> endtl)
        {
            Message obj = new Message();
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var deleteitemTask = Task.Run(() =>
                    {
                        obj = _EnquiryDetails.DeleteMlMaster(endtl);
                    });
                    await Task.WhenAll(deleteitemTask);

                    if (obj.result == "Error while Deleting Enquiry")
                    {
                        _log4net.Info("Error while Deleting Enquiry");
                        return Ok(obj);
                    }
                    else
                    {
                        return Ok(obj);
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

        [Route("SaveMLMaster")]
        [HttpPost]
        public async Task<ActionResult> SaveMLMaster(List<MLData> objmldata)
        {

            Message objData = new Message();
            try
            {

                if (User.Identity.IsAuthenticated)
                {

                    var getAllUserTask = Task.Run(() => { objData = _EnquiryDetails.SaveMLMaster(objmldata); });
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

        [Route("GetMLProcessType")]
        [HttpPost]
        public async Task<ActionResult> GetMLProcessType()
        {

            List<MLProcessType> lstMLData = new List<MLProcessType>();
            try
            {

                if (User.Identity.IsAuthenticated)
                {

                    var getAllUserTask = Task.Run(() => { lstMLData = _EnquiryDetails.GetMLProcessType(); });
                    await Task.WhenAll(getAllUserTask);
                    return Ok(lstMLData);
                }
                else return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }

        [Route("GetMLType")]
        [HttpPost]
        public async Task<ActionResult> GetMLType(MLType objmldata)
        {

            List<MLType> lstMLData = new List<MLType>();
            try
            {

                if (User.Identity.IsAuthenticated)
                {

                    var getAllUserTask = Task.Run(() => { lstMLData = _EnquiryDetails.GetMLType(objmldata); });
                    await Task.WhenAll(getAllUserTask);
                    return Ok(lstMLData);
                }
                else return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }

        [Route("GetPagignationData")]
        [HttpPost]
        public async Task<ActionResult> GetMLType(PagignationData objmldata)
        {

            List<PagignationData> lstMLData = new List<PagignationData>();
            try
            {

                if (User.Identity.IsAuthenticated)
                {

                    var getAllUserTask = Task.Run(() => { lstMLData = _EnquiryDetails.GetPagignationData(objmldata); });
                    await Task.WhenAll(getAllUserTask);
                    return Ok(lstMLData);
                }
                else return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }


        [Route("RegisterNewImpa")]
        [HttpPost]
        public async Task<ActionResult> RegisterNewImpa(MLData objmldata)
        {

            Message objData = new Message();
            try
            {

                if (User.Identity.IsAuthenticated)
                {

                    var getAllUserTask = Task.Run(() => { objData = _EnquiryDetails.RegisterNewImpa(objmldata); });
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

        [Route("GetMLDataBySearch")]
        [HttpPost]
        public async Task<ActionResult> GetMLDataBySearch(searchdata objsearchdata)
        {

            List<MLMasterData1> lstMLData = new List<MLMasterData1>();
            try
            {

                if (User.Identity.IsAuthenticated)
                {

                    var getAllUserTask = Task.Run(() => { lstMLData = _EnquiryDetails.GetMLDataBySearch(objsearchdata); });
                    await Task.WhenAll(getAllUserTask);
                    return Ok(lstMLData);
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
