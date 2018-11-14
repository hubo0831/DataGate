using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data;

namespace DataGate.Com.DB
{
    /// <summary>
    /// 父子表的分页类
    /// </summary>
    public class MasterDetailPagerInfo : PagerInfo
    {
        /// <summary>
        /// 当涉及到父子表的查询时，需指定表名和连接条件子句，如table1 inner join table2
        /// </summary>
        public string TablesAndJoins { get; set; }

        /// <summary>
        /// 当父子表查询时，指定where后面的条件子句
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// 排序语句，如"CreateTime DESC, ID ASC"
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// 当指定了TablesAndJoins属性时，必须同时指定Fields属性
        /// </summary>
        public string Fields { get; set; }

        /// <summary>
        /// 主键字段,如"ID"
        /// </summary>
        public string KeyId { get; set; }
    }
}
