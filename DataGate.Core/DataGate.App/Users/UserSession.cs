using DataGate.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGate.App
{
    /// <summary>
    /// 用户会话信息
    /// </summary>
    public class UserSession
    {
        /// <summary>
        /// 会话的Token,定期刷新
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 用户的账户名
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 用户最近一次的操作时间
        /// </summary>
        public DateTime LastOpTime { get; set; }

        /// <summary>
        /// 用户的ID
        /// </summary>
        public string Id { get; internal set; }
    }
}