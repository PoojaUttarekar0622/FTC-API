using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

using System.Threading.Tasks;
using static Helper.Model.LoginAuthClsDeclaration;
using Helper.Interface;
using Microsoft.Extensions.Logging;

namespace FTC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        static readonly log4net.ILog _log4net = log4net.LogManager.GetLogger(typeof(LoginController));
        private readonly ILogger<LoginController> _logger;
        ILoginAuth _LoginDetails;

        public LoginController(ILoginAuth LoginDetails)
        {
            _LoginDetails = LoginDetails;
        }


        [Route("Authenticate")]
        [HttpPost]
        public async Task<ActionResult> GetLoginDetails([FromBody] User obj)
        {

            User objuserMaster = new User();
            try
            {
               
                var getLoginDetailsTask =
                   Task.Run(() =>
                   {
                       objuserMaster = _LoginDetails.Authenticate(obj.userName, obj.password);
                   });
                await (getLoginDetailsTask);

                if (objuserMaster.userId > 0)
                {
                   
                    return Ok(objuserMaster);

                }
                else
                {
                    return Ok(objuserMaster);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }



        [Route("RefereshToken")]
        [HttpPost]
        public async Task<ActionResult> GetRefereshToken()
        {

            User objuserMaster = new User();
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    string ResultMessage = string.Empty;
                    int Userid = _LoginDetails.GetUserDetailsFromToken(HttpContext.Request.Headers["Authorization"].ToString());

                    var getLoginDetailsTask =
                   Task.Run(() =>
                   {
                       objuserMaster = _LoginDetails.RefershTokens(Userid);
                   });
                    await (getLoginDetailsTask);

                    if (objuserMaster.userId > 0)
                    {
                       
                        return Ok(objuserMaster);

                    }
                    else
                    {
                        return Ok(objuserMaster);
                    }
                }
         
                else return Unauthorized();
        }
            catch (Exception ex)
            {
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }

        /// logout the portal using token id
        [Route("Logout")]
        [HttpPost]
        public async Task<ActionResult> UserLogout([FromBody] User obj)
        {
            User objuserMaster = new User();
            string ResultMessage = string.Empty;
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var getLogoutDetailsTask = Task.Run(() => { objuserMaster.resultMessage = _LoginDetails.Logout(obj); });
                    await Task.WhenAll(getLogoutDetailsTask);
                    if (objuserMaster.resultMessage == "Invalid token")
                    {
                        _log4net.Info("Invalid Token");
                        return Ok(objuserMaster.resultMessage);
                    }
                    else
                    {
                        return Ok(objuserMaster);
                    }
                   
                }
                else return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(HttpContext.Response.StatusCode, ex.Message);
            }
        }


        [Route("ChangePassword")]
        [HttpPost]
        public async Task<ActionResult> ChangePassword([FromBody] UserChangePassword obj)
        {
            UserChangePassword objuserMaster = new UserChangePassword();
            string ResultMessage = string.Empty;
            try
            {
              
                if (User.Identity.IsAuthenticated)
                {
                    int Userid = _LoginDetails.GetUserDetailsFromToken(HttpContext.Request.Headers["Authorization"].ToString());
                    obj.userId = Userid;
                    var getChanegPwdDetailsTask = Task.Run(() => { objuserMaster.resultMessage = _LoginDetails.ChangePassword(obj); });
                    await Task.WhenAll(getChanegPwdDetailsTask);
                    if (objuserMaster.resultMessage == "Old Password is Invalid")
                    {
                        _log4net.Info("Old Password is Invalid");
                        return Ok(objuserMaster);
                    }
                    else
                    {
                        return Ok(objuserMaster);
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
