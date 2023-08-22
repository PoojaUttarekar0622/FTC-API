using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Helper.Model.MSDClassDeclarations;
using Microsoft.Extensions.Logging;
using Helper.Interface;


namespace FTC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MSDEnquiryController : ControllerBase
    {

        static readonly log4net.ILog _log4net = log4net.LogManager.GetLogger(typeof(MSDEnquiryController));
        private readonly ILogger<MSDEnquiryController> _logger;
        IMSDEnquiry _EnquiryDetails;
        IUser _userDetails;

        public MSDEnquiryController(IMSDEnquiry EnquiryDetails, IUser userDetails)
        {
            _EnquiryDetails = EnquiryDetails;
            _userDetails = userDetails;
        }

        //Get Enquiry headers for Portal
        [Route("Summary/{status}")]
        [HttpGet]
        public async Task<ActionResult> GetEnquiryHeader(int status)
        {

            List<Enquiryheaderdata> lstEnqHeader = new List<Enquiryheaderdata>();

            try
            {
                if (User.Identity.IsAuthenticated)
                {

                    var getEnquiryheaderTask = Task.Run(() => { lstEnqHeader = _EnquiryDetails.GetEnquiryHeader(status); });
                    await Task.WhenAll(getEnquiryheaderTask);

                    if (lstEnqHeader.Count > 0)
                    {
                        return Ok(lstEnqHeader);
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

        //Get Enquirydata for specified enquiry for Portal
        [HttpPost]
        [Route("Details")]
        public async Task<ActionResult> GetEnquiryDetails(Enquiryheaderdata endtl)
        {
            Enquiryheaderdata lstEnqDetails = new Enquiryheaderdata();

            try
            {
               
                var getEnquiryDetailsTask = Task.Run(() => { lstEnqDetails = _EnquiryDetails.GetEnquiryDetails(endtl.PK_MSDENQUIRY_HDR_ID); });
                await Task.WhenAll(getEnquiryDetailsTask);

                if (lstEnqDetails != null)
                {
                    return Ok(lstEnqDetails);
                }
                else
                {
                    _log4net.Info("Enquiry Details not found");
                    return Ok("Data Not avaialable");
                }
               
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }

        //Create Enquiry From Email Monitor bot
        [HttpPost]
        [Route("CreateEnquiry")]
        public async Task<ActionResult> Insertenquirydata([FromBody] Enquiryheader inobjuserMaster)
        {

            Enquiryheader objuser = new Enquiryheader();
            MessageMSD obj = new MessageMSD();
            try
            {

                string ResultMessage = string.Empty;
                var saveEnquiryTask = Task.Run(() => { obj = _EnquiryDetails.InsertEnquiry(inobjuserMaster); });
                await Task.WhenAll(saveEnquiryTask);

                if (obj.result == "Error while Creating Enquiry")
                {
                   
                    _log4net.Error("Error while Creating Enquiry" + Newtonsoft.Json.JsonConvert.SerializeObject(inobjuserMaster));

                    return Ok(obj);
                }
                else
                {
                    _log4net.Info("Data Saved into database" + Newtonsoft.Json.JsonConvert.SerializeObject(inobjuserMaster));
                    return Ok(obj);
                }


            }

            catch (Exception ex)
            {
                _log4net.Error(ex);
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }

        //Update Enquiry From Portal
        [HttpPost]
        [Route("UpdateData")]
        public async Task<ActionResult> Updateenquirydata([FromBody] Enquiryheader inobjuserMaster)
        {

            Enquiryheader objuser = new Enquiryheader();
            MessageMSD obj = new MessageMSD();
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    string ResultMessage = string.Empty;

                    int Userid = _userDetails.GetUserDetailsFromToken(HttpContext.Request.Headers["Authorization"].ToString());
                    inobjuserMaster.verifiedBy = Userid.ToString();
                    inobjuserMaster.correctedBy = Userid.ToString();

                    var updateEnquiryTask = Task.Run(() => { obj = _EnquiryDetails.UpdateEnquiry(inobjuserMaster); });
                    await Task.WhenAll(updateEnquiryTask);
                    if (obj.result == "Error while Updating Enquiry")
                    {
                       
                        _log4net.Error("Error while Updating Enquiry" + Newtonsoft.Json.JsonConvert.SerializeObject(inobjuserMaster));

                        return Ok(obj);
                    }
                    else
                    {
                        _log4net.Error("Updating Enquiry" + Newtonsoft.Json.JsonConvert.SerializeObject(inobjuserMaster));

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
        [Route("DeleteItem")]
        public async Task<ActionResult> DeleteDtlItem(List<Enquirydetailsdata> endtl)
        {
            List<Enquirydetailsdata> lstEnqDetails = new List<Enquirydetailsdata>();
            MessageMSD obj = new MessageMSD();
           
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var deleteitemTask = Task.Run(() =>
                    {
                        obj = _EnquiryDetails.DeleteItems(endtl);
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

        //Update Enquiry From AS400 Bot
        [HttpPost]
        [Route("UpdateStatus")]
        public async Task<ActionResult> UpdateEnquiryStatus([FromBody] EnquiryheaderdataForas400 inobjuserMaster)
        {

            EnquiryheaderdataForas400 objuser = new EnquiryheaderdataForas400();
            MessageMSD obj = new MessageMSD();
            try
            {
               
                string ResultMessage = string.Empty;

                var updateEnquiryTask = Task.Run(() => { obj = _EnquiryDetails.UpdateEnqStatus(inobjuserMaster); });
                await Task.WhenAll(updateEnquiryTask);
                if (obj.result == "Error while Updating Enquiry from AS400")
                {
                  
                    _log4net.Error("Error while Updating Enquiry from AS400" + Newtonsoft.Json.JsonConvert.SerializeObject(inobjuserMaster));

                    return Ok(obj);
                }
                else
                {
                    _log4net.Error("Updating Enquiry from AS400" + Newtonsoft.Json.JsonConvert.SerializeObject(inobjuserMaster));

                    return Ok(obj);
                }
              
            }

            catch (Exception ex)
            {
                _log4net.Error(ex);
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }

        //Get Enquiries for AS400 Bot
        [HttpPost]
        [Route("GetDataForAS400")]
        public async Task<ActionResult> GetDataForAS400()
        {

            List<EnquiryheaderdataForas400> lstEnqHeader = new List<EnquiryheaderdataForas400>();

            try
            {
               
                var getEnquiryDetailsTask = Task.Run(() => { lstEnqHeader = _EnquiryDetails.GetDetailsForAs400(); });
                await Task.WhenAll(getEnquiryDetailsTask);
                if (lstEnqHeader.Count > 0)
                {
                    _log4net.Info("Get MSD Enquiry  data for AS400" + Newtonsoft.Json.JsonConvert.SerializeObject(lstEnqHeader));

                    return Ok(lstEnqHeader);

                   
                }
                else
                {
                    _log4net.Info("Error while getting data for AS400");
                    return Ok("Data Not avaialable");
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }

        //Get Not Started Enquiry List
        [HttpPost]
        [Route("NotStartedtaskEnquiryList")]
        public async Task<ActionResult> TaskMsdEnquiry(searchdata objserachdata)
        {
            List<Enquiryheaderdata> lstEnqHeader = new List<Enquiryheaderdata>();
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    if (objserachdata.CustNameShipNameRefNo == null)
                    {
                        objserachdata.CustNameShipNameRefNo = "";
                    }
                    var getEnquiryTask = Task.Run(() => { lstEnqHeader = _EnquiryDetails.GetTaskList(objserachdata); });
                    await Task.WhenAll(getEnquiryTask);
                    if (lstEnqHeader != null)
                    {
                        return Ok(lstEnqHeader);
                    }
                    else
                    {
                        _log4net.Info("Enquiry Details not found");
                        return Ok("Enquiry Details not found");
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

        //Get Error Enquiry List
        [HttpPost]
        [Route("ErrortaskEnquiryList")]
        public async Task<ActionResult> ErrorTaskMsdEnquiry(searchdata objserachdata)
        {
            List<Enquiryheaderdata> lstEnqHeader = new List<Enquiryheaderdata>();
          
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    if (objserachdata.CustNameShipNameRefNo == null)
                    {
                        objserachdata.CustNameShipNameRefNo = "";
                    }
                    var getEnquiryTask = Task.Run(() => { lstEnqHeader = _EnquiryDetails.GetErrorTaskList(objserachdata); });
                    await Task.WhenAll(getEnquiryTask);
                    if (lstEnqHeader != null)
                    {
                        return Ok(lstEnqHeader);
                    }
                    else
                    {
                        _log4net.Info("Enquiry Details not found");
                        return Ok("Enquiry Details not found");
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

        //Get Completed Enquiry List
        [HttpPost]
        [Route("CompletedtaskEnquiryList")]
        public async Task<ActionResult> CompletedTaskMsdEnquiry(searchdata objserachdata)
        {
            List<Enquiryheaderdata> lstEnqHeader = new List<Enquiryheaderdata>();
           
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    if (objserachdata.CustNameShipNameRefNo == null)
                    {
                        objserachdata.CustNameShipNameRefNo = "";
                    }
                    var getEnquiryTask = Task.Run(() => { lstEnqHeader = _EnquiryDetails.GetCompletedTaskList(objserachdata); });
                    await Task.WhenAll(getEnquiryTask);
                    if (lstEnqHeader != null)
                    {
                        _log4net.Info("Enquiry Details not found");
                        return Ok(lstEnqHeader);
                    }
                    else
                    {
                        _log4net.Info("Enquiry Details not found");
                        return Ok("Enquiry Details not found");
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

        //Update Ownership for Enquiry
        [HttpPost]
        [Route("UpdateOwnership")]
        public async Task<ActionResult> Updateenquiryownership(EnqOwnership objowner)
        {
           
            EnqOwnership objownership = new EnqOwnership();
            MessageMSD obj = new MessageMSD();
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    int Userid = _userDetails.GetUserDetailsFromToken(HttpContext.Request.Headers["Authorization"].ToString());
                    objowner.Ownership = Userid;
                    string ResultMessage = string.Empty;

                    var updateOwnershipTask = Task.Run(() => { obj = _EnquiryDetails.Updateownership(objowner); });
                    await Task.WhenAll(updateOwnershipTask);
                    if (obj.result == "Error while taking Ownership of Enquiry")
                    {
                        _log4net.Info("Error while taking Ownership of Enquiry");
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


        //Release Ownership for Enquiry
        [HttpPost]
        [Route("ReleaseOwnership")]
        public async Task<ActionResult> Releaseenquiryownership(EnqOwnership objowner)
        {
          
            EnqOwnership objownership = new EnqOwnership();
            MessageMSD obj = new MessageMSD();
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    int Userid = _userDetails.GetUserDetailsFromToken(HttpContext.Request.Headers["Authorization"].ToString());
                    objowner.Ownership = Userid;
                    string ResultMessage = string.Empty;

                    var updateOwnershipTask = Task.Run(() => { obj = _EnquiryDetails.Releaseownership(objowner); });
                    await Task.WhenAll(updateOwnershipTask);
                    if (obj.result == "Error while releasing Ownership of Enquiry")
                    {
                        _log4net.Info("Error while releasing Ownership of Enquiry");
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

        //Get MSD RFQURL For Web
        [HttpPost]
        [Route("GetURLForWeb")]
        public async Task<ActionResult> GetURLForWeb(EnquiryheaderForMSDBOT endtl)
        {
            EnquiryheaderForMSDBOT lstEnqRefNO = new EnquiryheaderForMSDBOT();

            try
            {
                
                var getEnquiryRFQNOTask = Task.Run(() => { lstEnqRefNO = _EnquiryDetails.GetRFQURL(endtl.enqrefNo); });
                await Task.WhenAll(getEnquiryRFQNOTask);

                if (lstEnqRefNO != null)
                {
                    return Ok(lstEnqRefNO);
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

        //Update Price And Leadtime For ML From website
        [HttpPost]
        [Route("UpdatePriceLeadtimeFromWeb")]
        public async Task<ActionResult> UpdatePriceLeadtime([FromBody] EnquiryheaderForMSDBOT inobjPriceleadtime)
        {

            EnquiryheaderForMSDBOT objuser = new EnquiryheaderForMSDBOT();
            MessageMSD obj = new MessageMSD();
            try
            {

                var updatePriceLeadtimeTask = Task.Run(() => { obj = _EnquiryDetails.UpdatePriceandLeadtime(inobjPriceleadtime); });

                await Task.WhenAll(updatePriceLeadtimeTask);

                if (obj.result == "Error while Updating Price And Leadtime")
                {
                   
                    _log4net.Info("EError while Updating Price And Leadtime" + Newtonsoft.Json.JsonConvert.SerializeObject(inobjPriceleadtime));

                    return Ok(obj);
                }
                else
                {
                    _log4net.Info("update price lead time" + Newtonsoft.Json.JsonConvert.SerializeObject(inobjPriceleadtime));

                    return Ok(obj);
                }

            }

            catch (Exception ex)
            {
                _log4net.Error(ex);
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }


        //Get User Ownership Details for Portal
        [Route("Getuserownershipdtl/{PKHDRID}")]
        [HttpGet]
        public async Task<ActionResult> GetUserownershipdtl(int PKHDRID)
        {

            EnqOwnership lstEnqHeader = new EnqOwnership();
            try
            {
                if (User.Identity.IsAuthenticated)
                {

                    var getuserownershipdtlTask = Task.Run(() => { lstEnqHeader = _EnquiryDetails.Getuserownershipdtl(PKHDRID); });
                    await Task.WhenAll(getuserownershipdtlTask);
                    if (lstEnqHeader != null)
                    {
                        return Ok(lstEnqHeader);
                    }
                    else
                    {
                        _log4net.Info("Enquiry header data not found");
                        return Ok("Enquiry header data not found");
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



        //Update Enquiry From Portal
        [HttpPost]
        [Route("UpdateSaveAsData")]
        public async Task<ActionResult> UpdateSaveAsEnquiry([FromBody] Enquiryheader inobjuserMaster)
        {

            Enquiryheader objuser = new Enquiryheader();
            MessageMSD obj = new MessageMSD();
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    string ResultMessage = string.Empty;

                    int Userid = _userDetails.GetUserDetailsFromToken(HttpContext.Request.Headers["Authorization"].ToString());
                    inobjuserMaster.verifiedBy = Userid.ToString();
                    inobjuserMaster.correctedBy = Userid.ToString();

                    var updateEnquiryTask = Task.Run(() => { obj = _EnquiryDetails.UpdateSaveAsEnquiry(inobjuserMaster); });
                    await Task.WhenAll(updateEnquiryTask);
                    if (obj.result == "Error while Updating Enquiry")
                    {
                      
                        _log4net.Error("Error while Updating Enquiry" + Newtonsoft.Json.JsonConvert.SerializeObject(inobjuserMaster));

                        return Ok(obj);
                    }
                    else
                    {
                        _log4net.Error("Updating Enquiry" + Newtonsoft.Json.JsonConvert.SerializeObject(inobjuserMaster));

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


        //Get Enquiries for AS400 Bot
        [HttpPost]
        [Route("VerifyRFQ")]
        public async Task<ActionResult> VerifyRFQ(MSDRFQData errorRFQ)
        {

           MessageMSD objMessageMSD = new MessageMSD();

            try
            {
               
                var getEnquiryDetailsTask = Task.Run(() => { objMessageMSD = _EnquiryDetails.VerifyRFQ(errorRFQ.errorRFQ); });
                await Task.WhenAll(getEnquiryDetailsTask);
                return Ok(objMessageMSD);
               
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }

    }
}
