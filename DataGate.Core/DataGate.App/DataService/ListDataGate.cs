using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DataGate.App.DataService
{
    /// <summary>
    /// 内部用于管理DataGate的对象
    /// </summary>
    class ListDataGate : ISubmitDataGate, IQueryDataGate, IExportDataGate
    {
         List<IDataGate> _dataGates { get; set; } 
        public ListDataGate(List<IDataGate> datagates)
        {
            _dataGates = datagates;
        }

        public void OnAdd(List<string> fields, IDictionary<string, object> ps)
        {
            foreach (var dg in _dataGates.OfType<ISubmitDataGate>())
            {
                dg.OnAdd(fields, ps);
            }
        }

        public void OnChange(List<string> fields, IDictionary<string, object> ps)
        {
            foreach (var dg in _dataGates.OfType<ISubmitDataGate>())
            {
                dg.OnChange(fields, ps);
            }
        }

        public void OnRemove(IDictionary<string, object> ps)
        {
            foreach (var dg in _dataGates.OfType<ISubmitDataGate>())
            {
                dg.OnRemove(ps);
            }
        }

        public void OnExport(DataGateKey gkey, DataTable dt)
        {
            foreach (var dg in _dataGates.OfType<IExportDataGate>())
            {
                dg.OnExport(gkey, dt);
            }
        }

        public void OnResult(DataGateKey gkey, object result)
        {
            foreach (var dg in _dataGates.OfType<IQueryDataGate>())
            {
                dg.OnResult(gkey, result);
            }
        }
    }
}
