using System;
using System.Collections.Generic;

namespace DataGate.Com.Logs
{
    /// <summary>
    /// 系统日志数据实体类
    /// </summary>
    public class LogInfo : IId<int>
    {
        public int Id { get; set; }

        public string ModuleName { get; set; }
        public string ActionName { get; set; }
        public string UserName { get; set; }
        public string ClientIP { get; set; }
        public DateTime OpTime { get; set; }
        public string CatalogId { get; set; }
        public string ObjectId { get; set; }
        public string LogType { get; set; }
        public string Request { get; set; }
        public Double Costs { get; set; }
        public string Message { get; set; }
        public string Browser { get; set; }
        public decimal BrowserVersion { get; set; }
        public string Platform { get; set; }

        public override string ToString()
        {
            return UserName + ":" + Message;
        }

    }
}
