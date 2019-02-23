using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DataGate.App.DataService
{
    /// <summary>
    /// 数据更改后的过滤器,主要用于修改后的额外操作
    /// </summary>
    public interface ISubmitedDataGate:IDataGate
    {
        void OnAdded(DataGateKey gkey, IDictionary<string, object> ps);
        void OnChanged(DataGateKey gkey,  IDictionary<string, object> ps);
        void OnRemoved(DataGateKey gkey, IDictionary<string, object> ps);
    }
}
