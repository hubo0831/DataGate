using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGate.App
{
    /// <summary>
    /// 登录数据请求
    /// </summary>
    public class LoginRequest
    {
        public string Account { get; set; }

        public string Password { get; set; }

        public string AppSecret { get; set; }
    }
}
