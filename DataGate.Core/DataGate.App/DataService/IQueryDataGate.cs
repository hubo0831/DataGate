using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DataGate.App.DataService
{
    /// <summary>
    /// 查询前和查询后结果数据过滤接口
    /// </summary>
    public interface IQueryDataGate: IDataGate
    {
        /// <summary>
        /// 查询准备
        /// </summary>
        /// <param name="gkey"></param>
        /// <param name="param"></param>
        void OnQuery(DataGateKey gkey, IDictionary<string, object> param);

        /// <summary>
        /// 结果过滤
        /// </summary>
        /// <param name="gkey"></param>
        /// <param name="result"></param>
        void OnResult(DataGateKey gkey, object result);
    }
}
