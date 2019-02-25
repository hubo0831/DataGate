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
        /// <summary>
        /// 用户不存在
        /// </summary>
        public static LoginResult UserNotExists = new LoginResult
        {
            Code = 1001,
            Message = "用户不存在"
        };

        /// <summary>
        /// 密码错误
        /// </summary>
        public static LoginResult PasswordError = new LoginResult
        {
            Code = 1002,
            Message = "密码错误"
        };

        /// <summary>
        /// 尚未登录
        /// </summary>
        public static LoginResult NotLogined = new LoginResult
        {
            Code = 1003,
            Message = "尚未登录"
        };

        /// <summary>
        /// 验证码不对
        /// </summary>
        public static LoginResult WrongCode = new LoginResult
        {
            Code = 1004,
            Message = "验证码不对"
        };

        /// <summary>
        /// Token已过期
        /// </summary>
        public static UserInfoResult SessionExpired = new UserInfoResult
        {
            Code = 1010,
            Message = "Token已过期"
        };
        #endregion

        #region 杂项
        /// <summary>
        /// 该功能尚未实现
        /// </summary>
        public static ApiResult NotImplemented = new ApiResult
        {
            Code = 1099,
            Message = "该功能尚未实现"
        };

        /// <summary>
        /// 操作过于频繁
        /// </summary>
        public static ApiResult TooOften = new ApiResult
        {
            Code = 1098,
            Message = "操作过于频繁"
        };
       #endregion

    }
}
