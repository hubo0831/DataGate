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
    }
}
