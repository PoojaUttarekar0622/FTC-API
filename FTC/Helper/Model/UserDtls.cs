using System;
using System.Collections.Generic;
using Helper.Interface;
using Helper.Data;
using static Helper.Data.DataContextClass;
using static Helper.Model.LoginAuthClsDeclaration;
using Microsoft.Extensions.Configuration;
using System.Linq;
using static Helper.Model.EnDecryptOperation;

namespace Helper.Model
{
    public class UserDtls : IUser
    {
        public DataConnection _datacontext;
        private IConfiguration _Configuration;

        public UserDtls(DataConnection datacontext, IConfiguration configuration)
        {
            _datacontext = datacontext;
            _Configuration = configuration;
        }

        /// get single user details
        public User GetSingleUserDetail(string LoginName)
        {
            User objuserMaster = new User();
            var userdata = (from user in _datacontext.MstUserTable
                            where user.LOGIN_NAME == LoginName && user.IS_ACTIVE == 1
                            select new
                            {
                                user.PK_USER_ID,
                                user.LOGIN_NAME,
                                user.FK_ROLE_ID,
                                user.USER_NAME,
                                user.PHONE_NO,
                                user.PASSWORD,
                                user.EMAIL_ID,
                            }).ToList();
            if (userdata.Count > 0)
            {

                foreach (var data in userdata)
                {
                    objuserMaster.userId = data.PK_USER_ID;
                    objuserMaster.roleId = data.FK_ROLE_ID;
                    objuserMaster.userName = data.USER_NAME;
                    objuserMaster.loginName = data.LOGIN_NAME;
                    objuserMaster.password = data.PASSWORD; //Decrypt(data.pass, aes.Key, aes.IV);
                    objuserMaster.emailId = data.EMAIL_ID;
                    objuserMaster.contactNumber = data.PHONE_NO;

                }
            }
            return objuserMaster;
        }

        /// delete single user
        public Message DeleteSingleUser(long userid)
        {
            User objuserMaster = new User();
            Message obj = new Message();
            string Result = string.Empty;
            try
            {

                MstUser objuser = (from user in _datacontext.MstUserTable
                                   where user.PK_USER_ID == userid && user.IS_ACTIVE == 1
                                   select user).ToList().SingleOrDefault();
                if (objuser != null)
                {

                    _datacontext.MstUserTable.Remove(objuser);
                    _datacontext.SaveChanges();
                    objuserMaster.resultMessage = "Data deleted successfully";
                }
            }
            catch (Exception ex)
            {
                Result = ex.Message;
            }
            return obj;
        }

        /// save user data
        public Message SaveUser(User objUserMaster)
        {
            // string Result = string.Empty;
            Message obj = new Message();
            var key = "E546C8DF278CD5931069B522E695D4F2";

            try
            {
                var sameEntry = _datacontext.MstUserTable.Where(u => u.LOGIN_NAME == objUserMaster.loginName).ToList();
                if (sameEntry.Count() > 0)
                {
                    obj.result = "Same User Exist";
                }
                else
                {
                    MstUser objuser = new MstUser();
                    objuser.LOGIN_NAME = objUserMaster.loginName;
                    //   byte[] encrypted = Encrypt(raw, aes.Key, aes.IV);
                    //  string passe = System.Text.Encoding.UTF8.GetString(Cryptographys);
                    objuser.PASSWORD = EnDecryptOperation.Encrypt(objUserMaster.password, key); //objUserMaster.password;//Encrypt(objUserMaster.password, aes.Key, aes.IV); //System.Text.Encoding.UTF8.GetString//EncryptStringToBytes_Aes(objUserMaster.password, myAes.Key, myAes.IV).ToString();
                    objuser.USER_NAME = objUserMaster.userName;
                    objuser.EMAIL_ID = objUserMaster.emailId;
                    objuser.PHONE_NO = objUserMaster.contactNumber;
                    // objuser.roleId_FK = objUserMaster.roleId;
                    objuser.CREATED_BY = 1;// Convert.ToInt32(objUserMaster.userId);
                    objuser.CREATED_DATE = DateTime.Now;
                    objuser.IS_ACTIVE = 1;
                    _datacontext.MstUserTable.Add(objuser);
                    _datacontext.SaveChanges();
                    obj.result = "Data Save Successfully";
                }
            }
            catch (Exception ex)
            {
                obj.result = ex.Message;
            }
            return obj;
        }

