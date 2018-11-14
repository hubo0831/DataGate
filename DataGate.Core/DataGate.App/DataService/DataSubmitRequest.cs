using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataGate.App.DataService
{
    /// <summary>
    /// 批量增删改的数据请求
    /// </summary>
    public class DataSubmitRequest
    {
        public string Key { get; set; }

        public JArray Added { get; set; }

        public JArray Removed { get; set; }

        public JArray Changed { get; set; }

        public IEnumerable<DataSubmitRequest> Details { get; set; }
            = new List<DataSubmitRequest>();
    }
}
