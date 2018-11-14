using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DataGate.App.DataService
{
    public interface IDataGate
    {
        void OnAdd(List<string> fields, IDictionary<string, object> ps);
        void OnChange(List<string> fields, IDictionary<string, object> ps);
        void OnRemove(IDictionary<string, object> ps);
    }
}
