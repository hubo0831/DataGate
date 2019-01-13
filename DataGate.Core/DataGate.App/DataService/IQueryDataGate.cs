using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DataGate.App.DataService
{
    /// <summary>
    /// 查询结果数据过滤接口
    /// </summary>
    public interface IQueryDataGate: IDataGate
    {
        /// <summary>
        /// 过滤接口
        /// </summary>
        /// <param name="gkey"></param>
        /// <param name="result"></param>
        void OnResult(DataGateKey gkey, object result);
    }
}
