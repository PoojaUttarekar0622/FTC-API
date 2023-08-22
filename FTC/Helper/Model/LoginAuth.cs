using System;
using System.Collections.Generic;
using System.Text;
using static Helper.Data.DataContextClass;
using static Helper.Model.LoginAuthClsDeclaration;
using Helper.Interface;
using Helper.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Linq;

namespace Helper.Model
{
    public class LoginAuth: ILoginAuth
    {
        #region created object of connection and configuration class
        public DataConnection _datacontext;
        private IConfiguration _Configuration;
        #endregion

        #region created controller of that class
        public LoginAuth(DataConnection datacontext, IConfiguration configuration)
        {
            _datacontext = datacontext;
            _Configuration = configuration;
        }
        #endregion

        #region Authenticate to portal using below method
        public User Authenticate(string userName, string password)
        {
            #region created object of class
            User objuserMaster = new User();
            #endregion

            #region used this decrypted key
            var decryptkey = "E546C8DF278CD5931069B522E695D4F2";
            #endregion

            #region
            try
            {
                #region selected user details by username or password
                var userdata = (from usrmst in _datacontext.MstUserTable
                                join rolemst in _datacontext.MstRoleTable on usrmst.FK_ROLE_ID equals rolemst.PK_ROLE_ID 
                                into rlmt
                                     from rolem in rlmt.DefaultIfEmpty()
                                select new
                                     {
                                            usrmst.PK_USER_ID,
                                            usrmst.FK_ROLE_ID,
                                            rolem.ROLE_NAME,
                                            usrmst.USER_NAME,
                                            usrmst.LOGIN_NAME,
                                            usrmst.EMAIL_ID,
                                            usrmst.PHONE_NO,
                                            usrmst.PASSWORD

                                }).ToList();
                #endregion

                #region checkk record avilable for that user name or password
                userdata = userdata.Where((o => (o.LOGIN_NAME == userName) && (EnDecryptOperation.Decrypt(o.PASSWORD, decryptkey) == (password)))).ToList();
                #endregion

                #region if user count zero then retrun below result
                if (userdata.Count == 0)
                {
                   
                    objuserMaster.resultMessage = "The username or password you entered is incorrect.";
                    return objuserMaster;
                }
                #endregion
                #region retrun valid user deta
                else
                {
                    var selectedUserDetail = userdata.First();
                        objuserMaster.userId = selectedUserDetail.PK_USER_ID;
                        objuserMaster.roleId = selectedUserDetail.FK_ROLE_ID;
                        objuserMaster.roleName = selectedUserDetail.ROLE_NAME;
                        objuserMaster.userName = selectedUserDetail.USER_NAME;
                        objuserMaster.loginName = selectedUserDetail.LOGIN_NAME;
                        objuserMaster.emailId = selectedUserDetail.EMAIL_ID;
                        objuserMaster.contactNumber = selectedUserDetail.PHONE_NO;
                        objuserMaster.resultMessage = "User Name is valid";
                        objuserMaster.tokenId = GetToken(selectedUserDetail.LOGIN_NAME, EnDecryptOperation.Decrypt(selectedUserDetail.PASSWORD, decryptkey))[0];
                }
                #endregion
            }
            catch (Exception ex)
            {
            }
            #endregion
            return objuserMaster;
        }
        #endregion

        #region genrated token using below method
        public string[] GetToken(string userName, string password)
        {
            #region set token expire time
            string tokentime = this._Configuration.GetSection("TokenSettings")["TokenExpirieTime"];
            #endregion

            #region Secret key which will be used later during validation
            string key = "ftc_secret_key_12345";
            #endregion

            #region normally this will be your site URL   
            var issuer = "http://mysite.com";
            #endregion

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            #region Create a List of Claims, Keep claims name short    
            var permClaims = new List<Claim>();
            permClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            permClaims.Add(new Claim("valid", "1"));
            permClaims.Add(new Claim("userid", userName));
            permClaims.Add(new Claim("name", password));
            #endregion

            #region Create Security Token object by giving required parameters    
            var token = new JwtSecurityToken(issuer, //Issure    
                            issuer,  //Audience    
                            permClaims,
                              expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(tokentime)),


                            signingCredentials: credentials);
            var RefreshToken = GenerateRefreshToken();
            string[] jwt_token = new string[2];
            jwt_token[0] = new JwtSecurityTokenHandler().WriteToken(token);
            #endregion

            #region
            TrnLogin objlogin = new TrnLogin
            {
                LOGIN_ID = userName,
                LOGIN_TIME = DateTime.Now,
                LOGOUT_TIME = null,
                DOMAIN = "FTC",
                DOMAIN_HOST_NAME = "",
                APP_SESSION_ID = "",
                TOKEN_ID = jwt_token[0],
                CREATED_DATE = DateTime.Now,
                ISACTIVE = 1,
                CREATED_BY = 1

            };
            _datacontext.TrnLoginTable.Add(objlogin);
            _datacontext.SaveChanges();
            #endregion

