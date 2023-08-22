using System;
using System.Collections.Generic;
using System.Text;
namespace Helper.Model
{
   public class LoginAuthClsDeclaration
    {
        public class User
        {
            public long userId { get; set; }
            public long roleId { get; set; }
            public string userName { get; set; }
            public string roleName { get; set; }
            public string loginName { get; set; }
            public string password { get; set; }
            public string emailId { get; set; }
            public string contactNumber { get; set; }
            public string resultMessage { get; set; }
            public string tokenId { get; set; }
            public string refreshTokenID { get; set; }
            public string result { get; set; }
            public int? createdBy { get; set; }
            public DateTime? createdDate { get; set; }
            public int isActive { get; set; }
        }
        public class UserChangePassword
        {
            public long userId { get; set; }
            public string userName { get; set; }
            public string oldPassword { get; set; }
            public string newPassword { get; set; }
            public string confirmPassword { get; set; }
            public string resultMessage { get; set; }
        }
        public class LoginData
        {
            public long loginId { get; set; }
            public string loginID { get; set; }
            public DateTime loginTime { get; set; }
            public DateTime logOutTime { get; set; }
            public string tokenID { get; set; }
            public string domain { get; set; }
            public string appSessionId { get; set; }
            public string domainHostName { get; set; }
            public int? createdBy { get; set; }
            public DateTime? createdAt { get; set; }
        }
        public class Message
        {
            public string result { get; set; }
        }
    }
}
