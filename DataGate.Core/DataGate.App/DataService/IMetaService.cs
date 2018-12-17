using DataGate.Com.DB;
using System;

namespace DataGate.App.DataService
{
    public interface IMetaService
    {
        DataGateKey GetDataKey(string key);
        TableMeta GetTableMeta(string arrayItemType);
    }
}