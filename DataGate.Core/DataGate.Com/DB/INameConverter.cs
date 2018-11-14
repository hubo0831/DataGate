using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGate.Com.DB
{
    /// <summary>
    /// 将对象中的属性名称按某种规则映射成数据库中字段名称
    /// </summary>
    public interface INameConverter
    {
        /// <summary>
        /// 转换方法
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string ConvertToDBName(string name);

    }
}
