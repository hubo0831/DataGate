using System;
namespace DataGate.Com.Logs
{
    public interface ILogManager
    {
        ILog GetLogger(string name);
    }
}
