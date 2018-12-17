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
        /// 将属性Pascal或Camel命名方式转为全大写，并添加下划线在原来大写字母前面
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        string ToDBName(string propName);

        /// <summary>
        /// 将带下划分隔符风格的数据库大写属性转为camel命名方式，并去掉下划分隔符
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        string ToPropName(string dbName);

    }
}
