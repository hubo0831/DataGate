using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGate.Com
{
    /// <summary>
    /// 有Id和Name的实体抽象类
    /// </summary>
    public abstract class IdName<T> : IIdName<T>
    {
        /// <summary>
        /// 字符串Id主键
        /// </summary>
        public T Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
    }
}
