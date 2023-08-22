using System;
using System.Collections.Generic;
using System.Text;
using static Helper.Model.LoginAuthClsDeclaration;

namespace Helper.Interface
{
    public interface ILoginAuth
    {
        #region
        User Authenticate(string userName, string password);
        string[] GetToken(string userName, string password);
        string Logout(User objUserMaster);
        string ChangePassword(UserChangePassword objUserMaster);
        int GetUserDetailsFromToken(string token);
        string GenerateRefreshToken();

        public User RefershTokens(int id);
     
        #endregion
    }
}
