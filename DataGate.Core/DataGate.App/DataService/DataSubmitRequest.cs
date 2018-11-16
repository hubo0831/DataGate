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
        /// <summary>
        /// 查询或修改的配置项的Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 新增的对象数组
        /// </summary>
        public JArray Added { get; set; }

        /// <summary>
        /// 移除的对象数组，为节约带宽，可以只传只有一个id主键的对象,
        /// 在有复合主键的情况下,只传复合主键所组成的对象
        /// </summary>
        public JArray Removed { get; set; }

        /// <summary>
        /// 修改过的对象数组
        /// </summary>
        public JArray Changed { get; set; }

        /// <summary>
        /// 明细表的增删改记录
        /// </summary>
        public IEnumerable<DataSubmitRequest> Details { get; set; }
            = new List<DataSubmitRequest>();
    }
}
