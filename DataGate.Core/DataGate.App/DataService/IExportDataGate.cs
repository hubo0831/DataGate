using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DataGate.App.DataService
{
    /// <summary>
    /// Excel导出的过滤器
    /// </summary>
    public interface IExportDataGate : IDataGate
    {
        /// <summary>
        /// 导出前处理目标DataTable中的数据
        /// </summary>
        /// <param name="gkey">数据访问配置</param>
        /// <param name="dt">导出的数据DataTable</param>
        void OnExport(DataGateKey gkey, DataTable dt);
    }
}
