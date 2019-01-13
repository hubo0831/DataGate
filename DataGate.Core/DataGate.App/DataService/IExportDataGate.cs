using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DataGate.App.DataService
{
    /// <summary>
    /// 导出的过滤器
    /// </summary>
    public interface IExportDataGate : IDataGate
    {
        void OnExport(DataGateKey gkey, DataTable dt);
    }
}
