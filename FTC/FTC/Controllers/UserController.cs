using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Helper.Interface;
using static Helper.Model.LoginAuthClsDeclaration;

namespace FTC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        static readonly log4net.ILog _log4net = log4net.LogManager.GetLogger(typeof(UserController));
        private readonly ILogger<UserController> _logger;
        IUser _UserDetails;

        public UserController(IUser UserDetails)
        {
            _UserDetails = UserDetails;
        }

        /// get user details by user name wise
        [HttpGet]
        [Route("Getsingleuser/{LoginName}")]
        public async Task<ActionResult> GetUserdataByLoginName(string LoginName)
        {
            User objuserMaster = new User();
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var getAllUserDataByNameTask = Task.Run(() => { objuserMaster = _UserDetails.GetSingleUserDetail(LoginName); });
                    await Task.WhenAll(getAllUserDataByNameTask);

                    return Ok(objuserMaster);
                }
                else return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }
        
        /// delete single user details by user name wise
        [HttpDelete]
        [Route("Deleteuser/{LoginNameDelete}")]
        public async Task<ActionResult> DeleteUser(long LoginNameDelete)
        {
            User objuser = new User();
            Message obj = new Message();
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    string ResultMessage = string.Empty;
                    var deleteUserTask = Task.Run(() => { obj = _UserDetails.DeleteSingleUser(LoginNameDelete); });
                    await Task.WhenAll(deleteUserTask);
                    return Ok(obj);
                }
                else return Unauthorized();
            }
            catch (Exception ex)
            {

                return Ok(ex.Message);

            }
        }

        /// save single user details
        [HttpPost]
        [Route("SaveUser")]
        public async Task<ActionResult> SaveUser([FromBody] User inobjuserMaster)
        {
            User objuser = new User();
            Message obj = new Message();
            try
            {

                string ResultMessage = string.Empty;

                var saveUserTask = Task.Run(() => { obj = _UserDetails.SaveUser(inobjuserMaster); });
                await Task.WhenAll(saveUserTask);
                return Ok(obj);

            }

            catch (Exception ex)
            {
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }

        [HttpPut]
        [Route("Updateuser/{LoginName}")]
        public async Task<ActionResult> UpdateUser(string LoginName, User inobjuserMaster)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    string ResultMessage = string.Empty;
                    var updateUserTask = Task.Run(() => { ResultMessage = _UserDetails.UpdateUserDetails(inobjuserMaster); });
                    await Task.WhenAll(updateUserTask);
                    return Ok(ResultMessage);
                }
                else return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }

        [Route("Getallusersdata")]
        [HttpGet]
        public async Task<ActionResult> GetAllUserdata()
        {
            List<User> lstuserMaster = new List<User>();
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var getAllUserTask = Task.Run(() => { lstuserMaster = _UserDetails.GetAllUserDetail(); });
                    await Task.WhenAll(getAllUserTask);
                    if (lstuserMaster.Count > 0)
                    {
                        return Ok(lstuserMaster);
                    }
                    else
                    {
                        return Ok("No user avaialable");
                    }
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
