using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGate.App
{
    /// <summary>
    /// 定义常见错误信息
    /// </summary>
    public static class MSG
    {
        #region 登录
        public static LoginResult UserNotExists = new LoginResult
        {
            Code = 1001,
            Message = "用户不存在"
        };

        public static LoginResult PasswordError = new LoginResult
        {
            Code = 1002,
            Message = "密码错误"
        };

        public static LoginResult NotLogined = new LoginResult
        {
            Code = 1003,
            Message = "尚未登录"
        };

        public static UserInfoResult SessionExpired = new UserInfoResult
        {
            Code = 1010,
            Message = "Token已过期"
        };
        #endregion


    }
}
