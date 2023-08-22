using Helper.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Helper.Model.MESPASEventClassDeclarations;
using static Helper.Model.MSDClassDeclarations;

namespace FTC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MESPASEventController : ControllerBase
    {
        static readonly log4net.ILog _log4net = log4net.LogManager.GetLogger(typeof(MESPASEventController));
        private readonly ILogger<MESPASEventController> _logger;
        IMESPASEvent _EnquiryDetails;
        IUser _userDetails;


        public MESPASEventController(IMESPASEvent EnquiryDetails, IUser userDetails)
        {
            _EnquiryDetails = EnquiryDetails;
            _userDetails = userDetails;
        }

        [Route("GetEventDetails")]
        [HttpGet]
        public async Task<ActionResult> GetEventDetails()
        {
            long eventId = 0;
            try
            {

                var getEnquiryheaderTask = Task.Run(() => { eventId = _EnquiryDetails.GetlatestEventId(); });
                await Task.WhenAll(getEnquiryheaderTask);

                if (eventId != null)
                {
                    return Ok(eventId);
                }
                else
                {
                    _log4net.Info("Enquiry header data not found");
                    return Ok("Data Not avaialable");
                }


            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }

        //Get MSD RFQURL For Web
        [HttpPost]
        [Route("UpdateEventId/{eventId}")]
        public async Task<ActionResult> UpdateEventId(long eventId)
        {
            Helper.Model.MSDClassDeclarations.MessageMSD obj = new MessageMSD();

            try
            {
               
                var getEnquiryRFQNOTask = Task.Run(() => { obj = _EnquiryDetails.UpdateEventId(eventId); });
                await Task.WhenAll(getEnquiryRFQNOTask);

                if (obj != null)
                {
                    return Ok(obj);
                }
                else
                {
                    _log4net.Info("Enquiry Url not found for particular RFQNO");
                    return Ok("Enquiry Url not found for particular RFQNO");
                }
              

            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }


        [Route("GetActiveCustomer/{owner}")]
        [HttpGet]
        public async Task<ActionResult> GetActiveCustomer(string  owner)
        {
          
            try
            {

                var getEnquiryheaderTask = Task.Run(() => { owner = _EnquiryDetails.GetCustomerMapping(owner); });
                await Task.WhenAll(getEnquiryheaderTask);

                if (owner != null)
                {
                    return Ok(owner);
                }
                else
                {
                    _log4net.Info("Enquiry header data not found");
                    return Ok("Data Not avaialable");
                }


            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }


        [Route("GetMSDItemsList")]
        [HttpGet]
        public async Task<ActionResult> GetMSDItemsList()
        {

            List<MSDItemData> lstMSDItemData = new List<MSDItemData>();
            try
            {

                
                    var getAllUserTask = Task.Run(() => { lstMSDItemData = _EnquiryDetails.GetMSDItemsList(); });
                    await Task.WhenAll(getAllUserTask);
                    return Ok(lstMSDItemData);
              
            }
            catch (Exception ex)
            {
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }

        [Route("GetSNQItemsList")]
        [HttpGet]
        public async Task<ActionResult> GetSNQItemsList()
        {

            List<SNQItemData> lstSNQItemData = new List<SNQItemData>();
            try
            {

               
                    var getAllUserTask = Task.Run(() => { lstSNQItemData = _EnquiryDetails.GetSNQItemsList(); });
                    await Task.WhenAll(getAllUserTask);
                    return Ok(lstSNQItemData);
               
            }
            catch (Exception ex)
            {
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }


        [HttpPost]
        [Route("DeleteMsdItems")]
        public async Task<ActionResult> DeleteMsdItems(List<MSDItemData> endtl)
        {
            Message obj = new Message();
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var deleteitemTask = Task.Run(() =>
                    {
                        obj = _EnquiryDetails.DeleteMSDItemsData(endtl);
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

        [HttpPost]
        [Route("DeleteSNQItems")]
        public async Task<ActionResult> DeleteSNQItems(List<SNQItemData> endtl)
        {
            Message obj = new Message();
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var deleteitemTask = Task.Run(() =>
                    {
                        obj = _EnquiryDetails.DeleteSNQItemsData(endtl);
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


        [Route("SaveMSDItems")]
        [HttpPost]
        public async Task<ActionResult> SaveMSDItems(List<MSDItemData> objmldata)
        {

            Message objData = new Message();
            try
            {

                if (User.Identity.IsAuthenticated)
                {

                    var getAllUserTask = Task.Run(() => { objData = _EnquiryDetails.SaveMSDItemsData(objmldata); });
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


        [Route("SaveSNQItems")]
        [HttpPost]
        public async Task<ActionResult> SaveSNQItems(List<SNQItemData> objmldata)
        {

            Message objData = new Message();
            try
            {

                if (User.Identity.IsAuthenticated)
                {

                    var getAllUserTask = Task.Run(() => { objData = _EnquiryDetails.SaveSNQItemsData(objmldata); });
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


        [Route("GetMakerList")]
        [HttpGet]
        public async Task<ActionResult> GetMakerList()
        {

            List<MakerData> lstMakerData = new List<MakerData>();
            try
            {


                var getAllUserTask = Task.Run(() => { lstMakerData = _EnquiryDetails.GetMakerList(); });
                await Task.WhenAll(getAllUserTask);
                return Ok(lstMakerData);

            }
            catch (Exception ex)
            {
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }

        [Route("GetEquipmentList")]
        [HttpGet]
        public async Task<ActionResult> GetEquipmentList()
        {

            List<EquipmentData> lstGetEquipmentList = new List<EquipmentData>();
            try
            {


                var getAllUserTask = Task.Run(() => { lstGetEquipmentList = _EnquiryDetails.GetEquipmentList(); });
                await Task.WhenAll(getAllUserTask);
                return Ok(lstGetEquipmentList);

            }
            catch (Exception ex)
            {
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }

    }
}
