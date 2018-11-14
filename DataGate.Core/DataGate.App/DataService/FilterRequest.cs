using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataGate.App.DataService
{
    /// <summary>
    /// 查询请求
    /// </summary>
    public class FilterRequest
    {
        [JsonProperty("n")]
        public string Name { get; set; }

        [JsonProperty("o")]
        public string Operator { get; set; }

        [JsonProperty("v")]
        public object Value { get; set; }

        [JsonProperty("v1")]
        public object Value1 { get; set; }
    }
}