        public List<User> GetAllUserDetail()
        {
            List<User> lstuserMaster = new List<User>();
            var userdata = (from user in _datacontext.MstUserTable
                            where user.IS_ACTIVE == 1
                            select new
                            {
                                user.PK_USER_ID,
                                user.LOGIN_NAME,
                                user.FK_ROLE_ID,
                                user.USER_NAME,
                                user.PHONE_NO,
                                user.PASSWORD,
                                user.EMAIL_ID,
                            }).ToList();
            if (userdata.Count > 0)
            {
                foreach (var data in userdata)
                {
                    User objuserMaster = new User();
                    objuserMaster.userId = data.PK_USER_ID;
                    objuserMaster.roleId = data.FK_ROLE_ID;
                    objuserMaster.userName = data.USER_NAME;
                    objuserMaster.loginName = data.LOGIN_NAME;
                    objuserMaster.password = data.PASSWORD;
                    objuserMaster.emailId = data.EMAIL_ID;
                    objuserMaster.contactNumber = data.PHONE_NO;
                    lstuserMaster.Add(objuserMaster);
                }
            }
            return lstuserMaster;
        }

        public string UpdateUserDetails(User objUserMaster)
        {
            string Result = string.Empty;
            try
            {
                MstUser objuser = (from user in _datacontext.MstUserTable
                                   where user.PK_USER_ID == objUserMaster.userId
                                   select user).ToList().SingleOrDefault();
                objuser.LOGIN_NAME = objUserMaster.userName;
                objuser.PASSWORD = objUserMaster.password;//obj.EncryptStringToBytes_Aes(objUserMaster.password, myAes.Key, myAes.IV).ToString();
                objuser.USER_NAME = objUserMaster.userName;
                objuser.EMAIL_ID = objUserMaster.emailId;
                objuser.PHONE_NO = objUserMaster.contactNumber;
                objuser.FK_ROLE_ID = objUserMaster.roleId;
                //  objuser.createdBy = objUserMaster.createdBy;
                objuser.CREATED_DATE = DateTime.Now;
                objuser.IS_ACTIVE = 1;
                _datacontext.MstUserTable.Update(objuser);
                _datacontext.SaveChanges();
                Result = "Data Update Successfully";
            }
            catch (Exception ex)
            {
                Result = ex.Message;
            }
            return objUserMaster.result;
        }

        public int GetUserDetailsFromToken(string token)
        {
            token = token.Replace("Bearer ", "");
            int userIdPK = 0;
            try
            {
                userIdPK = (from trnlog in _datacontext.TrnLoginTable
                            from user in _datacontext.MstUserTable
                            where trnlog.TOKEN_ID.Equals(token) && trnlog.LOGIN_ID == user.LOGIN_NAME
                            select (int)(user.PK_USER_ID)).FirstOrDefault();

            }
            catch (Exception ex)
            {

            }
            return userIdPK;

        }

        public User GetUserIDandNameFromToken(string token)
        {
            token = token.Replace("Bearer ", "");
            User objuserMaster = new User();

            var userdata = (from trnlog in _datacontext.TrnLoginTable
                            from user in _datacontext.MstUserTable
                            where trnlog.TOKEN_ID.Equals(token) && trnlog.LOGIN_ID == user.LOGIN_NAME
                            select new
                            {
                                user.PK_USER_ID,
                                user.LOGIN_NAME,
                                user.USER_NAME

                            }).FirstOrDefault();
            if (userdata != null)
            {
                objuserMaster.userId = userdata.PK_USER_ID;
                objuserMaster.loginName = userdata.LOGIN_NAME;
                objuserMaster.userName = userdata.USER_NAME;

            }
            return objuserMaster;
        }




    }
}
