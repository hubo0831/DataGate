using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGate.Com
{
    /// <summary>
    /// 统一的数据管理类，提供对类型T数据列表的增删改。
    /// </summary>
    public class DataManager<T> : DataManagerBase<T, string> where T : class,IIdName<string>
    {
    }
}