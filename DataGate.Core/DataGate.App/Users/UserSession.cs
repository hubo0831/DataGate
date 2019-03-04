using DataGate.App.Models;
using Newtonsoft.Json;
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
        public string Id { get; set; }

        private Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// 获取或设置会话临时数据
        /// </summary>
        /// <param name="key">数据的键</param>
        /// <returns></returns>、
        [JsonIgnore]
        public object this[string key]
        {
            get
            {
                if (Data.ContainsKey(key))
                    return Data[key];
                return null;
            }
            set
            {
                Data[key] = value;
            }
        }

        /// <summary>
        /// 移除相关会话数据
        /// </summary>
        /// <param name="key"></param>
        public void RemoveData(string key)
        {
            Data.Remove(key);
        }
    }
}