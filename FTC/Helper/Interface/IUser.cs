using System;
using System.Collections.Generic;
using System.Text;
using static Helper.Model.LoginAuthClsDeclaration;

namespace Helper.Interface
{
   public interface IUser
    {
        #region
        User GetSingleUserDetail(string LoginName);
        Message DeleteSingleUser(long userid);
        Message SaveUser(User objUserMaster);
        List<User> GetAllUserDetail();
        string UpdateUserDetails(User objUserMaster);
        int GetUserDetailsFromToken(string token);
        User GetUserIDandNameFromToken(string token);
        #endregion
    }
}
