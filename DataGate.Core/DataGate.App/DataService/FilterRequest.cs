using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataGate.App.DataService
{
    /// <summary>
    /// 查询筛选请求
    /// </summary>
    public class FilterRequest
    {
        /// <summary>
        /// 字段（属性名）
        /// </summary>
        [JsonProperty("n")]
        public string Name { get; set; }

        /* 
*/
        /// <summary>
        /// 比较操作符
        ///<para>  e 相等 </para>
        ///  <para>  de 日期相等 判断日期相等，日期相等比较特殊，很难精确相等，
        ///    因此转成只判断是否在当天</para>
        ///   <para>   ne 不相等 !=////in 包含于 IN子句</para>
        ///   <para>   nin 不包含于，相当于NOT IN子句</para>
        ///   <para>   i 字符串匹配，相当于 LIKE</para>
        ///   <para>   ni 字符串不匹配 相当于 NOT LIKE</para>
        ///   <para>   lte 小于等于 &lt;=</para>
        ///   <para>   gte 大于等于 &gt;=</para>
        ///   <para>   bt 在两者之间 相当于 BETWEEN</para>
        /// </summary>
        [JsonProperty("o")]
        public string Operator { get; set; }

        /// <summary>
        /// 比较的值
        /// </summary>
        [JsonProperty("v")]
        public object Value { get; set; }

        /// <summary>
        /// 比较的值2，在范围查询时有用
        /// </summary>
        [JsonProperty("v1")]
        public object Value1 { get; set; }
    }
}