            return jwt_token;
        }
        #endregion
        #region refresh the token using below method 
        public User RefershTokens(int id)
        {
            #region created object of user class
            User objuserMaster = new User();
            #endregion

            #region set decrypt key 
            var decryptkey = "E546C8DF278CD5931069B522E695D4F2";
            #endregion

            #region 
            try
            {
                #region selected user details by username id
                var userdata = (from usrmst in _datacontext.MstUserTable
                                join rolemst in _datacontext.MstRoleTable on usrmst.FK_ROLE_ID equals rolemst.PK_ROLE_ID
                                into rlmt
                                from rolem in rlmt.DefaultIfEmpty()
                                select new
                                {
                                    usrmst.PK_USER_ID,
                                    usrmst.FK_ROLE_ID,
                                    rolem.ROLE_NAME,
                                    usrmst.USER_NAME,
                                    usrmst.LOGIN_NAME,
                                    usrmst.EMAIL_ID,
                                    usrmst.PHONE_NO,
                                    usrmst.PASSWORD

                                }).ToList();
                #endregion
                #region checkk record avilable for that user name or password
                userdata = userdata.Where((o => (o.PK_USER_ID == id))).ToList();//&& (EnDecryptOperation.Decrypt(o.PASSWORD, decryptkey) == (password)))).ToList();
                #endregion
                {
                    var selectedUserDetail = userdata.First();
                    objuserMaster.tokenId = GetToken(selectedUserDetail.LOGIN_NAME, EnDecryptOperation.Decrypt(selectedUserDetail.PASSWORD, decryptkey))[0];

                }
            }
            catch (Exception ex)
            {
               
            }
            #endregion
            return objuserMaster;
        }
        #endregion

        #region
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
        #endregion

        #region logout to portal using below methos
        public string Logout(User objUserMaster)
        {
            #region declared variable for retrun message
            string Result = string.Empty;
            #endregion

            #region
            try
            {
                #region checked token invalid or not
                TrnLogin objuser = (from login in _datacontext.TrnLoginTable
                                    where login.LOGIN_ID == objUserMaster.loginName && login.TOKEN_ID== objUserMaster.tokenId
                                    select login).FirstOrDefault();
                #endregion

                #region
                if (objuser != null)
                {
                    objuser.LOGOUT_TIME = DateTime.Now;
                    objuser.ISACTIVE = 0;
                    _datacontext.SaveChanges();
                    objUserMaster.resultMessage = "User logged out";
                }
                else
                {
                    objUserMaster.resultMessage = "Invalid token";
                }
                #endregion
            }
            catch (Exception ex)
            {
                return (ex.Message);
            }
            #endregion
            return objUserMaster.resultMessage;
        }
        #endregion

        #region password change of user using below method
        public string ChangePassword(UserChangePassword objUserMaster)
        {
            #region declared variable for retrun message
            string Result = string.Empty;
            #endregion

            #region set encrypt key or decrypt for password
            var decryptkey = "E546C8DF278CD5931069B522E695D4F2";
            var encryptkey = "E546C8DF278CD5931069B522E695D4F2";
            #endregion

            #region
            try
            {
                #region check password changed or invalid
                var data = (from login in _datacontext.MstUserTable
                            where login.PK_USER_ID == objUserMaster.userId
                            && login.IS_ACTIVE == 1
                            select login).ToList();
                if (objUserMaster.newPassword == objUserMaster.confirmPassword)
                {
                    MstUser objuser = data.Where(x => EnDecryptOperation.Decrypt(x.PASSWORD, decryptkey) == objUserMaster.oldPassword).FirstOrDefault();

                    if (objuser != null)
                    {
                        objuser.PASSWORD = EnDecryptOperation.Encrypt(objUserMaster.newPassword, encryptkey);
                        _datacontext.SaveChanges();
                        objUserMaster.resultMessage = "Password Changed Successfully!!!";
                    }
                    else
                    {
                        objUserMaster.resultMessage = "Old Password is Invalid!!!";
                    }
                }
                else 
                {
                    objUserMaster.resultMessage = "Please Check Password Entered in Confirm Box!!!";
                }
                #endregion

            }
            catch (Exception ex)
            {
               
            }
            #endregion
            return objUserMaster.resultMessage;
        }
        #endregion

        #region get user id for particular user using token
        public int GetUserDetailsFromToken(string token)
        {
            token = token.Replace("Bearer ", "");
            int userIdPK = 0;
           
            #region
            try
            {
                #region select user id from token
                userIdPK = (from trnlog in _datacontext.TrnLoginTable
                            from user in _datacontext.MstUserTable
                            where trnlog.TOKEN_ID.Equals(token) && trnlog.LOGIN_ID == user.LOGIN_NAME
                            select (int)(user.PK_USER_ID)).FirstOrDefault();
                #endregion
            }
            catch (Exception ex)
            {

            }
            #endregion

            return userIdPK;

        }
        #endregion

        #region checked user already loggined or not
        public string CheckUserAlreadyLoggined(string userName)
        {
            #region set return result
            string message = "";
            #endregion

            #region
            var userdata = (from usrmst in _datacontext.TrnLoginTable

                            where usrmst.LOGIN_ID == userName && usrmst.LOGIN_TIME <= DateTime.Now && usrmst.LOGOUT_TIME == null
                            select new
                            {
                                usrmst.LOGIN_ID,
                                usrmst.TOKEN_ID,
                            }).ToList();
            if(userdata.Count >0)
            {
                message = "user already loggined";
            }
            #endregion

            return message;
        }
        #endregion
    }
}
