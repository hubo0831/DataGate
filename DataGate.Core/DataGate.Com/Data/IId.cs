using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGate.Com
{
    /// <summary>
    /// 具有ID属性的数据实体接口
    /// </summary>
    /// <typeparam name="T">ID属性的数据类型</typeparam>
    public interface IId<T> 
    {
        T Id { get; set; }
    }

}
