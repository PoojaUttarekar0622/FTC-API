﻿using Helper.Interface;
using Helper.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Helper.Model.MESPASClassDeclarations;


namespace FTC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MESPASEnquiryController : ControllerBase
    {

        static readonly log4net.ILog _log4net = log4net.LogManager.GetLogger(typeof(MSDEnquiryController));
        private readonly ILogger<MESPASEnquiryController> _logger;
        IMESPASEnquiry _EnquiryDetails;
        IUser _userDetails;


        public MESPASEnquiryController(IMESPASEnquiry EnquiryDetails, IUser userDetails)
        {
            _EnquiryDetails = EnquiryDetails;
            _userDetails = userDetails;
        }
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
        public async Task<ActionResult> GetEnquiryDetails(Enquiryheader endtl)
        {
            Enquiryheader lstEnqDetails = new Enquiryheader();

            try
            {

                var getEnquiryDetailsTask = Task.Run(() => { lstEnqDetails = _EnquiryDetails.GetEnquiryDetails(endtl.PK_MESPASENQUIRY_HDR_ID); });
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
        [HttpPost]
        [Route("CreateEnquiry")]
        public async Task<ActionResult> Insertenquirydata([FromBody] Enquiryheader inobjuserMaster)
        {

            Enquiryheader objuser = new Enquiryheader();
            Message obj = new Message();
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
            Message obj = new Message();
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    string ResultMessage = string.Empty;

                    int Userid = _userDetails.GetUserDetailsFromToken(HttpContext.Request.Headers["Authorization"].ToString());
                    inobjuserMaster.verifiedBy = Userid.ToString();
                    inobjuserMaster.correctedBy = Userid.ToString();
                    inobjuserMaster.rejectedBy = Userid.ToString();

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
        [Route("GetDepartmentData")]
        [HttpGet]
        public async Task<ActionResult> GetDepartmentData()
        {

            List<Department> lstDepartment = new List<Department>();
            try
            {


                var getAllUserTask = Task.Run(() => { lstDepartment = _EnquiryDetails.GetDeptData(); });
                await Task.WhenAll(getAllUserTask);
                return Ok(lstDepartment);

            }
            catch (Exception ex)
            {
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


        [HttpPost]
        [Route("DeleteItem")]
        public async Task<ActionResult> DeleteDtlItem(List<EnquiryDetails> endtl)
        {
            List<EnquiryDetails> lstEnqDetails = new List<EnquiryDetails>();
            Message obj = new Message();

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


        //Update Enquiry From Portal
        [HttpPost]
        [Route("UpdateSaveAsData")]
        public async Task<ActionResult> UpdateSaveAsEnquiry([FromBody] Enquiryheader inobjuserMaster)
        {

            Enquiryheader objuser = new Enquiryheader();
            Message obj = new Message();
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



        //Update Ownership for Enquiry
        [HttpPost]
        [Route("UpdateOwnership")]
        public async Task<ActionResult> Updateenquiryownership(EnqOwnership objowner)
        {

            EnqOwnership objownership = new EnqOwnership();
            Message obj = new Message();
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
            Message obj = new Message();
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

    }
}
