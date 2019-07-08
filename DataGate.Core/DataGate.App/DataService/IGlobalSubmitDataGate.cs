using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DataGate.App.DataService
{
    /// <summary>
    /// 数据更改前后的总的过滤器,主·要用于重定义修改逻辑
    /// </summary>
    public interface IGlobalSubmitDataGate : IDataGate
    {
        void OnSubmit(DataGateKey gkey, DataSubmitRequest request);
    }
}
