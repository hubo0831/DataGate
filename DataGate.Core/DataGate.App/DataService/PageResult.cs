using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DataGate.App.DataService
{
    /// <summary>
    /// 分页查询的结果
    /// </summary>
    public class PageResult
    {
        /// <summary>
        /// 总数
        /// </summary>
       [JsonProperty("total")]
        public int Total { get; set; }

        /// <summary>
        /// 当前页数据
        /// </summary>
        [JsonProperty("data")]
        public object Data { get; set; }
    }
}
