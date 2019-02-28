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
    class ListDataGate : ISubmitDataGate, ISubmitedDataGate, IQueryDataGate, IExportDataGate
    {
        List<IDataGate> _dataGates { get; set; }
        public ListDataGate(List<IDataGate> datagates)
        {
            _dataGates = datagates.GroupBy(g => g.GetType())
                .Select(g => g.First()).ToList();
        }

        public void OnAdd(DataGateKey gkey, IDictionary<string, object> ps)
        {
            foreach (var dg in _dataGates.OfType<ISubmitDataGate>())
            {
                dg.OnAdd(gkey, ps);
            }
        }

        public void OnChange(DataGateKey gkey, IDictionary<string, object> ps)
        {
            foreach (var dg in _dataGates.OfType<ISubmitDataGate>())
            {
                dg.OnChange(gkey, ps);
            }
        }

        public void OnRemove(DataGateKey gkey, IDictionary<string, object> ps)
        {
            foreach (var dg in _dataGates.OfType<ISubmitDataGate>())
            {
                dg.OnRemove(gkey, ps);
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

        public void OnQuery(DataGateKey gkey, IDictionary<string, object> param)
        {
            foreach (var dg in _dataGates.OfType<IQueryDataGate>())
            {
                dg.OnQuery(gkey, param);
            }
        }

        public void OnAdded(DataGateKey gkey, IDictionary<string, object> param)
        {
            foreach (var dg in _dataGates.OfType<ISubmitedDataGate>())
            {
                dg.OnAdded(gkey, param);
            }
        }

        public void OnChanged(DataGateKey gkey, IDictionary<string, object> param)
        {
            foreach (var dg in _dataGates.OfType<ISubmitedDataGate>())
            {
                dg.OnChanged(gkey, param);
            }
        }

        public void OnRemoved(DataGateKey gkey, IDictionary<string, object> param)
        {
            foreach (var dg in _dataGates.OfType<ISubmitedDataGate>())
            {
                dg.OnRemoved(gkey, param);
            }
        }
    }
}
