using System;
using System.Collections.Generic;

namespace DataGate.Com.Logs
{
    /// <summary>
    /// 系统日志数据实体类
    /// </summary>
    public class LogInfo : IId<int>
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 模块名或控制器名
        /// </summary>
        public string Module { get; set; }

        /// <summary>
        /// 操作方法名
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string ClientIP { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime OpTime { get; set; }

        /// <summary>
        /// 业务分类ID
        /// </summary>
        public string CatalogId { get; set; }

        /// <summary>
        /// 业务ID
        /// </summary>
        public string ObjectId { get; set; }

        /// <summary>
        /// 日志级别
        /// </summary>
        public LogType LogLevel { get; set; }

        /// <summary>
        /// 请求字符串
        /// </summary>
        public string Request { get; set; }

        /// <summary>
        /// 所花时间
        /// </summary>
        public Double Costs { get; set; }

        /// <summary>
        /// 简要信息
        /// </summary>
        public string Abstract { get; set; }

        /// <summary>
        /// 详细信息
        /// </summary>
        public string Message { get; set; }
        //public string Browser { get; set; }
        //public string BrowserVersion { get; set; }
        //public string Platform { get; set; }

        /// <summary>
        /// 浏览器标识
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public string Exception { get; set; }

        public override string ToString()
        {
            return Account + ":" + Message;
        }

    }
}
