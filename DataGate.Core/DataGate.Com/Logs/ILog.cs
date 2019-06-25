using System;
namespace DataGate.Com.Logs
{
    public interface ILog
    {
        void Flush();
        void Write(LogInfo logInfo, Exception ex);
    }
}
