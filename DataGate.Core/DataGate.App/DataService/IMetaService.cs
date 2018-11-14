namespace DataGate.App.DataService
{
    public interface IMetaService
    {
        DataGateKey GetDataKey(string key);
        void RegisterDataGate(string key, IDataGate dataGate);
        TableMeta GetTableMeta(string arrayItemType);
    }
}