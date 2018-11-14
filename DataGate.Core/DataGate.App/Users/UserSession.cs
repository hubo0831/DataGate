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
        public string Token { get; set; }
        public string Id { get; set; }

        public DateTime LastOpTime { get; set; }
    }
}