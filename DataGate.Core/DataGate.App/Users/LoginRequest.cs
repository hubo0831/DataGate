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
        /// <summary>
        /// 用户账号
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 用户密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 应用程序的Secret,用于将来支持多个系统登录
        /// </summary>
        public string AppSecret { get; set; }

        /// <summary>
        /// 是否记住登录信息，下次直接登录 
        /// 当此信息为空表示不记住，当此字段为'1'时表示要求记住，当此字段为长字符串时表示是向服务器传递上次登录信息
        /// </summary>
         public string Remember { get; set; }
    }
}
