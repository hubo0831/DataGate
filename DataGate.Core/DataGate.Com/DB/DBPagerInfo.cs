using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data;

namespace DataGate.Com.DB
{
    /// <summary>
    /// 分页类,定义单表查询或查询条件只来自主表的分页查询信息
    /// </summary>
    public class DBPagerInfo : PagerInfo
    {
        /// <summary>
        /// 不分页的查询语句
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// 排序语句
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// 主键字段
        /// </summary>
        public string KeyId { get; set; }
    }
}
