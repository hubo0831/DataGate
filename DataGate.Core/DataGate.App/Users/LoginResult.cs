using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGate.App
{
    /// <summary>
    /// 登录结果数据协定
    /// </summary>
    public class LoginResult:ApiResult
    {
        public string Token { get; set; }

        /// <summary>
        /// 过期时间（分钟）
        /// </summary>
        public int ExpireIn { get; set; }

        /// <summary>
        /// 当用户勾选记住我时，返回客户端的记住我加密信息
        /// </summary>
        public string Remember { get; set; }
    }
}
